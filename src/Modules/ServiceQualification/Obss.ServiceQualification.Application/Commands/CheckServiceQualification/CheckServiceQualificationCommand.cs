using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceQualification.Application.Commands.CheckServiceQualification;

public sealed record CheckServiceQualificationCommand : IRequest<Result>;
