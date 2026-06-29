using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetContactsByCustomer;

public sealed record GetContactsByCustomerQuery(Guid CustomerId) : IRequest<Result<IReadOnlyList<ContactDto>>>;
