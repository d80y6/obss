using Mapster;
using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Application.Commands.CreateIndividual;

public sealed class CreateIndividualCommandHandler : IRequestHandler<CreateIndividualCommand, Result<IndividualDto>>
{
    private readonly IRepository<Individual> _individualRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateIndividualCommandHandler(IRepository<Individual> individualRepository, IUnitOfWork unitOfWork)
    {
        _individualRepository = individualRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IndividualDto>> Handle(CreateIndividualCommand request, CancellationToken cancellationToken)
    {
        var individual = Individual.Create(
            request.FirstName,
            request.LastName,
            request.MiddleName,
            request.Salutation,
            request.Title,
            request.BirthDate,
            request.Nationality,
            request.Gender);

        await _individualRepository.AddAsync(individual, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(individual.Adapt<IndividualDto>());
    }
}
