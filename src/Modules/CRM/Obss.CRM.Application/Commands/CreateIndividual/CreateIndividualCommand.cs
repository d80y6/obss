using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.CreateIndividual;

public sealed record CreateIndividualCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    string? Salutation,
    string? Title,
    DateTime? BirthDate,
    string? Nationality,
    string? Gender) : IRequest<Result<IndividualDto>>;
