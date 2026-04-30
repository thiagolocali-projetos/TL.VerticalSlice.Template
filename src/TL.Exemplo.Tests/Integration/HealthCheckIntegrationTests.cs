using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace TL.Exemplo.Tests.Integration;

[Collection("Integration Tests")]
public class HealthCheckIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;

    public HealthCheckIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetHealth_ShouldReturn200AndHealthyStatus()
    {
        // Act
        var response = await _fixture.HttpClient!.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("status").GetString().Should().Be("Healthy");
        root.GetProperty("timestamp").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetHealth_ShouldIncludeChecks()
    {
        // Act
        var response = await _fixture.HttpClient!.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty("checks", out var checksElement).Should().BeTrue();
        checksElement.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetHealthReady_ShouldReturn200()
    {
        // Act
        var response = await _fixture.HttpClient!.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetHealthLive_ShouldReturn200()
    {
        // Act
        var response = await _fixture.HttpClient!.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("status").GetString().Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthEndpoints_ShouldNotRequireAuthentication()
    {
        // Arrange - Não fazer login

        // Act
        var response1 = await _fixture.HttpClient!.GetAsync("/health");
        var response2 = await _fixture.HttpClient!.GetAsync("/health/ready");
        var response3 = await _fixture.HttpClient!.GetAsync("/health/live");

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        response3.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetHealth_ShouldIncludeTimestamp()
    {
        // Act
        var response = await _fixture.HttpClient!.GetAsync("/health");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Assert
        root.TryGetProperty("timestamp", out var timestamp).Should().BeTrue();
        DateTime.TryParse(timestamp.GetString(), out _).Should().BeTrue();
    }

    [Fact]
    public async Task HealthCheck_ResponseFormat_ShouldBeValid()
    {
        // Act
        var response = await _fixture.HttpClient!.GetAsync("/health");
        var json = await response.Content.ReadAsStringAsync();

        // Assert - Validar estrutura JSON
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty("status", out _).Should().BeTrue();
        root.TryGetProperty("timestamp", out _).Should().BeTrue();
        root.TryGetProperty("checks", out _).Should().BeTrue();
    }
}
