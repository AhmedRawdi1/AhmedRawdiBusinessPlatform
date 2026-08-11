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
        private readonly IPasswordService _passwordService;

        public UserService(ApplicationDbContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
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
            var passwordHash = string.IsNullOrWhiteSpace(model.UserPass) ? null : _passwordService.HashPassword(model.UserPass);
            if (!model.UserID.HasValue && passwordHash == null)
                throw new ArgumentException("A password is required for a new user.", nameof(model));
            var passwordHashParam = new SqlParameter("@PasswordHash", (object?)passwordHash ?? DBNull.Value);

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
                "@NewUserID = @NewUserID OUTPUT, " +
                "@PasswordHash = @PasswordHash",
                groupParam, codeParam, engNameParam, arbNameParam, isActiveParam, emailParam,
                mobileNumParam, regByParam, expiredDateParam, hasErrorParam, errorDescParam,
                userIdParam, langParam, newUserIdParam, passwordHashParam
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

        public async Task ResetPasswordAsync(long userId, string newPassword, long performedByUserId)
        {
            ValidatePassword(newPassword);
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_Reset_UserPassword @UserID={0}, @PasswordHash={1}, @PerformedByUserID={2}",
                userId, _passwordService.HashPassword(newPassword), performedByUserId);
        }

        public async Task ChangeOwnPasswordAsync(long userId, string newPassword)
        {
            ValidatePassword(newPassword);
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_Change_OwnPassword @UserID={0}, @PasswordHash={1}",
                userId, _passwordService.HashPassword(newPassword));
        }

        public async Task<bool> IsSessionValidAsync(long userId, Guid securityStamp)
        {
            var userParam = new SqlParameter("@UserID", userId);
            var stampParam = new SqlParameter("@SecurityStamp", securityStamp);
            var result = await _context.Database.SqlQueryRaw<SessionValidationDto>(
                "EXEC dbo.usp_Validate_UserSession @UserID=@UserID, @SecurityStamp=@SecurityStamp", userParam, stampParam).ToListAsync();
            return result.FirstOrDefault()?.IsValid == true;
        }

        private static void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 12 ||
                !password.Any(char.IsUpper) || !password.Any(char.IsLower) ||
                !password.Any(char.IsDigit) || !password.Any(ch => !char.IsLetterOrDigit(ch)))
                throw new ArgumentException("Password must contain at least 12 characters, uppercase, lowercase, number, and special character.");
        }
    }
}
