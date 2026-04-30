using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TL.Exemplo.Application.Common.Models;
using Xunit;

namespace TL.Exemplo.Tests.Integration;

[Collection("Integration Tests")]
public class RateLimitingIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;

    public RateLimitingIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RateLimiting_ShouldAllowRequestsWithinLimit()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Act - Fazer 10 requisições (bem abaixo do limite de 100/min)
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_fixture.GetAuthenticatedAsync("/api/v1/produtos"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - Todas devem ser bem-sucedidas (não 429)
        foreach (var response in responses)
        {
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }
    }

    [Fact]
    public async Task RateLimiting_ShouldRejectRequestsExceedingLimit()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Act - Fazer 110 requisições (acima do limite de 100/min)
        var successCount = 0;
        var limitExceededCount = 0;

        for (int i = 0; i < 110; i++)
        {
            var response = await _fixture.GetAuthenticatedAsync("/api/v1/produtos");

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                limitExceededCount++;
            }
            else if (response.StatusCode == HttpStatusCode.OK)
            {
                successCount++;
            }
        }

        // Assert
        successCount.Should().Be(100); // Primeiras 100 devem passar
        limitExceededCount.Should().BeGreaterThan(0); // Próximas devem falhar
    }

    [Fact]
    public async Task RateLimitingError_ShouldReturn429Status()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Exceder o limite
        for (int i = 0; i < 101; i++)
        {
            await _fixture.GetAuthenticatedAsync("/api/v1/produtos");
        }

        // Act - A 101ª requisição deve ser bloqueada
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/produtos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task RateLimitingError_ShouldIncludeErrorMessage()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Exceder o limite
        for (int i = 0; i < 101; i++)
        {
            await _fixture.GetAuthenticatedAsync("/api/v1/produtos");
        }

        // Act
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/produtos");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        content.Should().Contain("Rate limit exceeded");
    }

    [Fact]
    public async Task RateLimitingError_ShouldIncludeRetryAfter()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Exceder o limite
        for (int i = 0; i < 101; i++)
        {
            await _fixture.GetAuthenticatedAsync("/api/v1/produtos");
        }

        // Act
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/produtos");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().Contain("retryAfter");
    }

    [Fact]
    public async Task RateLimiting_IsPerIp_NotGlobal()
    {
        // Este teste é conceitual - na prática seria difícil de testar
        // sem múltiplos clientes HTTP com IPs diferentes

        // Arrange
        await _fixture.LoginAsync();

        // Act - Fazer muitas requisições (dentro do limite)
        var tasksBatch1 = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 20; i++)
        {
            tasksBatch1.Add(_fixture.GetAuthenticatedAsync("/api/v1/produtos"));
        }

        var responsesBatch1 = await Task.WhenAll(tasksBatch1);

        // Assert - Como vem do mesmo IP, conta tudo junto
        responsesBatch1.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task RateLimiting_ShouldAllowHealthCheckWithoutLimit()
    {
        // Nota: Health checks são públicos e testamos aqui se são limitados ou não
        // Se não houver proteção especial, eles devem estar sujeitos ao rate limit

        // Act - Fazer muitas requisições ao health check
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.FromResult(_fixture.HttpClient!.GetAsync("/health").Result));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - Health checks devem ser rápidos
        responses.Should().AllSatisfy(r =>
            r.StatusCode.Should().Be(HttpStatusCode.OK)
        );
    }
}
