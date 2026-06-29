using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Application.Commands.CreateOffer;

public sealed class CreateOfferCommandHandler : IRequestHandler<CreateOfferCommand, Result<OfferDto>>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOfferCommandHandler(
        IOfferRepository offerRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _offerRepository = offerRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OfferDto>> Handle(CreateOfferCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure<OfferDto>(Error.NotFound(nameof(Domain.Domain.Entities.Product), request.ProductId));

        var offer = Offer.Create(
            request.TenantId,
            request.Name,
            request.Description,
            request.OfferType,
            request.IsContract,
            request.ContractDurationMonths,
            request.BillingPeriod,
            request.TaxInclusive,
            request.SortOrder,
            request.ValidFrom,
            request.ValidTo);

        if (request.Pricings is not null)
        {
            foreach (var pricing in request.Pricings)
            {
                var offerPricing = new OfferPricing(
                    Guid.NewGuid(),
                    offer.Id,
                    pricing.PricingType,
                    pricing.Currency,
                    pricing.RecurringPrice,
                    pricing.OneTimePrice,
                    pricing.UsagePrice,
                    pricing.UnitOfMeasure,
                    pricing.MinQuantity,
                    pricing.MaxQuantity,
                    pricing.IsActive);

                offer.AddPricing(offerPricing);
            }
        }

        product.AddOffer(new ProductOffer(Guid.NewGuid(), product.Id, offer.Id, true, false));

        await _offerRepository.AddAsync(offer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(offer.Adapt<OfferDto>());
    }
}
