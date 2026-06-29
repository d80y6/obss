using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.IAM.Domain.Exceptions;

[Serializable]
public sealed class DuplicateEmailException : DomainException
{
    public DuplicateEmailException() { }
    public DuplicateEmailException(string email)
        : base($"A user with email '{email}' already exists.") { }
    public DuplicateEmailException(string message, Exception innerException)
        : base(message, innerException) { }
    private DuplicateEmailException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class DuplicateUsernameException : DomainException
{
    public DuplicateUsernameException() { }
    public DuplicateUsernameException(string username)
        : base($"A user with username '{username}' already exists.") { }
    public DuplicateUsernameException(string message, Exception innerException)
        : base(message, innerException) { }
    private DuplicateUsernameException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InvalidRoleException : DomainException
{
    public InvalidRoleException() { }
    public InvalidRoleException(string message)
        : base(message) { }
    public InvalidRoleException(string message, Exception innerException)
        : base(message, innerException) { }
    private InvalidRoleException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
