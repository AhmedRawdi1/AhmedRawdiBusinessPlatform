using AhmedRawdiBusinessPlatform.Models;

namespace AhmedRawdiBusinessPlatform.Services
{
    public interface IGroupService
    {
        Task<IReadOnlyList<GroupListItemDto>> GetAllGroupsAsync();
        Task<IReadOnlyList<GroupFormPermissionDto>> GetGroupPermissionsAsync(long groupId);
        Task<IReadOnlyList<GroupFormPermissionDto>> GetAllSystemFormsAsync();
        Task<IReadOnlyList<GroupMemberDto>> GetGroupMembersAsync(long groupId);
        Task DeleteGroupAsync(long groupId);
        Task<long> SaveGroupWithPermissionsAsync(SaveGroupRequestDto request, long regUserId);
    }
}
