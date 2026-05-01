using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TL.VerticalSlice.Template.Application.Common.Models;
using Xunit;

namespace TL.VerticalSlice.Template.Tests.Integration;

[Collection("Integration Tests")]
public class PaginationIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;

    public PaginationIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetPagedSamples_WithDefaultParameters_ShouldReturn200()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Act
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/Samples/paged");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<SampleDto>>>();
        content.Sucesso.Should().BeTrue();
        content.Dados.Should().NotBeNull();
        content.Dados.PageNumber.Should().Be(1);
        content.Dados.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task GetPagedSamples_WithCustomPageSize_ShouldReturn200()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Act
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/Samples/paged?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<SampleDto>>>();
        content.Dados.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetPagedSamples_WithPageSize100_ShouldCapAtMax()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Act - Tentar pageSize acima do mÃ¡ximo (100)
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/Samples/paged?pageNumber=1&pageSize=200");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<SampleDto>>>();
        content.Dados.PageSize.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public async Task GetPagedSamples_WithInvalidPageNumber_ShouldAdjustToMinimum()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Act - Tentar pageNumber 0 ou negativo
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/Samples/paged?pageNumber=0&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<SampleDto>>>();
        content.Dados.PageNumber.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetPagedSamples_ShouldIncludeMetadata()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Act
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/Samples/paged?pageNumber=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<SampleDto>>>();

        content.Dados.Should().NotBeNull();
        content.Dados.Items.Should().NotBeNull();
        content.Dados.PageNumber.Should().Be(1);
        content.Dados.PageSize.Should().Be(20);
        content.Dados.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        content.Dados.TotalPages.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetPagedSamples_WithApenasAtivos_ShouldReturnOnlyActive()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Criar alguns Samples (ativos por padrÃ£o)
        for (int i = 0; i < 3; i++)
        {
            var createRequest = new
            {
                nome = $"Sample Ativo {i}",
                descricao = "DescriÃ§Ã£o",
                preco = 100.00m + i,
                quantidadeEstoque = 10
            };
            await _fixture.PostAuthenticatedAsync("/api/v1/Samples", createRequest);
        }

        // Act
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/Samples/paged?apenasAtivos=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<SampleDto>>>();
        content.Dados.Items.Should().AllSatisfy(p => p.Ativo.Should().BeTrue());
    }

    [Fact]
    public async Task GetPagedSamples_HasNextPage_ShouldBeCorrect()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Criar mÃºltiplos Samples
        for (int i = 0; i < 25; i++)
        {
            var createRequest = new
            {
                nome = $"Sample {i:D2}",
                descricao = "DescriÃ§Ã£o",
                preco = 100.00m + i,
                quantidadeEstoque = 10
            };
            await _fixture.PostAuthenticatedAsync("/api/v1/Samples", createRequest);
        }

        // Act - Primeira pÃ¡gina com tamanho 10
        var response1 = await _fixture.GetAuthenticatedAsync("/api/v1/Samples/paged?pageNumber=1&pageSize=10");
        var content1 = await response1.Content.ReadFromJsonAsync<ApiResponse<PagedResult<SampleDto>>>();

        // Assert
        content1.Dados.HasNextPage.Should().BeTrue();
        content1.Dados.TotalPages.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task GetPagedSamples_LastPage_ShouldNotHaveNextPage()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Criar alguns Samples
        for (int i = 0; i < 5; i++)
        {
            var createRequest = new
            {
                nome = $"Sample Final {i}",
                descricao = "DescriÃ§Ã£o",
                preco = 100.00m + i,
                quantidadeEstoque = 10
            };
            await _fixture.PostAuthenticatedAsync("/api/v1/Samples", createRequest);
        }

        // Act - Obter Ãºltima pÃ¡gina
        var responsePageCount = await _fixture.GetAuthenticatedAsync("/api/v1/Samples/paged?pageSize=10");
        var contentPageCount = await responsePageCount.Content.ReadFromJsonAsync<ApiResponse<PagedResult<SampleDto>>>();
        var lastPageNumber = contentPageCount.Dados.TotalPages;

        var responseLastPage = await _fixture.GetAuthenticatedAsync($"/api/v1/Samples/paged?pageNumber={lastPageNumber}&pageSize=10");
        var contentLastPage = await responseLastPage.Content.ReadFromJsonAsync<ApiResponse<PagedResult<SampleDto>>>();

        // Assert
        contentLastPage.Dados.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetPagedSamples_WithoutAuthentication_ShouldReturn401()
    {
        // Arrange - NÃ£o fazer login

        // Act
        var response = await _fixture.HttpClient!.GetAsync("/api/v1/Samples/paged");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

