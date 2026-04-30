using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TL.Exemplo.Application.Common.Models;
using Xunit;

namespace TL.Exemplo.Tests.Integration;

[Collection("Integration Tests")]
public class ProdutosIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;

    public ProdutosIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateProduto_WithValidData_ShouldReturn201()
    {
        // Arrange
        await _fixture.LoginAsync();
        var createRequest = new
        {
            nome = "Notebook Dell XPS",
            descricao = "Laptop high-end com processador i7",
            preco = 3500.00m,
            quantidadeEstoque = 5
        };

        // Act
        var response = await _fixture.PostAuthenticatedAsync("/api/v1/produtos", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>();
        content.Sucesso.Should().BeTrue();
        content.Dados.Should().NotBeNull();
        content.Dados.Nome.Should().Be("Notebook Dell XPS");
        content.Dados.Preco.Should().Be(3500.00m);
    }

    [Fact]
    public async Task CreateProduto_WithInvalidData_ShouldReturn400()
    {
        // Arrange
        await _fixture.LoginAsync();
        var createRequest = new
        {
            nome = "AB", // Muito curto
            descricao = "Descrição",
            preco = 100.00m,
            quantidadeEstoque = 5
        };

        // Act
        var response = await _fixture.PostAuthenticatedAsync("/api/v1/produtos", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllProdutos_ShouldReturnOk()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Act
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/produtos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<ProdutoDto>>>();
        content.Sucesso.Should().BeTrue();
        content.Dados.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProdutoById_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Primeiro, criar um produto
        var createRequest = new
        {
            nome = "Monitor LG 27\"",
            descricao = "Monitor 4K",
            preco = 1200.00m,
            quantidadeEstoque = 10
        };
        var createResponse = await _fixture.PostAuthenticatedAsync("/api/v1/produtos", createRequest);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>();
        var produtoId = createdProduct.Dados.Id;

        // Act
        var response = await _fixture.GetAuthenticatedAsync($"/api/v1/produtos/{produtoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>();
        content.Sucesso.Should().BeTrue();
        content.Dados.Nome.Should().Be("Monitor LG 27\"");
    }

    [Fact]
    public async Task GetProdutoById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Act
        var response = await _fixture.GetAuthenticatedAsync("/api/v1/produtos/9999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProduto_WithValidData_ShouldReturn200()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Criar um produto
        var createRequest = new
        {
            nome = "Teclado Mecânico",
            descricao = "RGB Switches Blue",
            preco = 400.00m,
            quantidadeEstoque = 20
        };
        var createResponse = await _fixture.PostAuthenticatedAsync("/api/v1/produtos", createRequest);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>();
        var produtoId = createdProduct.Dados.Id;

        // Atualizar o produto
        var updateRequest = new
        {
            id = produtoId,
            nome = "Teclado Mecânico RGB",
            descricao = "Switches Red",
            preco = 450.00m,
            quantidadeEstoque = 15
        };

        // Act
        var response = await _fixture.PutAuthenticatedAsync($"/api/v1/produtos/{produtoId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verificar que foi atualizado
        var getResponse = await _fixture.GetAuthenticatedAsync($"/api/v1/produtos/{produtoId}");
        var updatedProduct = await getResponse.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>();
        updatedProduct.Dados.Nome.Should().Be("Teclado Mecânico RGB");
        updatedProduct.Dados.Preco.Should().Be(450.00m);
    }

    [Fact]
    public async Task DeleteProduto_WithValidId_ShouldReturn200()
    {
        // Arrange
        await _fixture.LoginAsync();

        // Criar um produto
        var createRequest = new
        {
            nome = "Mouse Logitech",
            descricao = "Wireless",
            preco = 150.00m,
            quantidadeEstoque = 30
        };
        var createResponse = await _fixture.PostAuthenticatedAsync("/api/v1/produtos", createRequest);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>();
        var produtoId = createdProduct.Dados.Id;

        // Act
        var response = await _fixture.DeleteAuthenticatedAsync($"/api/v1/produtos/{produtoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verificar que foi deletado
        var getResponse = await _fixture.GetAuthenticatedAsync($"/api/v1/produtos/{produtoId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduto_WithoutAuthentication_ShouldReturn401()
    {
        // Arrange - Não fazer login
        var createRequest = new
        {
            nome = "Produto Teste",
            descricao = "Descrição",
            preco = 100.00m,
            quantidadeEstoque = 5
        };

        // Act
        var response = await _fixture.HttpClient!.PostAsJsonAsync("/api/v1/produtos", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CompleteFlowTest_CreateUpdateGetDelete()
    {
        // Arrange
        await _fixture.LoginAsync();

        // 1. Criar
        var createRequest = new
        {
            nome = "Webcam HD",
            descricao = "1080p",
            preco = 250.00m,
            quantidadeEstoque = 12
        };
        var createResponse = await _fixture.PostAuthenticatedAsync("/api/v1/produtos", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>();
        var produtoId = createdProduct.Dados.Id;

        // 2. Obter
        var getResponse = await _fixture.GetAuthenticatedAsync($"/api/v1/produtos/{produtoId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Atualizar
        var updateRequest = new
        {
            id = produtoId,
            nome = "Webcam Full HD",
            descricao = "2K",
            preco = 300.00m,
            quantidadeEstoque = 10
        };
        var updateResponse = await _fixture.PutAuthenticatedAsync($"/api/v1/produtos/{produtoId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Deletar
        var deleteResponse = await _fixture.DeleteAuthenticatedAsync($"/api/v1/produtos/{produtoId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Verificar que foi deletado
        var finalGetResponse = await _fixture.GetAuthenticatedAsync($"/api/v1/produtos/{produtoId}");
        finalGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
