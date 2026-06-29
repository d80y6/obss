using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetProvisioningTemplates;

public sealed record GetProvisioningTemplatesQuery : IRequest<Result<IReadOnlyList<ProvisioningTemplateDto>>>;
