using FluentAssertions;
using Moq;
using TL.Exemplo.Application.Common.Models;
using TL.Exemplo.Application.Contracts.Repositories;
using TL.Exemplo.Application.Features.Produtos.Commands.CreateProduto;
using TL.Exemplo.Domain.Entities;
using Xunit;

namespace TL.Exemplo.Tests.Features.Produtos;

public class CreateProdutoCommandTests
{
    private readonly Mock<IProdutoRepository> _repositoryMock;

    public CreateProdutoCommandTests()
    {
        _repositoryMock = new Mock<IProdutoRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProduct()
    {
        // Arrange
        var command = new CreateProdutoCommand(
            Nome: "Produto Teste",
            Descricao: "Descrição teste",
            Preco: 100.50m,
            QuantidadeEstoque: 10
        );

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Produto>()))
            .ReturnsAsync(1);

        var handler = new CreateProdutoCommandHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("Produto Teste");
        result.Preco.Should().Be(100.50m);
        result.QuantidadeEstoque.Should().Be(10);
        result.Ativo.Should().BeTrue();

        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Produto>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidName_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateProdutoCommand(
            Nome: "AB", // Nome muito curto
            Descricao: "Descrição teste",
            Preco: 100.50m,
            QuantidadeEstoque: 10
        );

        var validator = new CreateProdutoCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Nome");
    }

    [Fact]
    public async Task Handle_WithInvalidPrice_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateProdutoCommand(
            Nome: "Produto Teste",
            Descricao: "Descrição teste",
            Preco: 0, // Preço inválido
            QuantidadeEstoque: 10
        );

        var validator = new CreateProdutoCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Preco");
    }

    [Fact]
    public async Task Handle_WithNegativeQuantity_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateProdutoCommand(
            Nome: "Produto Teste",
            Descricao: "Descrição teste",
            Preco: 100.50m,
            QuantidadeEstoque: -5 // Quantidade negativa
        );

        var validator = new CreateProdutoCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "QuantidadeEstoque");
    }
}
