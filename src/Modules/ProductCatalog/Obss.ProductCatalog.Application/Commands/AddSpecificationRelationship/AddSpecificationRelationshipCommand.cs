using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.AddSpecificationRelationship;

public sealed record AddSpecificationRelationshipCommand(
    Guid ProductSpecificationId,
    Guid TargetSpecificationId,
    SpecificationRelationshipType RelationshipType,
    string? Role,
    DateTime? ValidFrom,
    DateTime? ValidTo) : IRequest<Result<ProductSpecificationRelationshipDto>>;
