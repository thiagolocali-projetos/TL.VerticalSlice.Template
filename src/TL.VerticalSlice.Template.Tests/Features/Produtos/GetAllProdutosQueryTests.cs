using FluentAssertions;
using Moq;
using TL.VerticalSlice.Template.Application.Contracts.Repositories;
using TL.VerticalSlice.Template.Application.Features.Samples.Queries.GetAllSamples;
using TL.VerticalSlice.Template.Domain.Entities;
using Xunit;

namespace TL.VerticalSlice.Template.Tests.Features.Samples;

public class GetAllSamplesQueryTests
{
    private readonly Mock<ISampleRepository> _repositoryMock;

    public GetAllSamplesQueryTests()
    {
        _repositoryMock = new Mock<ISampleRepository>();
    }

    [Fact]
    public async Task Handle_WithAllProducts_ShouldReturnAllProducts()
    {
        // Arrange
        var Samples = new List<Sample>
        {
            new() { Id = 1, Nome = "Sample 1", Preco = 100, QuantidadeEstoque = 10, Ativo = true },
            new() { Id = 2, Nome = "Sample 2", Preco = 200, QuantidadeEstoque = 20, Ativo = true },
            new() { Id = 3, Nome = "Sample 3", Preco = 300, QuantidadeEstoque = 0, Ativo = false }
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(Samples);

        var query = new GetAllSamplesQuery(null);
        var handler = new GetAllSamplesQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(p => p.Should().NotBeNull());

        _repositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithOnlyActive_ShouldReturnOnlyActiveSamples()
    {
        // Arrange
        var Samples = new List<Sample>
        {
            new() { Id = 1, Nome = "Sample 1", Preco = 100, QuantidadeEstoque = 10, Ativo = true },
            new() { Id = 2, Nome = "Sample 2", Preco = 200, QuantidadeEstoque = 20, Ativo = true }
        };

        _repositoryMock
            .Setup(x => x.GetAllAtivosAsync())
            .ReturnsAsync(Samples);

        var query = new GetAllSamplesQuery(true);
        var handler = new GetAllSamplesQueryHandler(_repositoryMock.Object);

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
            .ReturnsAsync(new List<Sample>());

        var query = new GetAllSamplesQuery(null);
        var handler = new GetAllSamplesQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();

        _repositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }
}

