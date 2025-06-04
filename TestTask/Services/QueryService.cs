using Microsoft.EntityFrameworkCore;
using TestTask.Data;
using TestTask.Entities;
using TestTask.Helpers;
using TestTask.Models;

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

    public List<TvProducts> GetOverlappingTvProducts()
    {
        var now = DateTime.Now;
        var result = new List<TvProducts>();

        var activeTvProducts = _tvProducts
            .Where(tv => DateTimeHelper.IsDateActive(tv.StartDate, tv.EndDate))
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

    public List<DuplicateCustomerResult> GetDuplicateCustomerAccounts()
    {
        var now = DateTime.Now;
        var result = new List<DuplicateCustomerResult>();

        var customerGroups = _customers
            .GroupBy(c => new { c.Email, c.Address })
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var group in customerGroups)
        {
            var customersInGroup = group.ToList();

            for (int i = 0; i < customersInGroup.Count; i++)
            {
                for (int j = i + 1; j < customersInGroup.Count; j++)
                {
                    var customer1 = customersInGroup[i];
                    var customer2 = customersInGroup[j];

                    var customer1HasActiveTv = _tvProducts
                        .Any(tv => tv.CustomerId == customer1.Id && DateTimeHelper.IsDateActive(tv.StartDate, tv.EndDate));

                    var customer1HasActiveDsl = _dslProducts
                        .Any(dsl => dsl.CustomerId == customer1.Id && DateTimeHelper.IsDateActive(dsl.StartDate, dsl.EndDate));

                    var customer2HasActiveTv = _tvProducts
                        .Any(tv => tv.CustomerId == customer2.Id && DateTimeHelper.IsDateActive(tv.StartDate, tv.EndDate));

                    var customer2HasActiveDsl = _dslProducts
                        .Any(dsl => dsl.CustomerId == customer2.Id && DateTimeHelper.IsDateActive(dsl.StartDate, dsl.EndDate));

                    if (customer1HasActiveTv && customer2HasActiveDsl)
                    {
                        var startDate = GetLatestStartDate(customer1.Id, customer2.Id);
                        result.Add(new DuplicateCustomerResult
                        {
                            CustomerIdTv = customer1.Id,
                            CustomerIdDsl = customer2.Id,
                            StartDate = startDate
                        });
                    }

                    if (customer2HasActiveTv && customer1HasActiveDsl)
                    {
                        var startDate = GetLatestStartDate(customer2.Id, customer1.Id);
                        result.Add(new DuplicateCustomerResult
                        {
                            CustomerIdTv = customer2.Id,
                            CustomerIdDsl = customer1.Id,
                            StartDate = startDate
                        });
                    }
                }
            }
        }

        return result.Distinct().ToList();
    }

    private List<Customer> GetCustomersWithOnlyActiveProducts(bool requireActiveTv, bool requireActiveDsl)
    {
        var result = new List<Customer>();

        foreach (var customer in _customers)
        {
            var hasActiveTv = _tvProducts
                .Any(tv => tv.CustomerId == customer.Id && DateTimeHelper.IsDateActive(tv.StartDate, tv.EndDate));

            var hasActiveDsl = _dslProducts
                .Any(dsl => dsl.CustomerId == customer.Id && DateTimeHelper.IsDateActive(dsl.StartDate, dsl.EndDate));

            if ((requireActiveTv ? hasActiveTv : !hasActiveTv) &&
                (requireActiveDsl ? hasActiveDsl : !hasActiveDsl))
            {
                result.Add(customer);
            }
        }

        return result;
    }

    private DateTime GetLatestStartDate(int customerIdTv, int customerIdDsl)
    {
        var tvStartDates = _tvProducts
            .Where(tv => tv.CustomerId == customerIdTv && DateTimeHelper.IsDateActive(tv.StartDate, tv.EndDate))
            .Select(tv => tv.StartDate);

        var dslStartDates = _dslProducts
            .Where(dsl => dsl.CustomerId == customerIdDsl && DateTimeHelper.IsDateActive(dsl.StartDate, dsl.EndDate))
            .Select(dsl => dsl.StartDate);

        var allStartDates = tvStartDates.Concat(dslStartDates);
        return allStartDates.Any() ? allStartDates.Max() : DateTime.MinValue;
    }
}
