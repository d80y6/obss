using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AddRelatedParty;

public sealed class AddRelatedPartyCommandHandler : IRequestHandler<AddRelatedPartyCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddRelatedPartyCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddRelatedPartyCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
            return Result.Failure(Error.NotFound("Customer", request.CustomerId));

        customer.AddRelatedParty(new RelatedParty(request.Name, request.Role, request.ReferredId, request.ReferredType));

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
