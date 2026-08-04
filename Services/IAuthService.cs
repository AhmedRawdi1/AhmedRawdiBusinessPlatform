using System.Threading.Tasks;
using AhmedRawdiBusinessPlatform.Models;

namespace AhmedRawdiBusinessPlatform.Services
{
    public interface IAuthService
    {
        Task<UserInfoResultDto?> GetUserInfoByUserCodeAsync(string userCode);
        Task<(bool IsSuccess, string Message, UserInfoResultDto? UserInfo)> ValidateUserAsync(string userCode, string password);
    }
}
