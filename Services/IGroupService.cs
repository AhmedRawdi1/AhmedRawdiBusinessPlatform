using AhmedRawdiBusinessPlatform.Models;

namespace AhmedRawdiBusinessPlatform.Services
{
    public interface IGroupService
    {
        Task<IReadOnlyList<GroupListItemDto>> GetAllGroupsAsync();
        Task DeleteGroupAsync(long groupId);
    }
}
