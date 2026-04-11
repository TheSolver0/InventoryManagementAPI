using GestionDeStock.API.Models;
using GestionDeStock.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Dtos;

namespace GestionDeStock.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GerantsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GerantsController> _logger;

        public GerantsController(AppDbContext context, ILogger<GerantsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Récupérer tous les utilisateurs
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        /// <summary>
        /// Récupérer un utilisateur par ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Utilisateur non trouvé." });

            return Ok(user);
        }
        
        [HttpPut("{id}")]
public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
{
    var user = await _context.Users.FindAsync(id);
    if (user == null) return NotFound();

    user.Username = dto.Username;
    user.Email = dto.Email;
    user.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();
    return Ok(user);
}

        /// <summary>
        /// Supprimer un utilisateur
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Utilisateur non trouvé." });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}