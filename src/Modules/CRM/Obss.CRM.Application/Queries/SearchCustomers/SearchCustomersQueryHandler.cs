using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.SearchCustomers;

public sealed class SearchCustomersQueryHandler : IRequestHandler<SearchCustomersQuery, Result<IReadOnlyList<CustomerDto>>>
{
    private readonly ICustomerRepository _customerRepository;

    public SearchCustomersQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<IReadOnlyList<CustomerDto>>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.GetFilteredAsync(
            request.TenantId,
            request.Status,
            request.CustomerType,
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken);

        var result = customers.Adapt<List<CustomerDto>>();
        return Result.Success<IReadOnlyList<CustomerDto>>(result);
    }
}
