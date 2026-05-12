using GestionDeStock.API.Auth;
using GestionDeStock.API.Data;
using GestionDeStock.API.Dtos;
using GestionDeStock.API.Models;
using GestionDeStock.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionDeStock.API.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class PromotionEventsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IImageService _imageService;

        public PromotionEventsController(AppDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        // ── GET /api/events ─────────────────────────────────────────────
        // Public — liste les événements actifs (non expirés)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] bool includeExpired = false)
        {
            var now = DateTime.UtcNow;
            var query = _context.PromotionEvents.AsQueryable();

            if (!includeExpired)
                query = query.Where(e => e.IsActive && e.EndDate >= now);

            var events = await query
                .OrderByDescending(e => e.StartDate)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    bannerImage = _imageService.BuildPublicUrl(Request, e.BannerImage),
                    e.BadgeLabel,
                    e.BgColor,
                    e.TextColor,
                    e.StartDate,
                    e.EndDate,
                    e.IsActive,
                    enCours = e.StartDate <= now && e.EndDate >= now,
                    nbProduits = e.Discounts.Count(d => d.IsActive),
                    e.CreatedAt,
                })
                .ToListAsync();

            return Ok(events);
        }

        // ── GET /api/events/{id} ────────────────────────────────────────
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var now = DateTime.UtcNow;
            var ev = await _context.PromotionEvents
                .Include(e => e.Discounts)
                    .ThenInclude(d => d.Product)
                        .ThenInclude(p => p!.Images)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return NotFound(new { erreur = "Événement introuvable" });

            return Ok(new
            {
                ev.Id,
                ev.Name,
                ev.Description,
                bannerImage = _imageService.BuildPublicUrl(Request, ev.BannerImage),
                ev.BadgeLabel,
                ev.BgColor,
                ev.TextColor,
                ev.StartDate,
                ev.EndDate,
                ev.IsActive,
                enCours = ev.StartDate <= now && ev.EndDate >= now,
                ev.CreatedAt,
                remises = ev.Discounts
                    .Where(d => d.IsActive)
                    .Select(d => new
                    {
                        d.Id,
                        d.ProductId,
                        nomProduit = d.Product?.Name,
                        d.DiscountType,
                        d.DiscountValue,
                        d.StartDate,
                        d.EndDate,
                    }),
            });
        }

        // ── GET /api/events/{id}/products ───────────────────────────────
        // Produits de l'événement avec prix remisés calculés
        [HttpGet("{id:int}/products")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProducts(int id)
        {
            var now = DateTime.UtcNow;
            var ev = await _context.PromotionEvents.FindAsync(id);
            if (ev == null) return NotFound(new { erreur = "Événement introuvable" });

            var discounts = await _context.ProductDiscounts
                .Include(d => d.Product)
                    .ThenInclude(p => p!.Category)
                .Include(d => d.Product)
                    .ThenInclude(p => p!.Images)
                .Where(d => d.EventId == id && d.IsActive && d.StartDate <= now && d.EndDate >= now
                         && d.Product != null && d.Product.IsActive)
                .ToListAsync();

            // Regrouper par produit et appliquer les remises cumulatives
            var grouped = discounts
                .GroupBy(d => d.ProductId)
                .Select(g =>
                {
                    var product = g.First().Product!;
                    var prixRemise = ApplyDiscounts(product.Price, g.ToList());
                    var mainImg = product.Images.FirstOrDefault(i => i.IsMain)
                               ?? product.Images.OrderBy(i => i.Order).FirstOrDefault();

                    return new
                    {
                        id = product.Id,
                        nom = product.Name,
                        prix = product.Price,
                        prix_remise = prixRemise,
                        pourcentage_remise = product.Price > 0
                            ? Math.Round((1 - prixRemise / product.Price) * 100, 1)
                            : 0,
                        stock = product.Quantity,
                        marque = product.Brand ?? "",
                        badge = product.Badge ?? "",
                        note = product.Rating,
                        nb_avis = product.ReviewCount,
                        categorie_nom = product.Category?.Title ?? "",
                        image = _imageService.BuildPublicUrl(Request,
                            mainImg?.ImagePath ?? product.ImagePath),
                        remises = g.Select(d => new
                        {
                            d.Id,
                            d.DiscountType,
                            d.DiscountValue,
                        }),
                    };
                })
                .ToList();

            return Ok(new
            {
                evenement = new { ev.Id, ev.Name, ev.BadgeLabel, ev.BgColor, ev.TextColor },
                produits = grouped,
                total = grouped.Count,
            });
        }

        // ── POST /api/events ────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = Roles.AdminOrGerant)]
        public async Task<IActionResult> Create([FromBody] PromotionEventDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.EndDate <= dto.StartDate)
                return BadRequest(new { erreur = "La date de fin doit être après la date de début" });

            var ev = new PromotionEvent
            {
                Name = dto.Name,
                Description = dto.Description,
                BadgeLabel = dto.BadgeLabel,
                BgColor = dto.BgColor,
                TextColor = dto.TextColor,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive,
            };

            _context.PromotionEvents.Add(ev);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = ev.Id }, new { ev.Id, ev.Name });
        }

        // ── PUT /api/events/{id} ────────────────────────────────────────
        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.AdminOrGerant)]
        public async Task<IActionResult> Update(int id, [FromBody] PromotionEventDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.EndDate <= dto.StartDate)
                return BadRequest(new { erreur = "La date de fin doit être après la date de début" });

            var ev = await _context.PromotionEvents.FindAsync(id);
            if (ev == null) return NotFound(new { erreur = "Événement introuvable" });

            ev.Name = dto.Name;
            ev.Description = dto.Description;
            ev.BadgeLabel = dto.BadgeLabel;
            ev.BgColor = dto.BgColor;
            ev.TextColor = dto.TextColor;
            ev.StartDate = dto.StartDate;
            ev.EndDate = dto.EndDate;
            ev.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return Ok(new { ev.Id, ev.Name, ev.IsActive });
        }

        // ── POST /api/events/{id}/image ─────────────────────────────────
        [HttpPost("{id:int}/image")]
        [Authorize(Roles = Roles.AdminOrGerant)]
        public async Task<IActionResult> UploadBanner(int id, IFormFile file)
        {
            var ev = await _context.PromotionEvents.FindAsync(id);
            if (ev == null) return NotFound(new { erreur = "Événement introuvable" });

            if (ev.BannerImage != null)
                _imageService.DeleteImage(ev.BannerImage);

            ev.BannerImage = await _imageService.SaveImageAsync(file);
            await _context.SaveChangesAsync();

            return Ok(new { bannerImage = _imageService.BuildPublicUrl(Request, ev.BannerImage) });
        }

        // ── DELETE /api/events/{id} ─────────────────────────────────────
        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.AdminOrGerant)]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.PromotionEvents.FindAsync(id);
            if (ev == null) return NotFound(new { erreur = "Événement introuvable" });

            if (ev.BannerImage != null)
                _imageService.DeleteImage(ev.BannerImage);

            _context.PromotionEvents.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ── Helper ──────────────────────────────────────────────────────
        internal static decimal ApplyDiscounts(decimal price, IEnumerable<ProductDiscount> discounts)
        {
            // Applique d'abord les pourcentages, puis les montants fixes
            foreach (var d in discounts.Where(d => d.DiscountType == DiscountType.Percentage))
                price -= price * (d.DiscountValue / 100m);

            foreach (var d in discounts.Where(d => d.DiscountType == DiscountType.Fixed))
                price -= d.DiscountValue;

            return Math.Max(0, Math.Round(price, 2));
        }
    }
}
