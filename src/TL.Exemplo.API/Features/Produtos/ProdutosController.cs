using MediatR;
using Microsoft.AspNetCore.Mvc;
using TL.Exemplo.Application.Common.Models;
using TL.Exemplo.Application.Features.Produtos.Commands.CreateProduto;
using TL.Exemplo.Application.Features.Produtos.Commands.DeleteProduto;
using TL.Exemplo.Application.Features.Produtos.Commands.UpdateProduto;
using TL.Exemplo.Application.Features.Produtos.Queries.GetAllProdutos;
using TL.Exemplo.Application.Features.Produtos.Queries.GetProdutoById;

namespace TL.Exemplo.API.Features.Produtos;

/// <summary>
/// Controller responsável pelo gerenciamento de produtos.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ProdutosController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProdutosController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Retorna todos os produtos cadastrados.
    /// </summary>
    /// <param name="apenasAtivos">Filtrar apenas produtos ativos.</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProdutoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? apenasAtivos = null)
    {
        var result = await _mediator.Send(new GetAllProdutosQuery(apenasAtivos));
        return Ok(ApiResponse<IEnumerable<ProdutoDto>>.Ok(result));
    }

    /// <summary>
    /// Retorna um produto pelo Id.
    /// </summary>
    /// <param name="id">Identificador do produto.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProdutoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetProdutoByIdQuery(id));
        return Ok(ApiResponse<ProdutoDto>.Ok(result));
    }

    /// <summary>
    /// Cria um novo produto.
    /// </summary>
    /// <param name="command">Dados do produto a ser criado.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProdutoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProdutoCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            ApiResponse<ProdutoDto>.Criado(result));
    }

    /// <summary>
    /// Atualiza um produto existente.
    /// </summary>
    /// <param name="id">Identificador do produto.</param>
    /// <param name="command">Dados atualizados do produto.</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProdutoCommand command)
    {
        if (id != command.Id)
            return BadRequest(ApiResponse<object>.Falha("O Id da rota não corresponde ao Id do corpo da requisição."));

        await _mediator.Send(command);
        return Ok(ApiResponse.Ok("Produto atualizado com sucesso."));
    }

    /// <summary>
    /// Remove um produto pelo Id.
    /// </summary>
    /// <param name="id">Identificador do produto.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteProdutoCommand(id));
        return Ok(ApiResponse.Ok("Produto removido com sucesso."));
    }
}
