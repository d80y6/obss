using Obss.Rating.Domain.Entities;

namespace Obss.Rating.Domain.DomainServices;

public interface IPromotionEngine
{
    PromotionResult CalculateBestDiscount(IEnumerable<Promotion> promotions, BillLine line);
    IEnumerable<Promotion> GetApplicablePromotions(IEnumerable<Promotion> promotions, BillLine line);
}
