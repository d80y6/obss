using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetCustomerById;

public sealed record GetCustomerByIdQuery(Guid CustomerId) : IRequest<Result<CustomerDto>>;
