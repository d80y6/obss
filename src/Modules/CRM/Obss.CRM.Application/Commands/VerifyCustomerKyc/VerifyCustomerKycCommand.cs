using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.VerifyCustomerKyc;

public sealed record VerifyCustomerKycCommand(Guid CustomerId, string VerifiedBy, bool IsApproved) : IRequest<Result>;
