using System.Linq.Expressions;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Domain.Specifications;

namespace Obss.CRM.Domain.Specifications;

public sealed class ActiveCustomerSpecification : Specification<Customer>
{
    public override Expression<Func<Customer, bool>> ToExpression()
    {
        return customer => customer.IsActive;
    }
}
