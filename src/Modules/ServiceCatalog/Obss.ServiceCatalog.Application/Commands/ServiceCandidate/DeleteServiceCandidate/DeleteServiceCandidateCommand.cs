using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCandidate.DeleteServiceCandidate;

public sealed record DeleteServiceCandidateCommand(Guid Id) : IRequest;
