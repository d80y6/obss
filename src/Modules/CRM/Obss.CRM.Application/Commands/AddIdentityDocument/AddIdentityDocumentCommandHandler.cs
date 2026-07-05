using MediatR;
using Obss.CRM.Domain.Entities;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AddIdentityDocument;

public sealed class AddIdentityDocumentCommandHandler : IRequestHandler<AddIdentityDocumentCommand, Result>
{
    private readonly IRepository<Individual> _individualRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddIdentityDocumentCommandHandler(IRepository<Individual> individualRepository, IUnitOfWork unitOfWork)
    {
        _individualRepository = individualRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddIdentityDocumentCommand request, CancellationToken cancellationToken)
    {
        var individual = await _individualRepository.GetByIdAsync(request.IndividualId, cancellationToken);
        if (individual is null)
            return Result.Failure(Error.NotFound("Individual", request.IndividualId));

        if (!Enum.TryParse<DocumentType>(request.DocumentType, true, out var documentType))
            return Result.Failure(Error.Validation($"Invalid document type: {request.DocumentType}"));

        var document = new IdentityDocument(
            Guid.NewGuid(),
            request.IndividualId,
            documentType,
            request.DocumentNumber,
            request.IssuingAuthority,
            request.IssuingCountry,
            request.IssuedDate,
            request.ExpiryDate);

        individual.AddDocument(document);

        await _individualRepository.UpdateAsync(individual, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
