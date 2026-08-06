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

        public async Task<long> SaveGroupAsync(SaveGroupDto model, long? regBy)
        {
            var codeParam = new SqlParameter("@Code", model.GroupCode ?? (object)DBNull.Value);
            var engNameParam = new SqlParameter("@EngName", model.EnglishName ?? (object)DBNull.Value);
            var arbNameParam = new SqlParameter("@ArbName", string.IsNullOrWhiteSpace(model.ArabicName) ? model.EnglishName : model.ArabicName);
            var isActiveParam = new SqlParameter("@IsActive", model.IsActive);
            var regByParam = new SqlParameter("@RegBy", regBy ?? (object)DBNull.Value);
            var groupIdParam = new SqlParameter("@GroupID", model.GroupID.HasValue && model.GroupID > 0 ? model.GroupID.Value : (object)DBNull.Value);

            var newGroupIdParam = new SqlParameter("@NewGroupID", System.Data.SqlDbType.BigInt) { Direction = System.Data.ParameterDirection.Output };
            var hasErrorParam = new SqlParameter("@HasError", System.Data.SqlDbType.Bit) { Direction = System.Data.ParameterDirection.Output };
            var errorDescParam = new SqlParameter("@ErrorDesc", System.Data.SqlDbType.NVarChar, 2048) { Direction = System.Data.ParameterDirection.Output };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_Add_SystemGroups @Code = @Code, @EngName = @EngName, @ArbName = @ArbName, @IsActive = @IsActive, @RegBy = @RegBy, @GroupID = @GroupID, @NewGroupID = @NewGroupID OUTPUT, @HasError = @HasError OUTPUT, @ErrorDesc = @ErrorDesc OUTPUT",
                codeParam, engNameParam, arbNameParam, isActiveParam, regByParam, groupIdParam, newGroupIdParam, hasErrorParam, errorDescParam);

            if (hasErrorParam.Value != DBNull.Value && (bool)hasErrorParam.Value)
            {
                var errorMsg = errorDescParam.Value?.ToString() ?? "Error saving group details.";
                throw new InvalidOperationException(errorMsg);
            }

            long groupId = newGroupIdParam.Value != DBNull.Value && newGroupIdParam.Value != null ? (long)newGroupIdParam.Value : (model.GroupID ?? 0);

            if (!string.IsNullOrWhiteSpace(model.PermissionsJson) && groupId > 0)
            {
                var permGroupIdParam = new SqlParameter("@GroupID", groupId);
                var permJsonParam = new SqlParameter("@PermissionsJson", model.PermissionsJson);
                var permRegByParam = new SqlParameter("@RegUserID", regBy ?? (object)DBNull.Value);
                var permHasErrorParam = new SqlParameter("@HasError", System.Data.SqlDbType.Bit) { Direction = System.Data.ParameterDirection.Output };
                var permErrorDescParam = new SqlParameter("@ErrorDesc", System.Data.SqlDbType.NVarChar, 2048) { Direction = System.Data.ParameterDirection.Output };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.usp_Add_SystemGroupPermissions @GroupID = @GroupID, @PermissionsJson = @PermissionsJson, @RegUserID = @RegUserID, @HasError = @HasError OUTPUT, @ErrorDesc = @ErrorDesc OUTPUT",
                    permGroupIdParam, permJsonParam, permRegByParam, permHasErrorParam, permErrorDescParam);

                if (permHasErrorParam.Value != DBNull.Value && (bool)permHasErrorParam.Value)
                {
                    var errorMsg = permErrorDescParam.Value?.ToString() ?? "Error saving group permissions.";
                    throw new InvalidOperationException(errorMsg);
                }
            }

            return groupId;
        }
    }
}
