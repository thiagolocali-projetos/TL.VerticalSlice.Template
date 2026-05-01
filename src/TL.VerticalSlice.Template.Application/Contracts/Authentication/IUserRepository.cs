using TL.VerticalSlice.Template.Application.Common.Models;

namespace TL.VerticalSlice.Template.Application.Contracts.Authentication;

public interface IUserRepository
{
    UserCredential? GetUserByUsername(string username);
    UserCredential? GetUserById(string userId);
    bool ValidateCredentials(string username, string password);
}

