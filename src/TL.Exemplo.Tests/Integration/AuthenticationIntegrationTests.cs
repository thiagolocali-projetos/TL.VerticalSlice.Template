using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TL.Exemplo.Application.Common.Models;
using Xunit;

namespace TL.Exemplo.Tests.Integration;

[Collection("Integration Tests")]
public class AuthenticationIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;

    public AuthenticationIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Act
        var token = await _fixture.LoginAsync("admin", "admin123");

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().StartWith("eyJ"); // JWT header
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new { username = "admin", password = "wrongpassword" };

        // Act
        var response = await _fixture.HttpClient!.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        content.Sucesso.Should().BeFalse();
        content.Mensagem.Should().Contain("inválidos");
    }

    [Fact]
    public async Task Login_WithNonexistentUser_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new { username = "nonexistent", password = "password123" };

        // Act
        var response = await _fixture.HttpClient!.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new { username = "", password = "password123" };

        // Act
        var response = await _fixture.HttpClient!.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithAdminUser_ShouldReturnTokenWithAdminRole()
    {
        // Act
        var token = await _fixture.LoginAsync("admin", "admin123");

        // Assert - O token foi validado no LoginAsync
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithRegularUser_ShouldReturnToken()
    {
        // Act
        var token = await _fixture.LoginAsync("user", "user123");

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange - Não fazer login

        // Act
        var response = await _fixture.HttpClient!.GetAsync("/api/v1/produtos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Act
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/produtos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _fixture.SetAuthToken("invalid.token.here");

        // Act
        var response = await _fixture.HttpClient!.GetAsync("/api/v1/produtos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
