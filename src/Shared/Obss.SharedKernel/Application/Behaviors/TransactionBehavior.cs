using System.Reflection;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.SharedKernel.Application.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(IUnitOfWork unitOfWork, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IBaseRequest)
            return await next();

        var requestName = typeof(TRequest).Name;

        try
        {
            var response = await next();

            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var isSuccessProperty = typeof(TResponse).GetProperty("IsSuccess", BindingFlags.Public | BindingFlags.Instance);
                if (isSuccessProperty is not null)
                {
                    var isSuccess = (bool)isSuccessProperty.GetValue(response)!;
                    if (isSuccess)
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
            else
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction failed for request {RequestName}", requestName);
            throw;
        }
    }
}