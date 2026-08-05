using AhmedRawdiBusinessPlatform.Data;
using AhmedRawdiBusinessPlatform.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AhmedRawdiBusinessPlatform.Services
{
    public class GroupService : IGroupService
    {
        private readonly ApplicationDbContext _context;

        public GroupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<GroupListItemDto>> GetAllGroupsAsync()
        {
            return await _context.Database
                .SqlQueryRaw<GroupListItemDto>("EXEC dbo.usp_Get_AllGroups")
                .ToListAsync();
        }

        public async Task<IReadOnlyList<GroupFormPermissionDto>> GetGroupPermissionsAsync(long groupId)
        {
            var groupIdParameter = new SqlParameter("@GroupID", groupId);
            return await _context.Database
                .SqlQueryRaw<GroupFormPermissionDto>(
                    "EXEC dbo.usp_Get_GroupPermissions @GroupID = @GroupID",
                    groupIdParameter)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<GroupFormPermissionDto>> GetAllSystemFormsAsync()
        {
            return await _context.Database
                .SqlQueryRaw<GroupFormPermissionDto>("EXEC dbo.usp_Get_AllSystemForms")
                .ToListAsync();
        }

        public async Task DeleteGroupAsync(long groupId)
        {
            var groupIdParameter = new SqlParameter("@GroupID", groupId);
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_Delete_Group @GroupID = @GroupID",
                groupIdParameter);
        }
    }
}
