using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetSegmentById;

public sealed record GetSegmentByIdQuery(Guid SegmentId) : IRequest<Result<CustomerSegmentDto>>;
