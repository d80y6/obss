using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetNotesByCustomer;

public sealed class GetNotesByCustomerQueryHandler : IRequestHandler<GetNotesByCustomerQuery, Result<IReadOnlyList<CustomerNoteDto>>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetNotesByCustomerQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<IReadOnlyList<CustomerNoteDto>>> Handle(GetNotesByCustomerQuery request, CancellationToken cancellationToken)
    {
        var notes = await _customerRepository.GetNotesByCustomerAsync(request.CustomerId, cancellationToken);
        return Result.Success(notes.Adapt<IReadOnlyList<CustomerNoteDto>>());
    }
}
