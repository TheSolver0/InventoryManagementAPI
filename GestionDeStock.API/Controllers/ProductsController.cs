using GestionDeStock.API.Models;
using GestionDeStock.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Data;

namespace GestionDeStock.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();

            return Ok(products);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            return Ok(product);
        }
        [HttpPost]
        public IActionResult CreateProduct([FromBody] ProductDto product)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var category = _context.Categories.Find(product.CategoryId);
                if (category == null)
                    return BadRequest("Catégorie introuvable");

                var newProduct = new Product
                {
                    Name = product.Name,
                    Desc = product.Desc,
                    CategoryId = product.CategoryId,
                    Quantity = product.Quantity,
                    Price = product.Price,
                    Threshold = product.Threshold,
                    Category = category

                };

                _context.Products.Add(newProduct);
                _context.SaveChanges();

                return Ok(newProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Erreur lors de l'enregistrement",
                    details = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Product product, int id)
        {
            if (id != product.Id)
                return BadRequest("L'ID du produit ne correspond pas");
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
                return NotFound();

            // Mise à jour des propriétés individuellement
            existingProduct.Name = product.Name;
            existingProduct.Desc = product.Desc;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.Quantity = product.Quantity;
            existingProduct.Price = product.Price;
            existingProduct.Threshold = product.Threshold;

            await _context.SaveChangesAsync();

            return Ok(existingProduct);

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}