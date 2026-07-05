using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetCreditProfiles;

public sealed class GetCreditProfilesQueryHandler : IRequestHandler<GetCreditProfilesQuery, Result<IReadOnlyList<CreditProfileDto>>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCreditProfilesQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<IReadOnlyList<CreditProfileDto>>> Handle(GetCreditProfilesQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
            return Result.Failure<IReadOnlyList<CreditProfileDto>>(Error.NotFound("Customer", request.CustomerId));

        return Result.Success(customer.CreditProfiles.Adapt<List<CreditProfileDto>>() as IReadOnlyList<CreditProfileDto>);
    }
}
