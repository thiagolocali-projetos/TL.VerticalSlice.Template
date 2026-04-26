namespace TL.Exemplo.Application.Common.Models;

/// <summary>
/// Envelope de resposta genérico para todos os endpoints da API.
/// </summary>
public sealed class ApiResponse<T>
{
    public bool Sucesso { get; init; }
    public T? Dados { get; init; }
    public string? Mensagem { get; init; }
    public IEnumerable<string> Erros { get; init; } = Enumerable.Empty<string>();

    // Construtor interno — use os factory methods abaixo
    internal ApiResponse() { }

    public static ApiResponse<T> Ok(T dados, string? mensagem = null)
        => new() { Sucesso = true, Dados = dados, Mensagem = mensagem };

    public static ApiResponse<T> Criado(T dados, string mensagem = "Recurso criado com sucesso.")
        => new() { Sucesso = true, Dados = dados, Mensagem = mensagem };

    public static ApiResponse<T> Falha(string mensagem, IEnumerable<string>? erros = null)
        => new() { Sucesso = false, Mensagem = mensagem, Erros = erros ?? Enumerable.Empty<string>() };

    public static ApiResponse<T> NaoEncontrado(string mensagem = "Recurso não encontrado.")
        => new() { Sucesso = false, Mensagem = mensagem };
}

/// <summary>
/// Factory estática para respostas sem dado de retorno (object vazio).
/// Não herda de ApiResponse&lt;T&gt; para evitar conflitos de membros.
/// </summary>
public static class ApiResponse
{
    public static ApiResponse<object> Ok(string mensagem = "Operação realizada com sucesso.")
        => ApiResponse<object>.Ok(new { }, mensagem);

    public static ApiResponse<object> Falha(string mensagem, IEnumerable<string>? erros = null)
        => ApiResponse<object>.Falha(mensagem, erros);

    public static ApiResponse<object> NaoEncontrado(string mensagem = "Recurso não encontrado.")
        => ApiResponse<object>.NaoEncontrado(mensagem);
}
