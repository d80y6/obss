using System.Linq.Expressions;
using Mapster;
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.Provisioning.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.Specifications;

namespace Obss.Provisioning.Application.Queries.GetServiceOrders;

public sealed class GetServiceOrdersQueryHandler : IRequestHandler<GetServiceOrdersQuery, Result<PaginatedResult<ServiceOrderDto>>>
{
    private readonly IServiceOrderRepository _repository;

    public GetServiceOrdersQueryHandler(IServiceOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PaginatedResult<ServiceOrderDto>>> Handle(GetServiceOrdersQuery request, CancellationToken cancellationToken)
    {
        var spec = new ServiceOrderSpecification(request.State, request.ExternalId);
        var items = await _repository.GetListAsync(spec, cancellationToken);
        var total = await _repository.CountAsync(spec, cancellationToken);

        var paged = items
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Adapt<List<ServiceOrderDto>>();

        return Result.Success(new PaginatedResult<ServiceOrderDto>(paged, total, request.Page, request.PageSize));
    }
}

internal sealed class ServiceOrderSpecification : Specification<ServiceOrder>
{
    private readonly string? _state;
    private readonly string? _externalId;

    public ServiceOrderSpecification(string? state, string? externalId)
    {
        _state = state;
        _externalId = externalId;
    }

    public override Expression<Func<ServiceOrder, bool>> ToExpression()
    {
        Expression<Func<ServiceOrder, bool>> expression = _ => true;

        if (!string.IsNullOrWhiteSpace(_state))
        {
            Expression<Func<ServiceOrder, bool>> stateExpr = o => o.State.ToString() == _state;
            expression = Combine(expression, stateExpr);
        }

        if (!string.IsNullOrWhiteSpace(_externalId))
        {
            Expression<Func<ServiceOrder, bool>> extIdExpr = o => o.ExternalId == _externalId;
            expression = Combine(expression, extIdExpr);
        }

        return expression;
    }

    private static Expression<Func<T, bool>> Combine<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T));
        var body = Expression.AndAlso(
            Expression.Invoke(left, parameter),
            Expression.Invoke(right, parameter));
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
