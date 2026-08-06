using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AhmedRawdiBusinessPlatform.Data;
using AhmedRawdiBusinessPlatform.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AhmedRawdiBusinessPlatform.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<UserListItemDto>> GetAllUsersAsync()
        {
            return await _context.Database
                .SqlQueryRaw<UserListItemDto>("EXEC dbo.usp_Get_AllUsers")
                .ToListAsync();
        }

        public async Task<long> SaveUserAsync(SaveUserDto model, long? registeredBy = null)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var groupParam = new SqlParameter("@GroupID", model.GroupID);
            var codeParam = new SqlParameter("@Code", model.Code ?? string.Empty);
            var engNameParam = new SqlParameter("@EngName", model.EngName ?? string.Empty);
            var arbNameParam = new SqlParameter("@ArbName", (object?)model.ArbName ?? DBNull.Value);
            var isActiveParam = new SqlParameter("@IsActive", model.IsActive);
            var emailParam = new SqlParameter("@Email", (object?)model.Email ?? DBNull.Value);
            var mobileNumParam = new SqlParameter("@MobileNum", (object?)model.MobileNum ?? DBNull.Value);
            var regByParam = new SqlParameter("@RegBy", (object?)registeredBy ?? DBNull.Value);
            var expiredDateParam = new SqlParameter("@ExpiredDate", (object?)model.ExpiredDate ?? DBNull.Value);
            var userIdParam = new SqlParameter("@UserID", (object?)model.UserID ?? DBNull.Value);
            var langParam = new SqlParameter("@PreferredLanguage", (object?)model.PreferredLanguage ?? DBNull.Value);

            var hasErrorParam = new SqlParameter("@HasError", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };
            var errorDescParam = new SqlParameter("@ErrorDesc", SqlDbType.NVarChar, 2048)
            {
                Direction = ParameterDirection.Output
            };
            var newUserIdParam = new SqlParameter("@NewUserID", SqlDbType.BigInt)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_Add_SystemUser " +
                "@GroupID = @GroupID, " +
                "@Code = @Code, " +
                "@EngName = @EngName, " +
                "@ArbName = @ArbName, " +
                "@IsActive = @IsActive, " +
                "@Email = @Email, " +
                "@MobileNum = @MobileNum, " +
                "@RegBy = @RegBy, " +
                "@ExpiredDate = @ExpiredDate, " +
                "@HasError = @HasError OUTPUT, " +
                "@ErrorDesc = @ErrorDesc OUTPUT, " +
                "@UserID = @UserID, " +
                "@PreferredLanguage = @PreferredLanguage, " +
                "@NewUserID = @NewUserID OUTPUT",
                groupParam, codeParam, engNameParam, arbNameParam, isActiveParam, emailParam,
                mobileNumParam, regByParam, expiredDateParam, hasErrorParam, errorDescParam,
                userIdParam, langParam, newUserIdParam
            );

            if (hasErrorParam.Value != DBNull.Value && Convert.ToBoolean(hasErrorParam.Value))
            {
                var errorMsg = errorDescParam.Value != DBNull.Value ? errorDescParam.Value.ToString() : "An error occurred while saving the user.";
                throw new InvalidOperationException(errorMsg);
            }

            if (newUserIdParam.Value != DBNull.Value && newUserIdParam.Value != null)
            {
                return Convert.ToInt64(newUserIdParam.Value);
            }

            return model.UserID ?? 0;
        }

        public async Task DeleteUserAsync(long userId)
        {
            var userIdParam = new SqlParameter("@UserID", userId);
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_Delete_User @UserID = @UserID",
                userIdParam);
        }
    }
}
