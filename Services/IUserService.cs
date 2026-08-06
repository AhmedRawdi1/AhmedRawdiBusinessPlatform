using System.Collections.Generic;
using System.Threading.Tasks;
using AhmedRawdiBusinessPlatform.Models;

namespace AhmedRawdiBusinessPlatform.Services
{
    public interface IUserService
    {
        Task<IReadOnlyList<UserListItemDto>> GetAllUsersAsync();
        Task<long> SaveUserAsync(SaveUserDto model, long? registeredBy = null);
        Task DeleteUserAsync(long userId);
    }
}
