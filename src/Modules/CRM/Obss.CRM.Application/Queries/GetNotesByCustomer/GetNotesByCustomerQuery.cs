using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetNotesByCustomer;

public sealed record GetNotesByCustomerQuery(Guid CustomerId) : IRequest<Result<IReadOnlyList<CustomerNoteDto>>>;
