using Mapster;
using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.UpdateIndividual;

public sealed class UpdateIndividualCommandHandler : IRequestHandler<UpdateIndividualCommand, Result<IndividualDto>>
{
    private readonly IRepository<Individual> _individualRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateIndividualCommandHandler(IRepository<Individual> individualRepository, IUnitOfWork unitOfWork)
    {
        _individualRepository = individualRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IndividualDto>> Handle(UpdateIndividualCommand request, CancellationToken cancellationToken)
    {
        var individual = await _individualRepository.GetByIdAsync(request.Id, cancellationToken);
        if (individual is null)
            return Result.Failure<IndividualDto>(Error.NotFound("Individual", request.Id));

        individual.UpdateDetails(
            request.FirstName,
            request.LastName,
            request.MiddleName,
            request.Salutation,
            request.Title,
            request.BirthDate,
            request.Nationality,
            request.Gender);

        await _individualRepository.UpdateAsync(individual, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(individual.Adapt<IndividualDto>());
    }
}
