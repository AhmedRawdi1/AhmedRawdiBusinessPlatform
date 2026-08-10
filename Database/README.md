# ARBP database deployment

Database changes are versioned with the application. Do not edit production procedures manually.

## Deployment order

1. Take and verify a full database backup.
2. Run `Migrations/ApplyCoreSecurityBestPractices.sql`.
3. Deploy the files in `StoredProcedures` with `sqlcmd -b`.
4. For a legacy database only, set `ARBP_MIGRATION_CONNECTION` and `ARBP_LEGACY_PASSPHRASE`, then run `Tools/MigrateLegacyPasswords.ps1`.
5. Run `Migrations/FinalizePasswordHashMigration.sql` to remove the reversible password column.
6. Provision a dedicated SQL login outside source control, then run `Migrations/GrantApplicationLeastPrivilege.sql`.
7. Run `Tests/VerifyCoreAdministrationProcedures.sql`.
8. Run `Migrations/ApplyMasterDataManagement.sql` from the repository root with `sqlcmd -b`.
9. Run `Tests/VerifyMasterDataCrud.sql`; its test transaction is always rolled back.

The application connection string belongs in .NET User Secrets for local development and a managed secret store or environment variable in deployed environments. Never commit database passwords.

## Security model

- Passwords use salted PBKDF2-HMAC-SHA256 hashes with 310,000 iterations.
- The application login has execute access only to the procedures used by the application.
- User and group identifiers use SQL sequences rather than `MAX(ID) + 1`.
- Administrative writes use explicit transactions and parameterized calls.
- Reversible password encryption and hard-coded passphrases are not supported.
