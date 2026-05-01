using MediatR;
using TL.VerticalSlice.Template.Application.Common.Models;

namespace TL.VerticalSlice.Template.Application.Features.Authentication.Refresh;

public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResponse>;

