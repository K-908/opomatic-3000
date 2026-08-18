namespace OpoMatic3000.Application.Common.Exceptions;

public sealed class RequestValidationException : Exception
{
    public RequestValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Corrige los campos indicados.")
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
