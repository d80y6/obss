using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.ValidateProductConfiguration;

public sealed record ValidateProductConfigurationCommand(
    Guid ProductId,
    List<SelectedOptionDto> SelectedOptions) : IRequest<Result<ConfigurationValidationResultDto>>;
