using GestionDeStock.API.Models;
using GestionDeStock.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Data;

namespace GestionDeStock.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var orders = await _context.Orders
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    Quantity = o.Quantity,
                    Amount = o.Amount,
                    CustomerId = o.CustomerId,
                    Customer = o.Customer,
                    ProductId = o.ProductId,
                    Product = o.Product,
                    Status = o.Status   
                })
                .ToListAsync();

            return Ok(orders);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            return Ok(order);
        }
        [HttpPost]
        public IActionResult CreateOrder([FromBody] OrderDto orderDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var customer = _context.Customers.Find(orderDto.CustomerId);
                if (customer == null)
                    return BadRequest("Client introuvable");
                var product = _context.Products.Find(orderDto.ProductId);
                if (product == null)
                    return BadRequest("Produit introuvable");
                var amount = product.Price * orderDto.Quantity;

                var newOrder = new Order
                {
                    Quantity = orderDto.Quantity,
                    Amount = amount,
                    CustomerId = orderDto.CustomerId,
                    ProductId = orderDto.ProductId,
                    Product = product,
                    Customer = customer
                };

                _context.Orders.Add(newOrder);
                _context.SaveChanges();

                return Ok(newOrder);
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