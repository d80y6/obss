using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.ValidateConfiguration;

public sealed record ValidateConfigurationQuery(
    Guid ProductId,
    List<SelectedOptionDto> SelectedOptions) : IRequest<Result<ConfigurationValidationResultDto>>;
