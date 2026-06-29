using Mapster;
using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.CRM.Application.Commands.CreateCustomer;

public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(IRepository<Customer> customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customerType = Enum.Parse<CustomerType>(request.CustomerType);
        var email = Email.Create(request.Email);

        PhoneNumber? phoneNumber = null;
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            phoneNumber = PhoneNumber.Create(request.PhoneNumber, request.CountryCode ?? "+967");
        }

        var customer = Customer.Create(
            request.TenantId,
            customerType,
            request.CompanyName,
            request.DisplayName,
            request.TaxNumber,
            request.RegistrationNumber,
            email,
            phoneNumber,
            request.Website,
            request.Currency);

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(customer.Adapt<CustomerDto>());
    }
}
