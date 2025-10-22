using GestionDeStock.API.Models;
using GestionDeStock.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Data;

namespace GestionDeStock.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProvidesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ProvidesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProvideDto>>> GetProvides()
        {
            var provides = await _context.Provides
                .Select(o => new ProvideDto
                {
                    Id = o.Id,
                    Quantity = o.Quantity,
                    Amount = o.Amount,
                    SupplierId = o.SupplierId,
                    Supplier = o.Supplier,
                    ProductId = o.ProductId,
                    Product = o.Product,
                    Status = o.Status   
                })
                .ToListAsync();

            return Ok(provides);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Provide>> GetProvide(int id)
        {
            var provide = await _context.Provides.FindAsync(id);

            return Ok(provide);
        }
        [HttpPost]
        public IActionResult CreateProvide([FromBody] ProvideDto provideDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var supplier = _context.Suppliers.Find(provideDto.SupplierId);
                if (supplier == null)
                    return BadRequest("Fournisseur introuvable");
                var product = _context.Products.Find(provideDto.ProductId);
                if (product == null)
                    return BadRequest("Produit introuvable");
                var amount = product.Price * provideDto.Quantity;

                var newProvide = new Provide
                {
                    Quantity = provideDto.Quantity,
                    Amount = amount,
                    SupplierId = provideDto.SupplierId,
                    ProductId = provideDto.ProductId,
                    Product = product,
                    Supplier = supplier
                };

                _context.Provides.Add(newProvide);
                _context.SaveChanges();

                return Ok(newProvide);
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

        [HttpPut]
        public async Task<IActionResult> UpdateProduct(Product product, int id)
        {
            if (id != product.Id)
                return BadRequest("L'ID du produit ne correspond pas");
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
                return NotFound();

            // Mise à jour des propriétés

            existingProduct = product;

            await _context.SaveChangesAsync();

            return NoContent();

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