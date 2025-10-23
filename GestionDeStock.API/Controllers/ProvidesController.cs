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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProvide([FromBody] ProvideDto provideDto, int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingProvide = await _context.Provides.FindAsync(id);
            if (existingProvide == null)
                return NotFound();

            try
            {
                /*var supplier = _context.Suppliers.Find(provideDto.SupplierId);
                if (supplier == null)
                    return BadRequest("Fournisseur introuvable");
                var product = _context.Products.Find(provideDto.ProductId);
                if (product == null)
                    return BadRequest("Produit introuvable");
                var amount = product.Price * provideDto.Quantity;*/


                existingProvide.Quantity = provideDto.Quantity;
                // existingProvide.Amount = amount;
                existingProvide.SupplierId = provideDto.SupplierId;
                existingProvide.ProductId = provideDto.ProductId;
                // existingProvide.Product = product;
                // existingProvide.Supplier = supplier;
                existingProvide.Status = provideDto.Status;
                // Si la commande est livrée, ajuster le stock et enregistrer le mouvement
                if (existingProvide.Status == ProvideStatus.LIVREE)
                {
                    var product = await _context.Products.FindAsync(existingProvide.ProductId);
                    if (product != null)
                    {
                        if (product.Quantity < existingProvide.Quantity)
                            return BadRequest("Stock insuffisant pour livrer cette commande.");
                        product.Quantity += existingProvide.Quantity;
                        existingProvide.Amount = provideDto.Quantity * product.Price;
                    }

                    await _context.Movements.AddAsync(new Movement
                    {
                        Quantity = existingProvide.Quantity,
                        Amount = existingProvide.Amount,
                        ProductId = existingProvide.ProductId,
                        SupplierId = existingProvide.SupplierId,
                        Type = "Entrée",
                    });
                }


                // _context.Provides.Add(newProvide);
                _context.SaveChanges();

                return Ok(existingProvide);
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProvide(int id)
        {
            var provide = await _context.Provides.FindAsync(id);
            if (provide == null)
                return NotFound();

            _context.Provides.Remove(provide);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}