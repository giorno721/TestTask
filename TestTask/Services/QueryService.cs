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

    public List<TvProducts> GetOverlappingTvProducts()
    {
        var now = DateTime.Now;
        var result = new List<TvProducts>();

        var activeTvProducts = _tvProducts
            .Where(tv => DateTimeExtension.IsDateActive(tv.StartDate, tv.EndDate))
            .ToList();

        var customerGroups = activeTvProducts.GroupBy(tv => tv.CustomerId);

        foreach (var group in customerGroups)
        {
            var customerTvProducts = group.ToList();

            if (customerTvProducts.Count > 1)
            {
                for (int i = 0; i < customerTvProducts.Count; i++)
                {
                    for (int j = i + 1; j < customerTvProducts.Count; j++)
                    {
                        var product1 = customerTvProducts[i];
                        var product2 = customerTvProducts[j];

                        var end1 = product1.EndDate ?? DateTime.MaxValue;
                        var end2 = product2.EndDate ?? DateTime.MaxValue;

                        if (product1.StartDate < end2 && product2.StartDate < end1)
                        {
                            if (!result.Any(r => r.Id == product1.Id))
                                result.Add(product1);
                            if (!result.Any(r => r.Id == product2.Id))
                                result.Add(product2);
                        }
                    }
                }
            }
        }

        return result;
    }
}
