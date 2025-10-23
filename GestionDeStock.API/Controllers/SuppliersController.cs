using GestionDeStock.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Data;
using GestionDeStock.API.Dtos;

namespace GestionDeStock.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public SuppliersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Supplier>>> GetSuppliers()
        {
            var suppliers = await _context.Suppliers
                .Include(s => s.Products)
                .ToListAsync();

            return Ok(suppliers);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Supplier>> GetSupplier(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
                return NotFound();

            return Ok(supplier);
        }
        [HttpPost]
        public async Task<IActionResult> CreateSupplier([FromBody] SupplierDto supplier)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                // Charge les produits depuis la base
                var products = await _context.Products
                    .Where(p => supplier.Products.Contains(p.Id))
                    .ToListAsync();

                if (products.Count != supplier.Products.Count)
                {
                    return BadRequest("Un ou plusieurs produits n'ont pas été trouvés");
                }

                var newSupplier = new Supplier
                {
                    Name = supplier.Name,
                    Email = supplier.Email,
                    Address = supplier.Address,
                    Telephone = supplier.Telephone,
                    Delay = supplier.Delay,
                    Products = products
                };

                _context.Suppliers.Add(newSupplier);
                await _context.SaveChangesAsync();

                // Recharge le fournisseur avec ses produits pour le retourner
                var createdSupplier = await _context.Suppliers
                    .Include(s => s.Products)
                    .FirstOrDefaultAsync(s => s.Id == newSupplier.Id);

                return Ok(createdSupplier);
            }
            catch (Exception ex)
            {
                // Gérer l'exception (par exemple, journaliser l'erreur)
                return StatusCode(500, new
                {
                    error = "Erreur lors de l'enregistrement",
                    details = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }

        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier([FromBody] SupplierDto supplier, int id)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var existingSupplier = await _context.Suppliers
                    .Include(s => s.Products)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (existingSupplier == null)
                    return NotFound();

                // Charge les nouveaux produits
                var products = await _context.Products
                    .Where(p => supplier.Products.Contains(p.Id))
                    .ToListAsync();

                if (products.Count != supplier.Products.Count)
                {
                    return BadRequest("Un ou plusieurs produits n'ont pas été trouvés");
                }

                // Mise à jour des propriétés de base
                existingSupplier.Name = supplier.Name;
                existingSupplier.Address = supplier.Address;
                existingSupplier.Email = supplier.Email;
                existingSupplier.Telephone = supplier.Telephone;
                existingSupplier.Delay = supplier.Delay;

                // Vide la liste actuelle et ajoute les nouveaux produits
                existingSupplier.Products.Clear();
                foreach (var product in products)
                {
                    existingSupplier.Products.Add(product);
                }

                await _context.SaveChangesAsync();

                // Recharge le fournisseur pour avoir les données à jour
                var updatedSupplier = await _context.Suppliers
                    .Include(s => s.Products)
                    .FirstOrDefaultAsync(s => s.Id == id);

                return Ok(updatedSupplier);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Erreur lors de la mise à jour",
                    details = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound();

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}