using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetProductsByCustomer;

public sealed record GetProductsByCustomerQuery(Guid CustomerId) : IRequest<Result<List<ProductDto>>>;
