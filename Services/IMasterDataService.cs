using AhmedRawdiBusinessPlatform.Models;

namespace AhmedRawdiBusinessPlatform.Services;

public interface IMasterDataService
{
    Task<IReadOnlyList<MasterDataRecordDto>> SearchAsync(string entityKey, string? search, CancellationToken cancellationToken = default);
    Task<string?> GetRecordJsonAsync(string entityKey, long id, CancellationToken cancellationToken = default);
    Task<long> SaveAsync(string entityKey, long? id, string payloadJson, long? userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(string entityKey, long id, long? userId, CancellationToken cancellationToken = default);
}
