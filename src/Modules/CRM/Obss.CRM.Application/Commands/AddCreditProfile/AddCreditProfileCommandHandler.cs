using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.CRM.Application.Commands.AddCreditProfile;

public sealed class AddCreditProfileCommandHandler : IRequestHandler<AddCreditProfileCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCreditProfileCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddCreditProfileCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
            return Result.Failure(Error.NotFound("Customer", request.CustomerId));

        var validFor = new TimePeriod(request.ValidFrom, request.ValidUntil);
        var profile = new CreditProfile(Guid.NewGuid(), request.CustomerId, request.Score, request.ScoreType, validFor, request.RiskRating);

        customer.AddCreditProfile(profile);

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
