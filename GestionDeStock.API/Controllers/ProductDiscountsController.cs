using GestionDeStock.API.Auth;
using GestionDeStock.API.Data;
using GestionDeStock.API.Dtos;
using GestionDeStock.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionDeStock.API.Controllers
{
    [ApiController]
    [Route("api/discounts")]
    public class ProductDiscountsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductDiscountsController(AppDbContext context)
        {
            _context = context;
        }

        // ── GET /api/discounts/active ───────────────────────────────────
        // Public — toutes les remises actuellement actives avec infos produit
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive()
        {
            var now = DateTime.UtcNow;
            var discounts = await _context.ProductDiscounts
                .Include(d => d.Product)
                    .ThenInclude(p => p!.Images)
                .Include(d => d.Event)
                .Where(d => d.IsActive && d.StartDate <= now && d.EndDate >= now
                         && (d.Event == null || (d.Event.IsActive && d.Event.StartDate <= now && d.Event.EndDate >= now))
                         && d.Product != null && d.Product.IsActive)
                .OrderByDescending(d => d.DiscountValue)
                .ToListAsync();

            return Ok(discounts.Select(d => FormatDiscount(d)));
        }

        // ── GET /api/discounts ──────────────────────────────────────────
        // Admin/Gérant — toutes les remises avec filtres
        [HttpGet]
        [Authorize(Roles = Roles.AdminOrGerant)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? productId = null,
            [FromQuery] int? eventId = null,
            [FromQuery] bool? activeOnly = null)
        {
            var now = DateTime.UtcNow;
            var query = _context.ProductDiscounts
                .Include(d => d.Product)
                .Include(d => d.Event)
                .AsQueryable();

            if (productId.HasValue)
                query = query.Where(d => d.ProductId == productId.Value);

            if (eventId.HasValue)
                query = query.Where(d => d.EventId == eventId.Value);

            if (activeOnly == true)
                query = query.Where(d => d.IsActive && d.StartDate <= now && d.EndDate >= now);

            var discounts = await query
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return Ok(discounts.Select(d => FormatDiscount(d)));
        }

        // ── GET /api/discounts/{id} ─────────────────────────────────────
        [HttpGet("{id:int}")]
        [Authorize(Roles = Roles.AdminOrGerant)]
        public async Task<IActionResult> GetById(int id)
        {
            var d = await _context.ProductDiscounts
                .Include(d => d.Product)
                .Include(d => d.Event)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (d == null) return NotFound(new { erreur = "Remise introuvable" });
            return Ok(FormatDiscount(d));
        }

        // ── POST /api/discounts ─────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = Roles.AdminOrGerant)]
        public async Task<IActionResult> Create([FromBody] ProductDiscountDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.EndDate <= dto.StartDate)
                return BadRequest(new { erreur = "La date de fin doit être après la date de début" });

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                return BadRequest(new { erreur = "Produit introuvable" });

            if (dto.EventId.HasValue)
            {
                var ev = await _context.PromotionEvents.FindAsync(dto.EventId.Value);
                if (ev == null) return BadRequest(new { erreur = "Événement introuvable" });
            }

            if (!Enum.TryParse<DiscountType>(dto.DiscountType, out var discountType))
                return BadRequest(new { erreur = "Type de remise invalide. Utilisez 'Percentage' ou 'Fixed'" });

            if (discountType == DiscountType.Percentage && dto.DiscountValue > 100)
                return BadRequest(new { erreur = "Un pourcentage ne peut pas dépasser 100" });

            if (discountType == DiscountType.Fixed && dto.DiscountValue > product.Price)
                return BadRequest(new { erreur = "La remise fixe ne peut pas dépasser le prix du produit" });

            var discount = new ProductDiscount
            {
                ProductId = dto.ProductId,
                EventId = dto.EventId,
                DiscountType = discountType,
                DiscountValue = dto.DiscountValue,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive,
            };

            _context.ProductDiscounts.Add(discount);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = discount.Id }, new { discount.Id });
        }

        // ── PUT /api/discounts/{id} ─────────────────────────────────────
        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.AdminOrGerant)]
        public async Task<IActionResult> Update(int id, [FromBody] ProductDiscountDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.EndDate <= dto.StartDate)
                return BadRequest(new { erreur = "La date de fin doit être après la date de début" });

            var discount = await _context.ProductDiscounts.FindAsync(id);
            if (discount == null) return NotFound(new { erreur = "Remise introuvable" });

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null) return BadRequest(new { erreur = "Produit introuvable" });

            if (!Enum.TryParse<DiscountType>(dto.DiscountType, out var discountType))
                return BadRequest(new { erreur = "Type de remise invalide. Utilisez 'Percentage' ou 'Fixed'" });

            if (discountType == DiscountType.Percentage && dto.DiscountValue > 100)
                return BadRequest(new { erreur = "Un pourcentage ne peut pas dépasser 100" });

            discount.ProductId = dto.ProductId;
            discount.EventId = dto.EventId;
            discount.DiscountType = discountType;
            discount.DiscountValue = dto.DiscountValue;
            discount.StartDate = dto.StartDate;
            discount.EndDate = dto.EndDate;
            discount.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return Ok(new { discount.Id, discount.IsActive });
        }

        // ── DELETE /api/discounts/{id} ──────────────────────────────────
        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.AdminOrGerant)]
        public async Task<IActionResult> Delete(int id)
        {
            var discount = await _context.ProductDiscounts.FindAsync(id);
            if (discount == null) return NotFound(new { erreur = "Remise introuvable" });

            _context.ProductDiscounts.Remove(discount);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ── Helper ──────────────────────────────────────────────────────
        private static object FormatDiscount(ProductDiscount d)
        {
            var now = DateTime.UtcNow;
            return new
            {
                d.Id,
                d.ProductId,
                nomProduit = d.Product?.Name,
                d.EventId,
                nomEvenement = d.Event?.Name,
                badgeEvenement = d.Event?.BadgeLabel,
                bgColor = d.Event?.BgColor,
                discountType = d.DiscountType.ToString(),
                d.DiscountValue,
                d.StartDate,
                d.EndDate,
                d.IsActive,
                enCours = d.IsActive && d.StartDate <= now && d.EndDate >= now,
                d.CreatedAt,
            };
        }
    }
}
