using FluentValidation;
using MediatR;
using System.Security.Claims;
using TL.VerticalSlice.Template.Application.Common.Exceptions;
using TL.VerticalSlice.Template.Application.Common.Models;
using TL.VerticalSlice.Template.Application.Contracts.Authentication;
using TL.VerticalSlice.Template.Application.Contracts.Repositories;
using TL.VerticalSlice.Template.Domain.Entities;

namespace TL.VerticalSlice.Template.Application.Features.Authentication.Refresh;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token e obrigatorio.");
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;

    public RefreshTokenCommandHandler(
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository)
    {
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
    }

    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

        if (refreshToken == null || !refreshToken.IsValid)
            throw new BusinessException("Refresh token invalido ou expirado.");

        var user = _userRepository.GetUserById(refreshToken.UserId);
        if (user == null)
            throw new BusinessException("Usuario nao encontrado.");

        var accessToken = _tokenService.GenerateToken(user.UserId, user.Username, user.Roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.UserId,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.CreateAsync(newRefreshTokenEntity);

        return new LoginResponse(
            AccessToken: accessToken,
            RefreshToken: newRefreshToken,
            TokenType: "Bearer",
            ExpiresIn: 3600);
    }
}

