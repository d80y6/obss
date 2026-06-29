using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetCustomerSegmentAssignments;

public sealed record GetCustomerSegmentAssignmentsQuery(Guid SegmentId) : IRequest<Result<IReadOnlyList<CustomerSegmentAssignmentDto>>>;
