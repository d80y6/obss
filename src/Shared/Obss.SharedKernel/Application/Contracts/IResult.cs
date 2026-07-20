namespace Obss.SharedKernel.Application.Contracts;

public interface IOperationResult
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    Error Error { get; }
}