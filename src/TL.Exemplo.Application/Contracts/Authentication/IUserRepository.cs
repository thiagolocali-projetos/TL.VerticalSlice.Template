using TL.Exemplo.Application.Common.Models;

namespace TL.Exemplo.Application.Contracts.Authentication;

public interface IUserRepository
{
    UserCredential? GetUserByUsername(string username);
    UserCredential? GetUserById(string userId);
    bool ValidateCredentials(string username, string password);
}
