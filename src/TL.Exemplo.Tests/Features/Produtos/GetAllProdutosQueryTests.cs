using FluentAssertions;
using Moq;
using TL.Exemplo.Application.Contracts.Repositories;
using TL.Exemplo.Application.Features.Produtos.Queries.GetAllProdutos;
using TL.Exemplo.Domain.Entities;
using Xunit;

namespace TL.Exemplo.Tests.Features.Produtos;

public class GetAllProdutosQueryTests
{
    private readonly Mock<IProdutoRepository> _repositoryMock;

    public GetAllProdutosQueryTests()
    {
        _repositoryMock = new Mock<IProdutoRepository>();
    }

    [Fact]
    public async Task Handle_WithAllProducts_ShouldReturnAllProducts()
    {
        // Arrange
        var produtos = new List<Produto>
        {
            new() { Id = 1, Nome = "Produto 1", Preco = 100, QuantidadeEstoque = 10, Ativo = true },
            new() { Id = 2, Nome = "Produto 2", Preco = 200, QuantidadeEstoque = 20, Ativo = true },
            new() { Id = 3, Nome = "Produto 3", Preco = 300, QuantidadeEstoque = 0, Ativo = false }
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(produtos);

        var query = new GetAllProdutosQuery(null);
        var handler = new GetAllProdutosQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(p => p.Should().NotBeNull());

        _repositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithOnlyActive_ShouldReturnOnlyActiveProdutos()
    {
        // Arrange
        var produtos = new List<Produto>
        {
            new() { Id = 1, Nome = "Produto 1", Preco = 100, QuantidadeEstoque = 10, Ativo = true },
            new() { Id = 2, Nome = "Produto 2", Preco = 200, QuantidadeEstoque = 20, Ativo = true }
        };

        _repositoryMock
            .Setup(x => x.GetAllAtivosAsync())
            .ReturnsAsync(produtos);

        var query = new GetAllProdutosQuery(true);
        var handler = new GetAllProdutosQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Ativo.Should().BeTrue());

        _repositoryMock.Verify(x => x.GetAllAtivosAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Produto>());

        var query = new GetAllProdutosQuery(null);
        var handler = new GetAllProdutosQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();

        _repositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }
}
