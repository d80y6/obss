using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.AddConfigurationRule;

public sealed record AddConfigurationRuleCommand(
    Guid ProductId,
    RuleType RuleType,
    Guid? TargetProductId,
    string? TargetOption,
    string? Condition) : IRequest<Result<ProductConfigurationRuleDto>>;
