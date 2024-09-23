using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIProduct.Data;
using WebAPIProduct.Models;

namespace WebAPIProduct.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetProducts([FromQuery] string sortBy = "Name", [FromQuery] string filter = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                products = products.Where(p => p.Name.Contains(filter));
            }

            if (sortBy == "Price")
            {
                products = products.OrderBy(p => p.Price);
            }
            else
            {
                products = products.OrderBy(p => p.Name);
            }

            var pagedProducts = products.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return Ok(pagedProducts);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "ApiKey")]  
        public IActionResult AddProduct([FromBody] Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "ApiKey")]  
        public IActionResult UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.CategoryId = updatedProduct.CategoryId;

            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "ApiKey")]  
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
