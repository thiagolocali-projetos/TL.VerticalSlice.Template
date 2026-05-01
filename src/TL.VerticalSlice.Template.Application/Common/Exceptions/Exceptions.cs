namespace TL.VerticalSlice.Template.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"'{name}' com identificador '{key}' nÃ£o foi encontrado.") { }

    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException() : base("Ocorreram um ou mais erros de validaÃ§Ã£o.")
        => Errors = new Dictionary<string, string[]>();

    public ValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }
}

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

