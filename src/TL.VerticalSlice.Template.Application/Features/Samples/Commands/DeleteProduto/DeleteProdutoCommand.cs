using FluentValidation;
using MediatR;
using TL.VerticalSlice.Template.Application.Common.Exceptions;
using TL.VerticalSlice.Template.Application.Contracts.Repositories;

namespace TL.VerticalSlice.Template.Application.Features.Samples.Commands.DeleteSample;

// â”€â”€ Command â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public record DeleteSampleCommand(int Id) : IRequest<Unit>;

// â”€â”€ Validator â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class DeleteSampleCommandValidator : AbstractValidator<DeleteSampleCommand>
{
    public DeleteSampleCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("O Id deve ser maior que zero.");
    }
}

// â”€â”€ Handler â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class DeleteSampleCommandHandler : IRequestHandler<DeleteSampleCommand, Unit>
{
    private readonly ISampleRepository _repository;

    public DeleteSampleCommandHandler(ISampleRepository repository)
        => _repository = repository;

    public async Task<Unit> Handle(
        DeleteSampleCommand request,
        CancellationToken cancellationToken)
    {
        var Sample = await _repository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Domain.Entities.Sample), request.Id);

        await _repository.DeleteAsync(Sample.Id);

        return Unit.Value;
    }
}

