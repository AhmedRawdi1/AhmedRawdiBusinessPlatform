using System.Data;
using AhmedRawdiBusinessPlatform.Data;
using AhmedRawdiBusinessPlatform.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AhmedRawdiBusinessPlatform.Services;

public sealed class MasterDataService : IMasterDataService
{
    private readonly ApplicationDbContext _context;

    public MasterDataService(ApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<MasterDataRecordDto>> SearchAsync(string entityKey, string? search, CancellationToken cancellationToken = default)
    {
        var entity = new SqlParameter("@EntityKey", SqlDbType.VarChar, 50) { Value = entityKey };
        var term = new SqlParameter("@Search", SqlDbType.NVarChar, 200) { Value = (object?)search ?? DBNull.Value };
        return await _context.Database.SqlQueryRaw<MasterDataRecordDto>(
            "EXEC dbo.usp_MasterData_Search @EntityKey = @EntityKey, @Search = @Search", entity, term)
            .ToListAsync(cancellationToken);
    }

    public async Task<string?> GetRecordJsonAsync(string entityKey, long id, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.usp_MasterData_GetRecord";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@EntityKey", SqlDbType.VarChar, 50) { Value = entityKey });
        command.Parameters.Add(new SqlParameter("@ID", SqlDbType.BigInt) { Value = id });
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null or DBNull ? null : Convert.ToString(result);
    }

    public async Task<long> SaveAsync(string entityKey, long? id, string payloadJson, long? userId, CancellationToken cancellationToken = default)
    {
        var savedId = new SqlParameter("@SavedID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
        await _context.Database.ExecuteSqlRawAsync("EXEC dbo.usp_MasterData_Save @EntityKey=@EntityKey,@ID=@ID,@Payload=@Payload,@RegUserID=@RegUserID,@SavedID=@SavedID OUTPUT",
            [new SqlParameter("@EntityKey", entityKey), new SqlParameter("@ID", (object?)id ?? DBNull.Value), new SqlParameter("@Payload", payloadJson), new SqlParameter("@RegUserID", (object?)userId ?? DBNull.Value), savedId], cancellationToken);
        return Convert.ToInt64(savedId.Value);
    }

    public Task DeleteAsync(string entityKey, long id, long? userId, CancellationToken cancellationToken = default) =>
        _context.Database.ExecuteSqlRawAsync("EXEC dbo.usp_MasterData_Delete @EntityKey=@EntityKey,@ID=@ID,@CancelUserID=@CancelUserID",
            [new SqlParameter("@EntityKey", entityKey), new SqlParameter("@ID", id), new SqlParameter("@CancelUserID", (object?)userId ?? DBNull.Value)], cancellationToken);
}
