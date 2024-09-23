using Microsoft.EntityFrameworkCore;
using WebAPIProduct.Data;
using WebAPIProduct.Models;
using WebAPIProduct.Data;
using WebAPIProduct.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("ProductCatalogDB"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    SeedData(context); // Call the seeding method
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

void SeedData(ApplicationDbContext context)
{
    if (!context.Categories.Any())
    {
        var categories = new List<Category>
        {
            new Category { Name = "Electronics" },
            new Category { Name = "Books" }
        };

        context.Categories.AddRange(categories);
        context.SaveChanges();
    }

    if (!context.Products.Any())
    {
        var products = new List<Product>
        {
            new Product { Name = "Smartphone", Description = "Latest model", Price = 699, CategoryId = 1 },
            new Product { Name = "Laptop", Description = "Gaming laptop", Price = 1200, CategoryId = 1 },
            new Product { Name = "Novel", Description = "Fiction book", Price = 20, CategoryId = 2 }
        };

        context.Products.AddRange(products);
        context.SaveChanges();
    }
}
