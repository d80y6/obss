using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.VerifyCustomerKyc;

public sealed class VerifyCustomerKycCommandHandler : IRequestHandler<VerifyCustomerKycCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyCustomerKycCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(VerifyCustomerKycCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
            return Result.Failure(Error.NotFound("Customer", request.CustomerId));

        if (customer.Individual is not null)
        {
            if (request.IsApproved)
                customer.Individual.VerifyKyc(request.VerifiedBy);
            else
                customer.Individual.RejectKyc();
        }
        else if (customer.Organization is not null)
        {
            if (request.IsApproved)
                customer.Organization.VerifyKyc(request.VerifiedBy);
            else
                customer.Organization.RejectKyc();
        }
        else
        {
            return Result.Failure(Error.Validation("Customer has no engaged party (Individual or Organization)."));
        }

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
