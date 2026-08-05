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

        public async Task<long> SaveGroupWithPermissionsAsync(SaveGroupRequestDto request, long regUserId)
        {
            var codeParam = new SqlParameter("@Code", System.Data.SqlDbType.NVarChar, 20) { Value = (object?)request.GroupCode ?? DBNull.Value };
            var engNameParam = new SqlParameter("@EngName", System.Data.SqlDbType.NVarChar, 150) { Value = (object?)request.EnglishName ?? DBNull.Value };
            var arbNameParam = new SqlParameter("@ArbName", System.Data.SqlDbType.NVarChar, 150) { Value = (object?)request.ArabicName ?? DBNull.Value };
            var isActiveParam = new SqlParameter("@IsActive", System.Data.SqlDbType.Bit) { Value = request.IsActive };
            var regByParam = new SqlParameter("@RegBy", System.Data.SqlDbType.BigInt) { Value = regUserId };
            var expiredDateParam = new SqlParameter("@ExpiredDate", System.Data.SqlDbType.SmallDateTime) { Value = DBNull.Value };
            var groupIdParam = new SqlParameter("@GroupID", System.Data.SqlDbType.BigInt) { Value = (object?)request.GroupID ?? DBNull.Value };

            var newGroupIdParam = new SqlParameter("@NewGroupID", System.Data.SqlDbType.BigInt) { Direction = System.Data.ParameterDirection.Output };
            var hasErrorParam = new SqlParameter("@HasError", System.Data.SqlDbType.Bit) { Direction = System.Data.ParameterDirection.Output };
            var errorDescParam = new SqlParameter("@ErrorDesc", System.Data.SqlDbType.NVarChar, 2048) { Direction = System.Data.ParameterDirection.Output };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_Add_SystemGroups " +
                "@Code = @Code, @EngName = @EngName, @ArbName = @ArbName, @IsActive = @IsActive, @RegBy = @RegBy, " +
                "@ExpiredDate = @ExpiredDate, @NewGroupID = @NewGroupID OUTPUT, @HasError = @HasError OUTPUT, " +
                "@ErrorDesc = @ErrorDesc OUTPUT, @GroupID = @GroupID",
                codeParam, engNameParam, arbNameParam, isActiveParam, regByParam, expiredDateParam,
                newGroupIdParam, hasErrorParam, errorDescParam, groupIdParam);

            var hasError = hasErrorParam.Value != DBNull.Value && Convert.ToBoolean(hasErrorParam.Value);
            if (hasError)
            {
                var errorDesc = errorDescParam.Value?.ToString() ?? "Error saving system group.";
                throw new InvalidOperationException(errorDesc);
            }

            var savedGroupId = Convert.ToInt64(newGroupIdParam.Value);

            var mappedPermissions = request.Permissions.Select(p => new
            {
                FormID = p.FormID,
                CanSave = p.CanSave || p.CanView,
                CanUpdate = p.CanUpdate || p.CanView,
                CanDelete = p.CanDelete,
                CanSearch = p.CanSearch || p.CanView,
                CanPrint = p.CanPrint
            }).ToList();

            var jsonPermissions = System.Text.Json.JsonSerializer.Serialize(mappedPermissions);

            var permGroupIdParam = new SqlParameter("@GroupID", System.Data.SqlDbType.BigInt) { Value = savedGroupId };
            var permJsonParam = new SqlParameter("@PermissionsJson", System.Data.SqlDbType.NVarChar, -1) { Value = jsonPermissions };
            var permRegUserParam = new SqlParameter("@RegUserID", System.Data.SqlDbType.BigInt) { Value = regUserId };
            var permHasErrorParam = new SqlParameter("@HasError", System.Data.SqlDbType.Bit) { Direction = System.Data.ParameterDirection.Output };
            var permErrorDescParam = new SqlParameter("@ErrorDesc", System.Data.SqlDbType.NVarChar, 2048) { Direction = System.Data.ParameterDirection.Output };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_Add_SystemGroupPermissions " +
                "@GroupID = @GroupID, @PermissionsJson = @PermissionsJson, @RegUserID = @RegUserID, " +
                "@HasError = @HasError OUTPUT, @ErrorDesc = @ErrorDesc OUTPUT",
                permGroupIdParam, permJsonParam, permRegUserParam, permHasErrorParam, permErrorDescParam);

            var permHasError = permHasErrorParam.Value != DBNull.Value && Convert.ToBoolean(permHasErrorParam.Value);
            if (permHasError)
            {
                var permErrorDesc = permErrorDescParam.Value?.ToString() ?? "Error saving group permissions.";
                throw new InvalidOperationException(permErrorDesc);
            }

            return savedGroupId;
        }
    }
}
