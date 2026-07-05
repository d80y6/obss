using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.CRM.Application.Commands.PartialUpdateCustomer;

public sealed class PartialUpdateCustomerCommandHandler : IRequestHandler<PartialUpdateCustomerCommand, Result<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PartialUpdateCustomerCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerDto>> Handle(PartialUpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer is null)
            return Result.Failure<CustomerDto>(Error.NotFound("Customer", request.Id));

        TimePeriod? validFor = null;
        if (request.ValidFrom.HasValue || request.ValidUntil.HasValue)
            validFor = new TimePeriod(request.ValidFrom, request.ValidUntil);

        customer.UpdateTmfDetails(
            request.Description,
            request.StatusReason,
            request.ExternalId,
            validFor: validFor);

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(customer.Adapt<CustomerDto>());
    }
}
