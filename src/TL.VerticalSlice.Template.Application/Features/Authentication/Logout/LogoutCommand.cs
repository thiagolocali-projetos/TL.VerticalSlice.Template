using MediatR;

namespace TL.VerticalSlice.Template.Application.Features.Authentication.Logout;

public record LogoutCommand(string UserId) : IRequest<bool>;

