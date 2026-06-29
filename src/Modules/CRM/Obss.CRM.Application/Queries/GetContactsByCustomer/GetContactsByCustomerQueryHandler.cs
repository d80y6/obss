using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetContactsByCustomer;

public sealed class GetContactsByCustomerQueryHandler : IRequestHandler<GetContactsByCustomerQuery, Result<IReadOnlyList<ContactDto>>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetContactsByCustomerQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<IReadOnlyList<ContactDto>>> Handle(GetContactsByCustomerQuery request, CancellationToken cancellationToken)
    {
        var contacts = await _customerRepository.GetContactsByCustomerAsync(request.CustomerId, cancellationToken);
        return Result.Success(contacts.Adapt<IReadOnlyList<ContactDto>>());
    }
}
