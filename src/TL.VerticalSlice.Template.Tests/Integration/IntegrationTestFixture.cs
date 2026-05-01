using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using TL.VerticalSlice.Template.API;
using TL.VerticalSlice.Template.Application.Common.Models;
using Xunit;

namespace TL.VerticalSlice.Template.Tests.Integration;

/// <summary>
/// Fixture base para testes de integraÃ§Ã£o da API.
/// Gerencia o WebApplicationFactory e fornece helpers.
/// </summary>
public class IntegrationTestFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    public HttpClient HttpClient { get; private set; } = null!;
    private string? _authToken;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();
        HttpClient = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        HttpClient?.Dispose();
        _factory?.Dispose();
    }

    /// <summary>
    /// Faz login e retorna o token JWT.
    /// </summary>
    public async Task<string> LoginAsync(string username = "admin", string password = "admin123")
    {
        var loginRequest = new { username, password };
        var response = await HttpClient.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        Assert.True(response.IsSuccessStatusCode, $"Login failed: {await response.Content.ReadAsStringAsync()}");

        var content = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();
        _authToken = content!.Dados.AccessToken;

        return _authToken;
    }

    /// <summary>
    /// Adiciona o token JWT ao header Authorization.
    /// </summary>
    public void SetAuthToken(string token)
    {
        _authToken = token;
        HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Limpa o token de autenticaÃ§Ã£o.
    /// </summary>
    public void ClearAuthToken()
    {
        _authToken = null;
        HttpClient.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Faz uma requisiÃ§Ã£o GET autenticada.
    /// </summary>
    public async Task<HttpResponseMessage> GetAuthenticatedAsync(string url)
    {
        if (string.IsNullOrEmpty(_authToken))
            throw new InvalidOperationException("NÃ£o autenticado. FaÃ§a login primeiro.");

        return await HttpClient.GetAsync(url);
    }

    /// <summary>
    /// Faz uma requisiÃ§Ã£o POST autenticada.
    /// </summary>
    public async Task<HttpResponseMessage> PostAuthenticatedAsync(string url, object data)
    {
        if (string.IsNullOrEmpty(_authToken))
            throw new InvalidOperationException("NÃ£o autenticado. FaÃ§a login primeiro.");

        return await HttpClient.PostAsJsonAsync(url, data);
    }

    /// <summary>
    /// Faz uma requisiÃ§Ã£o PUT autenticada.
    /// </summary>
    public async Task<HttpResponseMessage> PutAuthenticatedAsync(string url, object data)
    {
        if (string.IsNullOrEmpty(_authToken))
            throw new InvalidOperationException("NÃ£o autenticado. FaÃ§a login primeiro.");

        return await HttpClient.PutAsJsonAsync(url, data);
    }

    /// <summary>
    /// Faz uma requisiÃ§Ã£o DELETE autenticada.
    /// </summary>
    public async Task<HttpResponseMessage> DeleteAuthenticatedAsync(string url)
    {
        if (string.IsNullOrEmpty(_authToken))
            throw new InvalidOperationException("NÃ£o autenticado. FaÃ§a login primeiro.");

        return await HttpClient.DeleteAsync(url);
    }
}

/// <summary>
/// Collection fixture para compartilhar a mesma instÃ¢ncia entre testes.
/// </summary>
[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
    // Esta classe estÃ¡ vazia, serve apenas para agrupar os testes
}

