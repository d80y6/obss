using MediatR;
using Obss.NumberInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Queries.GetNumberById;

public sealed record GetNumberByIdQuery(Guid Id) : IRequest<Result<TelephoneNumberDto>>;
