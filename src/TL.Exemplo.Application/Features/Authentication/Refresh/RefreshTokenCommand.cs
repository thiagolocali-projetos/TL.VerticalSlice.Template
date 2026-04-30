using MediatR;
using TL.Exemplo.Application.Common.Models;

namespace TL.Exemplo.Application.Features.Authentication.Refresh;

public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResponse>;
