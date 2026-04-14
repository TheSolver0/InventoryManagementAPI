using GestionDeStock.API.Data;
using GestionDeStock.API.Models;
using GestionDeStock.API.Services;
using GestionDeStock.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GestionDeStock.API.Controllers
{
    [ApiController]
    [Route("api/ecom")]
    public class EcomController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IImageService _imageService;

        // ── Mapping slug PHP → Title .NET ──────────────────────────────
        // Mettez ici vos vraies valeurs de Category.Title
        private static readonly Dictionary<string, string> SlugToTitle = new()
        {
            { "smartphones",  "Smartphones"   },
            { "tablettes",    "Tablettes"      },
            { "laptops",      "Laptops"        },
            { "accessoires",  "Accessoires"    },
            { "moniteurs",    "Moniteurs"      },
            // Ajoutez ici vos autres catégories...
        };

        // Icônes par défaut par titre (optionnel, pour la sidebar PHP)
        private static readonly Dictionary<string, string> TitleToIcone = new()
        {
            { "Smartphones",  "fas fa-mobile-alt"   },
            { "Tablettes",    "fas fa-tablet-alt"   },
            { "Laptops",      "fas fa-laptop"       },
            { "Accessoires",  "fas fa-headphones"   },
            { "Moniteurs",    "fas fa-desktop"      },
        };

        public EcomController(AppDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        // ── Helpers ────────────────────────────────────────────────────

        private string AbsoluteImage(string? path) =>
            _imageService.BuildPublicUrl(Request, path);

        // Génère un slug depuis un Title si absent du dictionnaire
        private static string TitleToSlug(string title) =>
            title.ToLower()
                 .Replace("é", "e").Replace("è", "e").Replace("ê", "e")
                 .Replace("à", "a").Replace("â", "a")
                 .Replace("ô", "o").Replace("ù", "u")
                 .Replace(" ", "-");

        private static string SlugFromTitle(string title)
        {
            // Cherche d'abord dans le dictionnaire inversé
            var found = SlugToTitle.FirstOrDefault(kv =>
                string.Equals(kv.Value, title, StringComparison.OrdinalIgnoreCase));
            return found.Key ?? TitleToSlug(title); // fallback auto-généré
        }

        private string MainImage(Product p)
        {
            var main = p.Images.FirstOrDefault(i => i.IsMain)
                    ?? p.Images.OrderBy(i => i.Order).FirstOrDefault();
            return AbsoluteImage(main?.ImagePath ?? p.ImagePath);
        }

        private object FormatProduct(Product p) => new
        {
            // ── Nomenclature PHP ───────────────────────────────────────
            id = p.Id,
            nom = p.Name,
            description = p.Desc,
            prix = p.Price,
            ancien_prix = p.OldPrice,
            stock = p.Quantity,
            categorie_id = p.CategoryId,
            categorie_nom = p.Category?.Title ?? "",
            categorie_slug = p.Category != null
                                ? SlugFromTitle(p.Category.Title)
                                : "",
            marque = p.Brand ?? "",
            badge = p.Badge ?? "",
            note = p.Rating,
            nb_avis = p.ReviewCount,
            actif = p.IsActive ? 1 : 0,
            image = MainImage(p),
            images = p.Images.OrderBy(i => i.Order).Select(img => AbsoluteImage(img.ImagePath)).ToList(),
            created_at = p.CreatedAt,
        };


        // GET /api/ecom/products?cat=...&q=...&marque=...&pmin=...&pmax=...&tri=...&page=...&limit=...
        [HttpGet("products")]

        public async Task<IActionResult> GetProducts(
            [FromQuery] string? cat = null,
            [FromQuery] string? q = null,
            [FromQuery] string? marque = null,   // nouveau
            [FromQuery] decimal? pmin = null,    // nouveau
            [FromQuery] decimal? pmax = null,    // nouveau
            [FromQuery] string tri = "recent",// nouveau
            [FromQuery] int page = 1,
            [FromQuery] int limit = 12)
        {
            page = Math.Max(1, page);
            limit = Math.Clamp(limit, 1, 100);

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(cat))
            {
                var title = SlugToTitle.TryGetValue(cat, out var t) ? t : cat;
                query = query.Where(p =>
                    p.Category != null &&
                    p.Category.Title.ToLower() == title.ToLower());
            }

            if (!string.IsNullOrEmpty(q))
                query = query.Where(p =>
                    p.Name.Contains(q) ||
                    p.Desc.Contains(q) ||
                    (p.Brand != null && p.Brand.Contains(q)));

            if (!string.IsNullOrEmpty(marque))
                query = query.Where(p => p.Brand == marque);

            if (pmin.HasValue)
                query = query.Where(p => p.Price >= pmin.Value);

            if (pmax.HasValue)
                query = query.Where(p => p.Price <= pmax.Value);

            // Tri
            query = tri switch
            {
                "prix_asc" => query.OrderBy(p => p.Price),
                "prix_desc" => query.OrderByDescending(p => p.Price),
                "note" => query.OrderByDescending(p => p.Rating)
                                    .ThenByDescending(p => p.ReviewCount),
                "promo" => query.OrderByDescending(p => p.OldPrice - p.Price),
                _ => query.OrderByDescending(p => p.CreatedAt), // recent
            };

            var total = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            // Marques disponibles pour la sidebar — sur le même filtre SANS filtre marque
            var marqueQuery = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.Brand != null && p.Brand != "");

            if (!string.IsNullOrEmpty(cat))
            {
                var title = SlugToTitle.TryGetValue(cat, out var t) ? t : cat;
                marqueQuery = marqueQuery.Where(p =>
                    p.Category != null &&
                    p.Category.Title.ToLower() == title.ToLower());
            }

            var marques = await marqueQuery
                .Select(p => p.Brand!)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            return Ok(new
            {
                produits = products.Select(FormatProduct),
                total = total,
                page = page,
                pages = (int)Math.Ceiling((double)total / limit),
                marques = marques,   // 👈 pour la sidebar marques
            });
        }

        // ── SINGLE ─────────────────────────────────────────────────────
        // GET /api/ecom/products/3
        [HttpGet("products/{id:int}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var p = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (p == null)
                return NotFound(new { erreur = "Produit introuvable" });

            // Produits similaires (même catégorie, max 4)
            var similaires = await _context.Products
                .Include(s => s.Category)
                .Include(s => s.Images)
                .Where(s => s.CategoryId == p.CategoryId
                         && s.Id != id
                         && s.IsActive)
                .Take(4)
                .ToListAsync();

            return Ok(new
            {
                // Tous les champs PHP
                id = p.Id,
                nom = p.Name,
                description = p.Desc,
                prix = p.Price,
                ancien_prix = p.OldPrice,
                stock = p.Quantity,
                categorie_id = p.CategoryId,
                categorie_nom = p.Category?.Title ?? "",
                categorie_slug = p.Category != null
                                    ? SlugFromTitle(p.Category.Title)
                                    : "",
                marque = p.Brand ?? "",
                badge = p.Badge ?? "",
                note = p.Rating,
                nb_avis = p.ReviewCount,
                actif = p.IsActive ? 1 : 0,
                image = MainImage(p),
                images = p.Images.OrderBy(i => i.Order).Select(img => AbsoluteImage(img.ImagePath)).ToList(),
                created_at = p.CreatedAt,

                // Champs supplémentaires pour product.php
                avis = p.Reviews.Select(r => new
                {
                    r.Id,
                    r.Author,
                    r.Comment,
                    r.Rating,
                    r.CreatedAt,
                    // Alias PHP
                    auteur = r.Author,
                    commentaire = r.Comment,
                    note = r.Rating,
                    created_at = r.CreatedAt,
                }),
                similaires = similaires.Select(FormatProduct),
            });
        }

        [HttpPost("reviews")]
        public async Task<IActionResult> PostReview([FromBody] ReviewDto dto)
        {
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null) return NotFound(new { erreur = "Produit introuvable" });

            var review = new Review
            {
                ProductId = dto.ProductId,
                Author = dto.Author,
                Rating = dto.Rating,
                Comment = dto.Comment,
            };

            _context.Reviews.Add(review);

            // Recalculer la note moyenne du produit
            var allReviews = await _context.Reviews
                .Where(r => r.ProductId == dto.ProductId)
                .ToListAsync();
            allReviews.Add(review);

            product.Rating = (float)allReviews.Average(r => r.Rating);
            product.ReviewCount = allReviews.Count;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { succes = true, review.Id });
        }

        // ── TENDANCE ───────────────────────────────────────────────────
        // GET /api/ecom/products/tendance
        [HttpGet("products/tendance")]
        public async Task<IActionResult> GetTendance()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.ReviewCount)
                .ThenByDescending(p => p.Rating)
                .Take(8)
                .ToListAsync();

            return Ok(new
            {
                produits = products.Select(FormatProduct),
                total = products.Count
            });
        }

        // ── MEILLEURS ──────────────────────────────────────────────────
        // GET /api/ecom/products/meilleurs
        [HttpGet("products/meilleurs")]
        public async Task<IActionResult> GetMeilleurs()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Rating)
                .ThenByDescending(p => p.ReviewCount)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                produits = products.Select(FormatProduct),
                total = products.Count
            });
        }

        // ── CATEGORIES ─────────────────────────────────────────────────
        // GET /api/ecom/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var cats = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();

            var result = cats.Select((c, index) => new
            {
                id = c.Id,
                nom = c.Title,
                slug = SlugFromTitle(c.Title),
                icone = TitleToIcone.TryGetValue(c.Title, out var icone)
                                ? icone
                                : "fas fa-tag",
                ordre = index + 1,
                nb_produits = c.Products.Count(p => p.IsActive),
                created_at = c.CreatedAt,
            });

            return Ok(result);
        }

        // ── HERO SLIDES ────────────────────────────────────────────────
        // GET /api/ecom/hero
        // Les hero slides ne sont pas liés au stock → ils restent en DB PHP
        // Cet endpoint les proxifie juste pour uniformiser les appels côté JS
        [HttpGet("hero")]
        public IActionResult GetHero()
        {
            // Option A : retourner vide si pas encore migré
            return Ok(Array.Empty<object>());

            // Option B (plus tard) : quand HeroSlide sera dans votre DbContext :
            // var slides = await _context.HeroSlides
            //     .Where(h => h.IsActive)
            //     .OrderBy(h => h.Ordre)
            //     .ToListAsync();
            // return Ok(slides);
        }
    }
}