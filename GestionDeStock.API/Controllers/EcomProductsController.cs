using GestionDeStock.API.Data;
using GestionDeStock.API.Models;
using GestionDeStock.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        private object FormatProduct(Product p) => new
        {
            // ── Nomenclature PHP ───────────────────────────────────────
            id             = p.Id,
            nom            = p.Name,
            description    = p.Desc,
            prix           = p.Price,
            ancien_prix    = p.OldPrice,
            stock          = p.Quantity,
            categorie_id   = p.CategoryId,
            categorie_nom  = p.Category?.Title ?? "",
            categorie_slug = p.Category != null
                                ? SlugFromTitle(p.Category.Title)
                                : "",
            marque         = p.Brand  ?? "",
            badge          = p.Badge  ?? "",
            note           = p.Rating,
            nb_avis        = p.ReviewCount,
            actif          = p.IsActive ? 1 : 0,
            image          = AbsoluteImage(p.ImagePath),
            created_at     = p.CreatedAt,
        };

        // ── LISTE ──────────────────────────────────────────────────────
        // GET /api/ecom/products?cat=laptops&page=1&limit=8
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts(
            [FromQuery] string? cat   = null,
            [FromQuery] string? q     = null,
            [FromQuery] int     page  = 1,
            [FromQuery] int     limit = 8)
        {
            page  = Math.Max(1, page);
            limit = Math.Clamp(limit, 1, 100);

            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsQueryable();

            // Filtre catégorie : slug PHP → Title .NET
            if (!string.IsNullOrEmpty(cat))
            {
                // Résolution : slug → title via dictionnaire, sinon auto-match
                var title = SlugToTitle.TryGetValue(cat, out var t)
                    ? t
                    : cat; // fallback : utilise le slug tel quel

                query = query.Where(p =>
                    p.Category != null &&
                    p.Category.Title.ToLower() == title.ToLower());
            }

            // Recherche texte
            if (!string.IsNullOrEmpty(q))
                query = query.Where(p =>
                    p.Name.Contains(q) ||
                    p.Desc.Contains(q) ||
                    (p.Brand != null && p.Brand.Contains(q)) ||
                    (p.Category != null && p.Category.Title.Contains(q)));

            var total = await query.CountAsync();

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return Ok(new
            {
                produits = products.Select(FormatProduct),
                total    = total,
                page     = page,
                pages    = (int)Math.Ceiling((double)total / limit),
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
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (p == null)
                return NotFound(new { erreur = "Produit introuvable" });

            // Produits similaires (même catégorie, max 4)
            var similaires = await _context.Products
                .Include(s => s.Category)
                .Where(s => s.CategoryId == p.CategoryId
                         && s.Id != id
                         && s.IsActive)
                .Take(4)
                .ToListAsync();

            return Ok(new
            {
                // Tous les champs PHP
                id             = p.Id,
                nom            = p.Name,
                description    = p.Desc,
                prix           = p.Price,
                ancien_prix    = p.OldPrice,
                stock          = p.Quantity,
                categorie_id   = p.CategoryId,
                categorie_nom  = p.Category?.Title ?? "",
                categorie_slug = p.Category != null
                                    ? SlugFromTitle(p.Category.Title)
                                    : "",
                marque         = p.Brand  ?? "",
                badge          = p.Badge  ?? "",
                note           = p.Rating,
                nb_avis        = p.ReviewCount,
                actif          = p.IsActive ? 1 : 0,
                image          = AbsoluteImage(p.ImagePath),
                created_at     = p.CreatedAt,

                // Champs supplémentaires pour product.php
                avis       = p.Reviews.Select(r => new
                {
                    r.Id,
                    r.Author,
                    r.Comment,
                    r.Rating,
                    r.CreatedAt,
                    // Alias PHP
                    auteur      = r.Author,
                    commentaire = r.Comment,
                    note        = r.Rating,
                    created_at  = r.CreatedAt,
                }),
                similaires = similaires.Select(FormatProduct),
            });
        }

        // ── TENDANCE ───────────────────────────────────────────────────
        // GET /api/ecom/products/tendance
        [HttpGet("products/tendance")]
        public async Task<IActionResult> GetTendance()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.ReviewCount)
                .ThenByDescending(p => p.Rating)
                .Take(8)
                .ToListAsync();

            return Ok(new
            {
                produits = products.Select(FormatProduct),
                total    = products.Count
            });
        }

        // ── MEILLEURS ──────────────────────────────────────────────────
        // GET /api/ecom/products/meilleurs
        [HttpGet("products/meilleurs")]
        public async Task<IActionResult> GetMeilleurs()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Rating)
                .ThenByDescending(p => p.ReviewCount)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                produits = products.Select(FormatProduct),
                total    = products.Count
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
                // Nomenclature .NET
                c.Id,
                c.Title,
                c.CreatedAt,

                // Nomenclature PHP — tout calculé, zéro migration
                id          = c.Id,
                nom         = c.Title,
                slug        = SlugFromTitle(c.Title),
                icone       = TitleToIcone.TryGetValue(c.Title, out var icone)
                                ? icone
                                : "fas fa-tag",              // icône par défaut
                ordre       = index + 1,                     // ordre = position dans la liste
                nb_produits = c.Products.Count(p => p.IsActive),
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