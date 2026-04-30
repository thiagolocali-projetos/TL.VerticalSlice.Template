namespace TL.Exemplo.Application.Common.Models;

/// <summary>
/// DTO retornado ao enfileirar ou agendar um background job.
/// </summary>
public class JobResultDto
{
    public string JobId { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;  // "Enfileirado", "Agendado", "Processando", "Completado"
    public DateTime EnqueuedAt { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public DateTime? CompletedAt { get; set; }
}
