using FluentValidation;
using MediatR;
using TL.Exemplo.Application.Common.Exceptions;
using TL.Exemplo.Application.Common.Models;
using TL.Exemplo.Application.Contracts.Authentication;
using TL.Exemplo.Application.Contracts.Repositories;
using TL.Exemplo.Domain.Entities;

namespace TL.Exemplo.Application.Features.Authentication.Login;

public record LoginCommand(string Username, string Password) : IRequest<LoginResponse>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("O nome de usuario e obrigatorio.")
            .MinimumLength(3).WithMessage("O nome de usuario deve ter no minimo 3 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha e obrigatoria.")
            .MinimumLength(6).WithMessage("A senha deve ter no minimo 6 caracteres.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LoginCommandHandler(
        ITokenService tokenService,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _tokenService = tokenService;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (!_userRepository.ValidateCredentials(request.Username, request.Password))
            throw new BusinessException("Nome de usuario ou senha invalidos.");

        var user = _userRepository.GetUserByUsername(request.Username);
        if (user == null)
            throw new BusinessException("Usuario nao encontrado.");

        var accessToken = _tokenService.GenerateToken(user.UserId, user.Username, user.Roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.UserId,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

        return new LoginResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            TokenType: "Bearer",
            ExpiresIn: 3600);
    }
}
