namespace TL.VerticalSlice.Template.Application.Features.BackgroundJobs;

/// <summary>
/// Interface base para definir um background job no projeto.
/// Cada job implementa essa interface e Ã© registrado no Hangfire.
/// </summary>
public interface IBackgroundJob
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

