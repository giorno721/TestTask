using Microsoft.EntityFrameworkCore;
using TestTask.Data;
using TestTask.Services;

using var context = new ProductContext();
context.Database.EnsureCreated();

// task 1
var queryService = new QueryService(context);
var customersWithOnlyActiveTv = queryService.GetCustomersWithOnlyActiveTvProducts();
foreach (var customer in customersWithOnlyActiveTv)
{
    Console.WriteLine($"Customer with only active TV products: {customer.Id}");
}

var customersWithOnlyActiveDsl = queryService.GetCustomersWithOnlyActiveDslProducts();
foreach (var customer in customersWithOnlyActiveDsl)
{
    Console.WriteLine($"Customer with only active DSL products: {customer.Id}");
}