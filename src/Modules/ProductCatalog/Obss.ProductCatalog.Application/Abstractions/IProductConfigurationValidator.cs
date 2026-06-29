using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Application.Abstractions;

public interface IProductConfigurationValidator
{
    ConfigurationValidationResultDto Validate(
        List<SelectedOptionDto> selectedOptions,
        List<ProductConfigurationRule> rules,
        List<ProductOption> options);
}
