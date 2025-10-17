using Chibest.Repository.Models;
using System.Security.Claims;

namespace Chibest.Service.Interface;
public interface ITokenService
{
    string GenerateAccessToken(Account account, string roleName);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}