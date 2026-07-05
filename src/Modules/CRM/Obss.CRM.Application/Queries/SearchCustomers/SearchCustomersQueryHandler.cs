using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.Contracts;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.SearchCustomers;

public sealed class SearchCustomersQueryHandler : IRequestHandler<SearchCustomersQuery, Result<PaginatedResult<CustomerDto>>>
{
    private readonly ICustomerRepository _customerRepository;

    public SearchCustomersQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<PaginatedResult<CustomerDto>>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.GetFilteredAsync(
            request.TenantId,
            request.Status,
            request.CustomerType,
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken);

        var totalCount = await _customerRepository.GetFilteredCountAsync(
            request.TenantId,
            request.Status,
            request.CustomerType,
            request.SearchTerm,
            cancellationToken);

        var items = customers.Adapt<List<CustomerDto>>();
        return Result.Success(new PaginatedResult<CustomerDto>(items, totalCount));
    }
}
