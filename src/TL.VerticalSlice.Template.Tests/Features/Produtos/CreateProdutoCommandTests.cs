using FluentAssertions;
using Moq;
using TL.VerticalSlice.Template.Application.Common.Models;
using TL.VerticalSlice.Template.Application.Contracts.Repositories;
using TL.VerticalSlice.Template.Application.Features.Samples.Commands.CreateSample;
using TL.VerticalSlice.Template.Domain.Entities;
using Xunit;

namespace TL.VerticalSlice.Template.Tests.Features.Samples;

public class CreateSampleCommandTests
{
    private readonly Mock<ISampleRepository> _repositoryMock;

    public CreateSampleCommandTests()
    {
        _repositoryMock = new Mock<ISampleRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProduct()
    {
        // Arrange
        var command = new CreateSampleCommand(
            Nome: "Sample Teste",
            Descricao: "DescriÃ§Ã£o teste",
            Preco: 100.50m,
            QuantidadeEstoque: 10
        );

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Sample>()))
            .ReturnsAsync(1);

        var handler = new CreateSampleCommandHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("Sample Teste");
        result.Preco.Should().Be(100.50m);
        result.QuantidadeEstoque.Should().Be(10);
        result.Ativo.Should().BeTrue();

        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Sample>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidName_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateSampleCommand(
            Nome: "AB", // Nome muito curto
            Descricao: "DescriÃ§Ã£o teste",
            Preco: 100.50m,
            QuantidadeEstoque: 10
        );

        var validator = new CreateSampleCommandValidator();

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
        var command = new CreateSampleCommand(
            Nome: "Sample Teste",
            Descricao: "DescriÃ§Ã£o teste",
            Preco: 0, // PreÃ§o invÃ¡lido
            QuantidadeEstoque: 10
        );

        var validator = new CreateSampleCommandValidator();

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
        var command = new CreateSampleCommand(
            Nome: "Sample Teste",
            Descricao: "DescriÃ§Ã£o teste",
            Preco: 100.50m,
            QuantidadeEstoque: -5 // Quantidade negativa
        );

        var validator = new CreateSampleCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "QuantidadeEstoque");
    }
}

