using System.Linq.Expressions;
using Obss.CRM.Domain.Entities;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Specifications;

namespace Obss.CRM.Domain.Specifications;

public sealed class CustomerByTypeSpecification : Specification<Customer>
{
    private readonly CustomerType _customerType;

    public CustomerByTypeSpecification(CustomerType customerType)
    {
        _customerType = customerType;
    }

    public override Expression<Func<Customer, bool>> ToExpression()
    {
        return customer => customer.CustomerType == _customerType;
    }
}
