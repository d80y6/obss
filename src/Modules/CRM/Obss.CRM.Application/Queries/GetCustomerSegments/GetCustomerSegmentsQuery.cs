using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetCustomerSegments;

public sealed record GetCustomerSegmentsQuery : IRequest<Result<IReadOnlyList<CustomerSegmentDto>>>;
