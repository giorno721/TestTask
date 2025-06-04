using TestTask.Data;

using var context = new ProductContext();
context.Database.EnsureCreated();