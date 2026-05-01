using System.Security.Claims;

namespace TL.VerticalSlice.Template.Application.Contracts.Authentication;

public interface ITokenService
{
    string GenerateToken(string userId, string username, IEnumerable<string> roles);

    string GenerateRefreshToken();

    bool ValidateToken(string token);

    ClaimsPrincipal GetClaimsFromExpiredToken(string token);
}

