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

                Console.WriteLine("*********Amount calculated: " + amount);

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

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrder([FromBody] OrderDto orderDto, int id)
        {
            if (id != orderDto.Id)
                return BadRequest("L'ID de la commande ne correspond pas");

            var existingOrder = await _context.Orders.FindAsync(id);
            if (existingOrder == null)
                return NotFound();

            // Mise à jour des propriétés
            existingOrder.Quantity = orderDto.Quantity;
            existingOrder.CustomerId = orderDto.CustomerId;
            existingOrder.ProductId = orderDto.ProductId;
            existingOrder.Status = orderDto.Status;

            // Si la commande est livrée, ajuster le stock et enregistrer le mouvement
            if (existingOrder.Status == OrderStatus.LIVREE)
            {
                var product = await _context.Products.FindAsync(existingOrder.ProductId);
                if (product != null)
                {
                    if (product.Quantity < existingOrder.Quantity)
                        return BadRequest("Stock insuffisant pour livrer cette commande.");
                    product.Quantity -= existingOrder.Quantity;
                    existingOrder.Amount = orderDto.Quantity * product.Price;
                }

                await _context.Movements.AddAsync(new Movement
                {
                    Quantity = existingOrder.Quantity,
                    Amount = existingOrder.Amount,
                    ProductId = existingOrder.ProductId,
                    CustomerId = existingOrder.CustomerId,
                    Type = "Sortie",
                });
            }

            // Sauvegarde finale
            await _context.SaveChangesAsync();

            return Ok(existingOrder);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}