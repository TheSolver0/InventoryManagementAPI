using GestionDeStock.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Data;
using GestionDeStock.API.Dtos;

namespace GestionDeStock.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            var customers = await _context.Customers.ToListAsync();

            return Ok(customers);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            return Ok(customer);
        }
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerDto customer)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var newCustomer = new Customer
            {
                Name = customer.Name,
                Email = customer.Email,
                Address = customer.Address,
                Telephone = customer.Telephone,
                Points = 0
            };
            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();

            return Ok(newCustomer);
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
        public async Task<IActionResult> UpdateCustomer(Customer customer, int id)
        {
            if (id != customer.Id)
                return BadRequest("L'ID du client ne correspond pas");
            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer == null)
                return NotFound();

            // Mise à jour des propriétés

            existingCustomer.Name = customer.Name;
            existingCustomer.Email = customer.Email;
            existingCustomer.Address = customer.Address;
            existingCustomer.Telephone = customer.Telephone;
            existingCustomer.Points = customer.Points;

            await _context.SaveChangesAsync();

            return Ok(existingCustomer);

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound();

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}