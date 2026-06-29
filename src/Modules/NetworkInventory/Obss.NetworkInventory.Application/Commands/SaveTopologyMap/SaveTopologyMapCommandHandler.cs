using MediatR;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Commands.SaveTopologyMap;

public sealed class SaveTopologyMapCommandHandler : IRequestHandler<SaveTopologyMapCommand, Result<Guid>>
{
    private readonly IRepository<TopologyMap> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveTopologyMapCommandHandler(IRepository<TopologyMap> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(SaveTopologyMapCommand request, CancellationToken cancellationToken)
    {
        var map = TopologyMap.Create(
            request.Name,
            request.Description,
            request.Configuration);

        await _repository.AddAsync(map, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(map.Id);
    }
}
