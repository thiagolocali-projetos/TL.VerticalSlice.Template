using TL.VerticalSlice.Template.Application.Common.Models;
using TL.VerticalSlice.Template.Application.Contracts.Authentication;

namespace TL.VerticalSlice.Template.Infrastructure.Authentication;

/// <summary>
/// RepositÃ³rio simulado de usuÃ¡rios (em produÃ§Ã£o, usar banco de dados).
/// </summary>
public class UserRepository : IUserRepository
{
    // Dados de teste - em produÃ§Ã£o, usar banco de dados com senhas hasheadas
    private static readonly List<UserCredential> Users = new()
    {
        new UserCredential(
            UserId: "1",
            Username: "admin",
            Password: "admin123",
            Roles: new[] { "Admin", "User" }
        ),
        new UserCredential(
            UserId: "2",
            Username: "user",
            Password: "user123",
            Roles: new[] { "User" }
        )
    };

    public UserCredential? GetUserByUsername(string username)
    {
        return Users.FirstOrDefault(u => u.Username == username);
    }

    public UserCredential? GetUserById(string userId)
    {
        return Users.FirstOrDefault(u => u.UserId == userId);
    }

    public bool ValidateCredentials(string username, string password)
    {
        var user = GetUserByUsername(username);
        return user != null && user.Password == password;
    }
}

