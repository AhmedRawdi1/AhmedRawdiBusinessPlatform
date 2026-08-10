$ErrorActionPreference = 'Stop'

$connectionString = [Environment]::GetEnvironmentVariable('ARBP_MIGRATION_CONNECTION')
$legacyPassphrase = [Environment]::GetEnvironmentVariable('ARBP_LEGACY_PASSPHRASE')

if ([string]::IsNullOrWhiteSpace($connectionString) -or [string]::IsNullOrWhiteSpace($legacyPassphrase)) {
    throw 'Set ARBP_MIGRATION_CONNECTION and ARBP_LEGACY_PASSPHRASE before running this migration.'
}

$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()
$transaction = $connection.BeginTransaction()

try {
    $read = $connection.CreateCommand()
    $read.Transaction = $transaction
    $read.CommandText = @'
SELECT ID, CONVERT(nvarchar(256), DECRYPTBYPASSPHRASE(@LegacyPassphrase, UserPass)) AS PlainPassword
FROM dbo.SystemUsers WITH (UPDLOCK, HOLDLOCK)
WHERE PasswordHash IS NULL AND UserPass IS NOT NULL;
'@
    [void]$read.Parameters.Add('@LegacyPassphrase', [System.Data.SqlDbType]::NVarChar, 256)
    $read.Parameters['@LegacyPassphrase'].Value = $legacyPassphrase

    $reader = $read.ExecuteReader()
    $users = [System.Collections.Generic.List[object]]::new()
    while ($reader.Read()) {
        if (-not $reader.IsDBNull(1)) {
            $users.Add([pscustomobject]@{ Id = $reader.GetInt64(0); Password = $reader.GetString(1) })
        }
    }
    $reader.Close()

    foreach ($user in $users) {
        $salt = New-Object byte[] 16
        $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
        try { $rng.GetBytes($salt) } finally { $rng.Dispose() }

        $derive = [System.Security.Cryptography.Rfc2898DeriveBytes]::new(
            $user.Password, $salt, 310000, [System.Security.Cryptography.HashAlgorithmName]::SHA256)
        try { $hashBytes = $derive.GetBytes(32) } finally { $derive.Dispose() }

        $passwordHash = '$pbkdf2-sha256$310000$' + [Convert]::ToBase64String($salt) + '$' + [Convert]::ToBase64String($hashBytes)
        $update = $connection.CreateCommand()
        $update.Transaction = $transaction
        $update.CommandText = 'UPDATE dbo.SystemUsers SET PasswordHash=@Hash, PasswordChangedAt=SYSUTCDATETIME(), UserPass=NULL WHERE ID=@ID;'
        [void]$update.Parameters.Add('@Hash', [System.Data.SqlDbType]::NVarChar, 512)
        $update.Parameters['@Hash'].Value = $passwordHash
        [void]$update.Parameters.Add('@ID', [System.Data.SqlDbType]::BigInt)
        $update.Parameters['@ID'].Value = $user.Id
        [void]$update.ExecuteNonQuery()

        $user.Password = $null
        $passwordHash = $null
    }

    $transaction.Commit()
    Write-Output "Migrated users: $($users.Count)"
}
catch {
    if ($transaction.Connection) { $transaction.Rollback() }
    throw
}
finally {
    $legacyPassphrase = $null
    $connection.Dispose()
}
