using Mapster;
using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AddNote;

public sealed class AddNoteCommandHandler : IRequestHandler<AddNoteCommand, Result<CustomerNoteDto>>
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddNoteCommandHandler(IRepository<Customer> customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerNoteDto>> Handle(AddNoteCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer is null)
            return Result.Failure<CustomerNoteDto>(Error.NotFound(nameof(Customer), request.CustomerId));

        var category = Enum.Parse<NoteCategory>(request.Category);

        var note = new CustomerNote(
            Guid.NewGuid(),
            customer.Id,
            request.Content,
            category,
            request.CreatedById);

        customer.AddNote(note);
        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(note.Adapt<CustomerNoteDto>());
    }
}
