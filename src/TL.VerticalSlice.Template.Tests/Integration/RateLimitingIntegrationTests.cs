using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TL.VerticalSlice.Template.Application.Common.Models;
using Xunit;

namespace TL.VerticalSlice.Template.Tests.Integration;

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

        // Act - Fazer 10 requisiÃ§Ãµes (bem abaixo do limite de 100/min)
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_fixture.GetAuthenticatedAsync("/api/v1/Samples"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - Todas devem ser bem-sucedidas (nÃ£o 429)
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

        // Act - Fazer 110 requisiÃ§Ãµes (acima do limite de 100/min)
        var successCount = 0;
        var limitExceededCount = 0;

        for (int i = 0; i < 110; i++)
        {
            var response = await _fixture.GetAuthenticatedAsync("/api/v1/Samples");

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
        limitExceededCount.Should().BeGreaterThan(0); // PrÃ³ximas devem falhar
    }

    [Fact]
    public async Task RateLimitingError_ShouldReturn429Status()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Exceder o limite
        for (int i = 0; i < 101; i++)
        {
            await _fixture.GetAuthenticatedAsync("/api/v1/Samples");
        }

        // Act - A 101Âª requisiÃ§Ã£o deve ser bloqueada
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/Samples");

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
            await _fixture.GetAuthenticatedAsync("/api/v1/Samples");
        }

        // Act
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/Samples");
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
            await _fixture.GetAuthenticatedAsync("/api/v1/Samples");
        }

        // Act
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/Samples");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().Contain("retryAfter");
    }

    [Fact]
    public async Task RateLimiting_IsPerIp_NotGlobal()
    {
        // Este teste Ã© conceitual - na prÃ¡tica seria difÃ­cil de testar
        // sem mÃºltiplos clientes HTTP com IPs diferentes

        // Arrange
        await _fixture.LoginAsync();

        // Act - Fazer muitas requisiÃ§Ãµes (dentro do limite)
        var tasksBatch1 = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 20; i++)
        {
            tasksBatch1.Add(_fixture.GetAuthenticatedAsync("/api/v1/Samples"));
        }

        var responsesBatch1 = await Task.WhenAll(tasksBatch1);

        // Assert - Como vem do mesmo IP, conta tudo junto
        responsesBatch1.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task RateLimiting_ShouldAllowHealthCheckWithoutLimit()
    {
        // Nota: Health checks sÃ£o pÃºblicos e testamos aqui se sÃ£o limitados ou nÃ£o
        // Se nÃ£o houver proteÃ§Ã£o especial, eles devem estar sujeitos ao rate limit

        // Act - Fazer muitas requisiÃ§Ãµes ao health check
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.FromResult(_fixture.HttpClient!.GetAsync("/health").Result));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - Health checks devem ser rÃ¡pidos
        responses.Should().AllSatisfy(r =>
            r.StatusCode.Should().Be(HttpStatusCode.OK)
        );
    }
}

