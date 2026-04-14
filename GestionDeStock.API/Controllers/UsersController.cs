using GestionDeStock.API.Auth;
using GestionDeStock.API.Data;
using GestionDeStock.API.Dtos;
using GestionDeStock.API.Models;
using GestionDeStock.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace GestionDeStock.API.Controllers
{
    /// <summary>
    /// Gestion des utilisateurs — réservé aux Admins.
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Username = u.Username,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Utilisateur non trouvé." });

            return Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        // PUT api/users/{id}  — modifier profil, rôle, statut
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "Utilisateur non trouvé." });

            // Empêcher un admin de se retirer son propre rôle
            var requesterId = int.Parse(User.FindFirst("id")?.Value ?? "0");
            if (requesterId == id && dto.Role.HasValue && dto.Role != UserRole.Admin)
                return BadRequest(new { message = "Un admin ne peut pas rétrograder son propre compte." });

            user.Username = dto.Username;
            user.Email = dto.Email;
            if (dto.Role.HasValue) user.Role = dto.Role.Value;
            if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        // PATCH api/users/{id}/password  — réinitialiser le mot de passe
        [HttpPatch("{id}/password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
                return BadRequest(new { message = "Le mot de passe doit contenir au moins 6 caractères." });

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "Utilisateur non trouvé." });

            user.PasswordHash = HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Mot de passe réinitialisé." });
        }

        // DELETE api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var requesterId = int.Parse(User.FindFirst("id")?.Value ?? "0");
            if (requesterId == id)
                return BadRequest(new { message = "Impossible de supprimer votre propre compte." });

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Utilisateur non trouvé." });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static string HashPassword(string password)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hash);
        }
    }

    public class ResetPasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
