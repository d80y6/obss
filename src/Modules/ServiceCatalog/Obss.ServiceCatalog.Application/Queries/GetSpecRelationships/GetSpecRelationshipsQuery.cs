using MediatR;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetSpecRelationships;

public sealed record GetSpecRelationshipsQuery(Guid ServiceSpecificationId) : IRequest<List<ServiceSpecRelationshipDto>>;
