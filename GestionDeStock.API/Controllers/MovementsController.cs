using GestionDeStock.API.Models;
using GestionDeStock.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Data;

namespace GestionDeStock.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovementsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public MovementsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Movement>>> GetMovements()
        {
            var movements = await _context.Movements.ToListAsync();
            return Ok(movements);
        }

    }
}