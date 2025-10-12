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
            var suppliers = await _context.Suppliers.ToListAsync();

            return Ok(suppliers);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Supplier>> GetSupplier(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);

            return Ok(supplier);
        }
        [HttpPost]
        public async Task<IActionResult> CreateSupplier([FromBody] SupplierDto supplier)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var newSupplier = new Supplier
                {
                    Name = supplier.Name,
                    Email = supplier.Email,
                    Address = supplier.Address,
                    Telephone = supplier.Telephone,
                    Delay = supplier.Delay,
                    Products = supplier.Products

                };
                _context.Suppliers.Add(newSupplier);
                await _context.SaveChangesAsync();

                return Ok(newSupplier);
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
        [HttpPut]
        public async Task<IActionResult> UpdateSupplier(Supplier supplier, int id)
        {
            if (id != supplier.Id)
                return BadRequest("L'ID du fournisseur ne correspond pas");
            var existingSupplier = await _context.Suppliers.FindAsync(id);
            if (existingSupplier == null)
                return NotFound();

            // Mise à jour des propriétés

            existingSupplier = supplier;

            await _context.SaveChangesAsync();

            return NoContent();

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