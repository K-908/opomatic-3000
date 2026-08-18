namespace OpoMatic3000.Application.Common.Exceptions;

public sealed class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string message)
        : base(message)
    {
    }
}
