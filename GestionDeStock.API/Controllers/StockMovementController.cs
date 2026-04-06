using Microsoft.AspNetCore.Mvc;
using GestionDeStock.API.Services;
using GestionDeStock.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace GestionDeStock.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StockMovementsController : ControllerBase
    {
        private readonly IStockMovementService _stockMovementService;

        public StockMovementsController(IStockMovementService stockMovementService)
        {
            _stockMovementService = stockMovementService;
        }

        /// <summary>
        /// Récupère tous les mouvements de stock
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var movements = await _stockMovementService.GetAllMovementsAsync();
            return Ok(movements);
        }

        /// <summary>
        /// Récupère un mouvement par son ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var movement = await _stockMovementService.GetMovementByIdAsync(id);
            
            if (movement == null)
                return NotFound(new { message = "Mouvement non trouvé" });

            return Ok(movement);
        }

        /// <summary>
        /// Récupère tous les mouvements d'un produit spécifique
        /// </summary>
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProductId(int productId)
        {
            var movements = await _stockMovementService.GetMovementsByProductIdAsync(productId);
            return Ok(movements);
        }

        /// <summary>
        /// Crée un nouveau mouvement de stock manuellement
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StockMovement movement)
        {
            try
            {
                var created = await _stockMovementService.CreateMovementAsync(movement);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
    }
}