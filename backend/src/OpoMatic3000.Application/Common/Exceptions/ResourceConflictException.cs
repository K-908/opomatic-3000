namespace OpoMatic3000.Application.Common.Exceptions;

public sealed class ResourceConflictException : Exception
{
    public ResourceConflictException(string message)
        : base(message)
    {
    }
}
