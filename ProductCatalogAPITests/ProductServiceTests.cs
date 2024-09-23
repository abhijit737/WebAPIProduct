using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebAPIProduct.Controllers;
using WebAPIProduct.Data;
using WebAPIProduct.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using WebAPIProduct.Controllers;
public class ProductServiceTests
{
    private readonly Mock<DbSet<Product>> _productDbSetMock;
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly ProductsController _controller;

    public ProductServiceTests()
    {
        
        _productDbSetMock = new Mock<DbSet<Product>>();

        
        _dbContextMock = new Mock<ApplicationDbContext>();
        _dbContextMock.Setup(db => db.Products).Returns(_productDbSetMock.Object);

       
        _controller = new ProductsController(_dbContextMock.Object);
    }

    [Fact]
    public void GetProducts_ReturnsOkResult()
    {
        // Arrange: Setup data for the test
        var mockProducts = new List<Product>
        {
            new Product { Id = 1, Name = "Product1", Description = "Test product", Price = 100, CategoryId = 1 },
            new Product { Id = 2, Name = "Product2", Description = "Test product", Price = 200, CategoryId = 1 }
        }.AsQueryable();

        // Setup mock DbSet to return the list of products
        var mockProductDbSet = new Mock<DbSet<Product>>();
        mockProductDbSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(mockProducts.Provider);
        mockProductDbSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(mockProducts.Expression);
        mockProductDbSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(mockProducts.ElementType);
        mockProductDbSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(mockProducts.GetEnumerator());

        // Replace the original DbSet mock with this data
        _dbContextMock.Setup(db => db.Products).Returns(mockProductDbSet.Object);

        // Act: Call the GetProducts method
        var result = _controller.GetProducts();

        // Assert: Check if the result is OkObjectResult and contains expected data
        var okResult = Assert.IsType<OkObjectResult>(result);
        var products = Assert.IsType<List<Product>>(okResult.Value);
        Assert.Equal(2, products.Count);
    }

    [Fact]
    public void GetProducts_WithPagination_ReturnsPaginatedResult()
    {
        // Arrange: Setup data for pagination test
        var mockProducts = new List<Product>
        {
            new Product { Id = 1, Name = "Product1", Description = "Test product", Price = 100, CategoryId = 1 },
            new Product { Id = 2, Name = "Product2", Description = "Test product", Price = 200, CategoryId = 1 },
            new Product { Id = 3, Name = "Product3", Description = "Test product", Price = 300, CategoryId = 1 }
        }.AsQueryable();

        // Setup mock DbSet with IQueryable data
        var mockProductDbSet = new Mock<DbSet<Product>>();
        mockProductDbSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(mockProducts.Provider);
        mockProductDbSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(mockProducts.Expression);
        mockProductDbSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(mockProducts.ElementType);
        mockProductDbSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(mockProducts.GetEnumerator());

        _dbContextMock.Setup(db => db.Products).Returns(mockProductDbSet.Object);

        // Act: Call the GetProducts method with pagination
        var result = _controller.GetProducts(pageNumber: 1, pageSize: 2);

        // Assert: Check if the result is paginated correctly
        var okResult = Assert.IsType<OkObjectResult>(result);
        dynamic paginatedResult = okResult.Value;
        Assert.Equal(2, paginatedResult.PageSize);
        Assert.Equal(3, paginatedResult.TotalItems);
        Assert.Equal(2, ((IEnumerable<Product>)paginatedResult.Products).Count());
    }

    [Fact]
    public void GetProducts_WithFilter_ReturnsFilteredResult()
    {
        // Arrange: Setup data for filtering test
        var mockProducts = new List<Product>
        {
            new Product { Id = 1, Name = "Product1", Description = "Test product", Price = 100, CategoryId = 1 },
            new Product { Id = 2, Name = "Phone", Description = "Test product", Price = 200, CategoryId = 1 }
        }.AsQueryable();

        // Setup mock DbSet
        var mockProductDbSet = new Mock<DbSet<Product>>();
        mockProductDbSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(mockProducts.Provider);
        mockProductDbSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(mockProducts.Expression);
        mockProductDbSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(mockProducts.ElementType);
        mockProductDbSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(mockProducts.GetEnumerator());

        _dbContextMock.Setup(db => db.Products).Returns(mockProductDbSet.Object);

        // Act: Call the GetProducts method with filtering
        var result = _controller.GetProducts(filter: "Phone");

        // Assert: Check if the result is filtered correctly
        var okResult = Assert.IsType<OkObjectResult>(result);
        var products = Assert.IsType<List<Product>>(okResult.Value);
        Assert.Single(products);
        Assert.Equal("Phone", products.First().Name);
    }

    [Fact]
    public void AddProduct_ReturnsCreatedAtAction()
    {
        // Arrange: Create a product to add
        var newProduct = new Product { Id = 1, Name = "New Product", Description = "Test product", Price = 100, CategoryId = 1 };

        // Act: Call the AddProduct method
        var result = _controller.AddProduct(newProduct);

        // Assert: Check if the result is CreatedAtActionResult
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetProducts), createdResult.ActionName);
    }

    [Fact]
    public void DeleteProduct_ReturnsNoContent_WhenProductExists()
    {
        // Arrange: Setup a product to delete
        var product = new Product { Id = 1, Name = "Product1", Description = "Test product", Price = 100, CategoryId = 1 };

        // Setup DbContext mock to find the product by id
        _dbContextMock.Setup(db => db.Products.Find(1)).Returns(product);

        // Act: Call the DeleteProduct method
        var result = _controller.DeleteProduct(1);

        // Assert: Check if the result is NoContent
        Assert.IsType<NoContentResult>(result);

        // Verify if the product was removed from DbSet
        _dbContextMock.Verify(db => db.Products.Remove(product), Times.Once);
        _dbContextMock.Verify(db => db.SaveChanges(), Times.Once);
    }

    [Fact]
    public void DeleteProduct_ReturnsNotFound_WhenProductDoesNotExist()
    {
        // Arrange: Setup DbContext to return null when product is not found
        _dbContextMock.Setup(db => db.Products.Find(1)).Returns((Product)null);

        // Act: Call the DeleteProduct method
        var result = _controller.DeleteProduct(1);

        // Assert: Check if the result is NotFound
        Assert.IsType<NotFoundResult>(result);
    }
}
