using FluentAssertions;
using Moq;
using TL.Exemplo.Application.Common.Exceptions;
using TL.Exemplo.Application.Contracts.Authentication;
using TL.Exemplo.Application.Contracts.Repositories;
using TL.Exemplo.Application.Features.Authentication.Login;
using TL.Exemplo.Infrastructure.Authentication;
using Xunit;

namespace TL.Exemplo.Tests.Features.Authentication;

public class LoginCommandTests
{
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly UserRepository _userRepository;

    public LoginCommandTests()
    {
        _tokenServiceMock = new Mock<ITokenService>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _userRepository = new UserRepository();
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var command = new LoginCommand("admin", "admin123");
        const string expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        const string expectedRefreshToken = "refreshToken123";

        _tokenServiceMock
            .Setup(x => x.GenerateToken("1", "admin", It.IsAny<IEnumerable<string>>()))
            .Returns(expectedToken);

        _tokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns(expectedRefreshToken);

        _refreshTokenRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<TL.Exemplo.Domain.Entities.RefreshToken>()))
            .ReturnsAsync(1);

        var handler = new LoginCommandHandler(_tokenServiceMock.Object, _userRepository, _refreshTokenRepositoryMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(expectedToken);
        result.RefreshToken.Should().Be(expectedRefreshToken);
        result.TokenType.Should().Be("Bearer");
        result.ExpiresIn.Should().Be(3600);

        _tokenServiceMock.Verify(
            x => x.GenerateToken("1", "admin", It.IsAny<IEnumerable<string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldThrowBusinessException()
    {
        // Arrange
        var command = new LoginCommand("admin", "wrongpassword");
        var handler = new LoginCommandHandler(_tokenServiceMock.Object, _userRepository, _refreshTokenRepositoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(
            () => handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_WithNonexistentUser_ShouldThrowBusinessException()
    {
        // Arrange
        var command = new LoginCommand("nonexistent", "password123");
        var handler = new LoginCommandHandler(_tokenServiceMock.Object, _userRepository, _refreshTokenRepositoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(
            () => handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_WithEmptyUsername_ShouldThrowValidationException()
    {
        // Arrange
        var command = new LoginCommand("", "password123");
        var validator = new LoginCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Username");
    }

    [Fact]
    public async Task Handle_WithShortPassword_ShouldThrowValidationException()
    {
        // Arrange
        var command = new LoginCommand("user", "123"); // Senha muito curta
        var validator = new LoginCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Password");
    }
}
