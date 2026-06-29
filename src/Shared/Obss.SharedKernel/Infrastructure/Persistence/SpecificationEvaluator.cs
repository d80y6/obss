using Obss.SharedKernel.Domain.Specifications;

namespace Obss.SharedKernel.Infrastructure.Persistence;

public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(
        IQueryable<T> inputQuery,
        Specification<T> specification)
    {
        var query = inputQuery;

        var expression = specification.ToExpression();
        query = query.Where(expression);

        return query;
    }
}
