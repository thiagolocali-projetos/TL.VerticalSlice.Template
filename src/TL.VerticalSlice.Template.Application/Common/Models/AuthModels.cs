namespace TL.VerticalSlice.Template.Application.Common.Models;

public record LoginRequest(string Username, string Password);

public record TokenResponse(string AccessToken, string TokenType = "Bearer", int ExpiresIn = 3600);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType = "Bearer",
    int ExpiresIn = 3600);

public record RefreshTokenRequest(string RefreshToken);

public record UserCredential(string UserId, string Username, string Password, IEnumerable<string> Roles);

