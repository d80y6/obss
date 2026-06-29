using Mapster;
using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.CRM.Application.Commands.AddContact;

public sealed class AddContactCommandHandler : IRequestHandler<AddContactCommand, Result<ContactDto>>
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddContactCommandHandler(IRepository<Customer> customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ContactDto>> Handle(AddContactCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer is null)
            return Result.Failure<ContactDto>(Error.NotFound(nameof(Customer), request.CustomerId));

        var email = Email.Create(request.Email);

        PhoneNumber? phoneNumber = null;
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            phoneNumber = PhoneNumber.Create(request.PhoneNumber, request.CountryCode ?? "+967");
        }

        PhoneNumber? mobileNumber = null;
        if (!string.IsNullOrWhiteSpace(request.MobileNumber))
        {
            mobileNumber = PhoneNumber.Create(request.MobileNumber, request.MobileCountryCode ?? "+967");
        }

        var contact = new Contact(
            Guid.NewGuid(),
            customer.Id,
            request.FirstName,
            request.LastName,
            email,
            phoneNumber,
            mobileNumber,
            request.Position,
            request.IsPrimary,
            request.IsBilling,
            request.IsTechnical);

        customer.AddContact(contact);
        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(contact.Adapt<ContactDto>());
    }
}
