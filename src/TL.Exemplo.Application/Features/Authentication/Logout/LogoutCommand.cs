using MediatR;

namespace TL.Exemplo.Application.Features.Authentication.Logout;

public record LogoutCommand(string UserId) : IRequest<bool>;
