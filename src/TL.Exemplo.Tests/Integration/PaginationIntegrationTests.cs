using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TL.Exemplo.Application.Common.Models;
using Xunit;

namespace TL.Exemplo.Tests.Integration;

[Collection("Integration Tests")]
public class PaginationIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;

    public PaginationIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetPagedProdutos_WithDefaultParameters_ShouldReturn200()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Act
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/produtos/paged");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProdutoDto>>>();
        content.Sucesso.Should().BeTrue();
        content.Dados.Should().NotBeNull();
        content.Dados.PageNumber.Should().Be(1);
        content.Dados.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task GetPagedProdutos_WithCustomPageSize_ShouldReturn200()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Act
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/produtos/paged?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProdutoDto>>>();
        content.Dados.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetPagedProdutos_WithPageSize100_ShouldCapAtMax()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Act - Tentar pageSize acima do máximo (100)
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/produtos/paged?pageNumber=1&pageSize=200");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProdutoDto>>>();
        content.Dados.PageSize.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public async Task GetPagedProdutos_WithInvalidPageNumber_ShouldAdjustToMinimum()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Act - Tentar pageNumber 0 ou negativo
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/produtos/paged?pageNumber=0&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProdutoDto>>>();
        content.Dados.PageNumber.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetPagedProdutos_ShouldIncludeMetadata()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Act
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/produtos/paged?pageNumber=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProdutoDto>>>();

        content.Dados.Should().NotBeNull();
        content.Dados.Items.Should().NotBeNull();
        content.Dados.PageNumber.Should().Be(1);
        content.Dados.PageSize.Should().Be(20);
        content.Dados.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        content.Dados.TotalPages.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetPagedProdutos_WithApenasAtivos_ShouldReturnOnlyActive()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Criar alguns produtos (ativos por padrão)
        for (int i = 0; i < 3; i++)
        {
            var createRequest = new
            {
                nome = $"Produto Ativo {i}",
                descricao = "Descrição",
                preco = 100.00m + i,
                quantidadeEstoque = 10
            };
            await _fixture.PostAuthenticatedAsync("/api/v1/produtos", createRequest);
        }

        // Act
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/produtos/paged?apenasAtivos=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProdutoDto>>>();
        content.Dados.Items.Should().AllSatisfy(p => p.Ativo.Should().BeTrue());
    }

    [Fact]
    public async Task GetPagedProdutos_HasNextPage_ShouldBeCorrect()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Criar múltiplos produtos
        for (int i = 0; i < 25; i++)
        {
            var createRequest = new
            {
                nome = $"Produto {i:D2}",
                descricao = "Descrição",
                preco = 100.00m + i,
                quantidadeEstoque = 10
            };
            await _fixture.PostAuthenticatedAsync("/api/v1/produtos", createRequest);
        }

        // Act - Primeira página com tamanho 10
        var response1 = await _fixture.GetAuthenticatedAsync("/api/v1/produtos/paged?pageNumber=1&pageSize=10");
        var content1 = await response1.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProdutoDto>>>();

        // Assert
        content1.Dados.HasNextPage.Should().BeTrue();
        content1.Dados.TotalPages.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task GetPagedProdutos_LastPage_ShouldNotHaveNextPage()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Criar alguns produtos
        for (int i = 0; i < 5; i++)
        {
            var createRequest = new
            {
                nome = $"Produto Final {i}",
                descricao = "Descrição",
                preco = 100.00m + i,
                quantidadeEstoque = 10
            };
            await _fixture.PostAuthenticatedAsync("/api/v1/produtos", createRequest);
        }

        // Act - Obter última página
        var responsePageCount = await _fixture.GetAuthenticatedAsync("/api/v1/produtos/paged?pageSize=10");
        var contentPageCount = await responsePageCount.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProdutoDto>>>();
        var lastPageNumber = contentPageCount.Dados.TotalPages;

        var responseLastPage = await _fixture.GetAuthenticatedAsync($"/api/v1/produtos/paged?pageNumber={lastPageNumber}&pageSize=10");
        var contentLastPage = await responseLastPage.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProdutoDto>>>();

        // Assert
        contentLastPage.Dados.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetPagedProdutos_WithoutAuthentication_ShouldReturn401()
    {
        // Arrange - Não fazer login

        // Act
        var response = await _fixture.HttpClient!.GetAsync("/api/v1/produtos/paged");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
