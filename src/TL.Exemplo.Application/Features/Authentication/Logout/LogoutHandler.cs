using MediatR;
using TL.Exemplo.Application.Contracts.Repositories;

namespace TL.Exemplo.Application.Features.Authentication.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _refreshTokenRepository.RevokeAllByUserAsync(request.UserId);
        return true;
    }
}
