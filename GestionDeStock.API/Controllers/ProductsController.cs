using GestionDeStock.API.Models;
using GestionDeStock.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Data;
using GestionDeStock.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace GestionDeStock.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IImageService _imageService;

        public ProductsController(AppDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        // GET api/products — avec URL image absolue pour le site PHP
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();

            var result = products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Desc,
                p.CategoryId,
                p.Category,
                p.Quantity,
                p.Price,
                p.Threshold,
                p.Sku,
                p.Location,
                p.ImagePath,
                ImageUrl = _imageService.BuildPublicUrl(Request, p.ImagePath), //  URL absolue
                p.CreatedAt,
                p.UpdatedAt
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetProduct(int id)
        {
            var p = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (p == null) return NotFound();

            return Ok(new
            {
                p.Id,
                p.Name,
                p.Desc,
                p.CategoryId,
                p.Category,
                p.Quantity,
                p.Price,
                p.Threshold,
                p.Sku,
                p.Location,
                p.ImagePath,
                ImageUrl = _imageService.BuildPublicUrl(Request, p.ImagePath),
                p.CreatedAt,
                p.UpdatedAt
            });
        }

        // POST — multipart/form-data obligatoire pour l'image
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var category = await _context.Categories.FindAsync(dto.CategoryId);
                if (category == null) return BadRequest("Catégorie introuvable.");

                var imagePath = dto.Image != null
                    ? await _imageService.SaveImageAsync(dto.Image)
                    : null;

                var product = new Product
                {
                    Id = 0,
                    Name = dto.Name,
                    Desc = dto.Desc,
                    CategoryId = dto.CategoryId,
                    Category = category,
                    Quantity = dto.Quantity,
                    Price = dto.Price,
                    Threshold = dto.Threshold,
                    Sku = dto.Sku ?? string.Empty,
                    Location = dto.Location ?? string.Empty,
                    ImagePath = imagePath,
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new
                {
                    product.Id,
                    product.Name,
                    product.ImagePath,
                    ImageUrl = _imageService.BuildPublicUrl(Request, product.ImagePath)
                });
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
            }
        }


        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            try
            {
                if (dto.Image != null)
                {
                    _imageService.DeleteImage(product.ImagePath);
                    product.ImagePath = await _imageService.SaveImageAsync(dto.Image);
                }

                product.Name = dto.Name;
                product.Desc = dto.Desc;
                product.CategoryId = dto.CategoryId;
                product.Quantity = dto.Quantity;
                product.Price = dto.Price;
                product.Threshold = dto.Threshold;
                product.Sku = dto.Sku ?? string.Empty;           // ✅ jamais null
                product.Location = dto.Location ?? string.Empty; // ✅ jamais null
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    product.Id,
                    product.Name,
                    product.ImagePath,
                    ImageUrl = _imageService.BuildPublicUrl(Request, product.ImagePath)
                });
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Suppliers)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            // Supprimer toutes les relations
            var movements = await _context.Movements.Where(m => m.ProductId == id).ToListAsync();
            _context.Movements.RemoveRange(movements);

            var stockMovements = await _context.StockMovements.Where(m => m.ProductId == id).ToListAsync();
            _context.StockMovements.RemoveRange(stockMovements);

            var orders = await _context.Orders.Where(o => o.ProductId == id).ToListAsync();
            _context.Orders.RemoveRange(orders);

            var provides = await _context.Provides.Where(p => p.ProductId == id).ToListAsync();
            _context.Provides.RemoveRange(provides);

            var inventoryLines = await _context.InventoryLines.Where(il => il.ProductId == id).ToListAsync();
            _context.InventoryLines.RemoveRange(inventoryLines); // ✅ le vrai coupable

            var reviews = await _context.Reviews.Where(r => r.ProductId == id).ToListAsync();
            _context.Reviews.RemoveRange(reviews);

            product.Suppliers.Clear();

            await _context.SaveChangesAsync();

            _imageService.DeleteImage(product.ImagePath);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}