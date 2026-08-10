using System.Collections.Generic;
using System.Threading.Tasks;
using AhmedRawdiBusinessPlatform.Models;

namespace AhmedRawdiBusinessPlatform.Services
{
    public interface IPermissionService
    {
        Task<List<UserPermissionDto>> GetUserPermissionsAsync(long? userId, long? groupId = null);
        Task<NavigationMenuViewModel> GetNavigationMenuAsync(long? userId, long? groupId = null);
        Task SaveUserPermissionsAsync(long userId, long? groupId, string permissionsJson, long? registeredUserId = null);
    }
}
