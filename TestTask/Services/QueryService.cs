using Microsoft.EntityFrameworkCore;
using TestTask.Data;
using TestTask.Entities;
using TestTask.Helpers;

namespace TestTask.Services;

public class QueryService
{
    private readonly ProductContext _context;
    private readonly List<Customer> _customers = new List<Customer>();
    private readonly List<TvProducts> _tvProducts = new List<TvProducts>();
    private readonly List<DslProducts> _dslProducts = new List<DslProducts>();

    public QueryService(ProductContext context)
    {
        _context = context;
        _customers = _context.Customers.ToList();
        _tvProducts = _context.TvProducts.ToList();
        _dslProducts = _context.DslProducts.ToList();

    }

    public List<Customer> GetCustomersWithOnlyActiveTvProducts()
    {
        return GetCustomersWithOnlyActiveProducts(
            requireActiveTv: true,
            requireActiveDsl: false
        );
    }

    public List<Customer> GetCustomersWithOnlyActiveDslProducts()
    {
        return GetCustomersWithOnlyActiveProducts(
            requireActiveTv: false,
            requireActiveDsl: true
        );
    }

    private List<Customer> GetCustomersWithOnlyActiveProducts(bool requireActiveTv, bool requireActiveDsl)
    {
        var result = new List<Customer>();

        foreach (var customer in _customers)
        {
            var hasActiveTv = _tvProducts
                .Any(tv => tv.CustomerId == customer.Id && DateTimeExtension.IsDateActive(tv.StartDate, tv.EndDate));

            var hasActiveDsl = _dslProducts
                .Any(dsl => dsl.CustomerId == customer.Id && DateTimeExtension.IsDateActive(dsl.StartDate, dsl.EndDate));

            if ((requireActiveTv ? hasActiveTv : !hasActiveTv) &&
                (requireActiveDsl ? hasActiveDsl : !hasActiveDsl))
            {
                result.Add(customer);
            }
        }

        return result;
    }
}
