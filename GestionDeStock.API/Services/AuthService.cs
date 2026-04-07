using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using GestionDeStock.API.Data;
using GestionDeStock.API.Dtos;
using GestionDeStock.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GestionDeStock.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(registerDto.Email) || 
                    string.IsNullOrWhiteSpace(registerDto.Username) || 
                    string.IsNullOrWhiteSpace(registerDto.Password))
                {
                    return new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "Email, username et mot de passe sont requis." 
                    };
                }

                if (registerDto.Password != registerDto.ConfirmPassword)
                {
                    return new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "Les mots de passe ne correspondent pas." 
                    };
                }

                if (registerDto.Password.Length < 6)
                {
                    return new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "Le mot de passe doit contenir au moins 6 caractères." 
                    };
                }

                // Vérifier si l'email existe déjà
                if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                {
                    return new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "Cet email est déjà utilisé." 
                    };
                }

                // Vérifier si le username existe déjà
                if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
                {
                    return new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "Ce nom d'utilisateur est déjà utilisé." 
                    };
                }

                // Créer le nouvel utilisateur
                var user = new User
                {
                    Email = registerDto.Email,
                    Username = registerDto.Username,
                    PasswordHash = HashPassword(registerDto.Password),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Utilisateur enregistré: {user.Email}");

                var token = GenerateJwtToken(user);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Enregistrement réussi.",
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Username = user.Username,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de l'enregistrement: {ex.Message}");
                return new AuthResponseDto 
                { 
                    Success = false, 
                    Message = ex.Message 
                };
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
                {
                    return new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "Email et mot de passe sont requis." 
                    };
                }

                // Chercher l'utilisateur
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "Email ou mot de passe incorrect." 
                    };
                }

                if (!user.IsActive)
                {
                    return new AuthResponseDto 
                    { 
                        Success = false, 
                        Message = "Cet utilisateur est inactif." 
                    };
                }

                _logger.LogInformation($"Utilisateur connecté: {user.Email}");

                var token = GenerateJwtToken(user);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Connexion réussie.",
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Username = user.Username,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la connexion: {ex.Message}");
                return new AuthResponseDto 
                { 
                    Success = false, 
                    Message = "Une erreur s'est produite lors de la connexion." 
                };
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: new[]
                {
                    new System.Security.Claims.Claim("id", user.Id.ToString()),
                    new System.Security.Claims.Claim("email", user.Email),
                    new System.Security.Claims.Claim("username", user.Username)
                },
                expires: DateTime.UtcNow.AddHours(int.Parse(_configuration["JwtSettings:ExpirationHours"] ?? "24")),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hash);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }
    }
}
