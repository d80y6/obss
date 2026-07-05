using MediatR;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.RemoveIdentityDocument;

public sealed class RemoveIdentityDocumentCommandHandler : IRequestHandler<RemoveIdentityDocumentCommand, Result>
{
    private readonly IRepository<Individual> _individualRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveIdentityDocumentCommandHandler(IRepository<Individual> individualRepository, IUnitOfWork unitOfWork)
    {
        _individualRepository = individualRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveIdentityDocumentCommand request, CancellationToken cancellationToken)
    {
        var individual = await _individualRepository.GetByIdAsync(request.IndividualId, cancellationToken);
        if (individual is null)
            return Result.Failure(Error.NotFound("Individual", request.IndividualId));

        individual.RemoveDocument(request.DocumentId);

        await _individualRepository.UpdateAsync(individual, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
