using AhmedRawdiBusinessPlatform.Models;

namespace AhmedRawdiBusinessPlatform.Services
{
    public interface IGroupService
    {
        Task<IReadOnlyList<GroupListItemDto>> GetAllGroupsAsync();
        Task<IReadOnlyList<GroupFormPermissionDto>> GetGroupPermissionsAsync(long groupId);
        Task<IReadOnlyList<GroupFormPermissionDto>> GetAllSystemFormsAsync();
        Task DeleteGroupAsync(long groupId);
        Task<long> SaveGroupWithPermissionsAsync(SaveGroupRequestDto request, long regUserId);
    }
}
