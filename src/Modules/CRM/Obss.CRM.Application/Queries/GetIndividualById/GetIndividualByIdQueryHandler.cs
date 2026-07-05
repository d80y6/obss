using Mapster;
using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetIndividualById;

public sealed class GetIndividualByIdQueryHandler : IRequestHandler<GetIndividualByIdQuery, Result<IndividualDto>>
{
    private readonly IRepository<Individual> _individualRepository;

    public GetIndividualByIdQueryHandler(IRepository<Individual> individualRepository)
    {
        _individualRepository = individualRepository;
    }

    public async Task<Result<IndividualDto>> Handle(GetIndividualByIdQuery request, CancellationToken cancellationToken)
    {
        var individual = await _individualRepository.GetByIdAsync(request.Id, cancellationToken);
        if (individual is null)
            return Result.Failure<IndividualDto>(Error.NotFound("Individual", request.Id));

        return Result.Success(individual.Adapt<IndividualDto>());
    }
}
