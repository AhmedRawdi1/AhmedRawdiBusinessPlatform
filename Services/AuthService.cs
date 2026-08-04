using System;
using System.Linq;
using System.Threading.Tasks;
using AhmedRawdiBusinessPlatform.Data;
using AhmedRawdiBusinessPlatform.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AhmedRawdiBusinessPlatform.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserInfoResultDto?> GetUserInfoByUserCodeAsync(string userCode)
        {
            if (string.IsNullOrWhiteSpace(userCode))
                return null;

            var param = new SqlParameter("@UserCode", userCode.Trim());
            
            var result = await _context.Database
                .SqlQueryRaw<UserInfoResultDto>("EXEC dbo.usp_Get_UserInfo @UserCode = @UserCode", param)
                .ToListAsync();

            return result.FirstOrDefault();
        }

        public async Task<(bool IsSuccess, string Message, UserInfoResultDto? UserInfo)> ValidateUserAsync(string userCode, string password)
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return (false, "User code is required.", null);
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return (false, "Password is required.", null);
            }

            UserInfoResultDto? userInfo;
            try
            {
                userInfo = await GetUserInfoByUserCodeAsync(userCode);
            }
            catch (Exception ex)
            {
                return (false, $"Error fetching user info: {ex.Message}", null);
            }

            if (userInfo == null)
            {
                return (false, "Invalid user code or password.", null);
            }

            if (!userInfo.IsActive)
            {
                return (false, "This user account is inactive.", null);
            }

            if (userInfo.ExpiredDate.HasValue && userInfo.ExpiredDate.Value < DateTime.Now)
            {
                return (false, "This user account has expired.", null);
            }

            if (userInfo.UserPass != password)
            {
                return (false, "Invalid user code or password.", null);
            }

            return (true, "Authentication successful.", userInfo);
        }
    }
}
