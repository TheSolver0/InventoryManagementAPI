using GestionDeStock.API.Auth;
using GestionDeStock.API.Models;
using GestionDeStock.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Data;
using GestionDeStock.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace GestionDeStock.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IImageService _imageService;

        public ProductsController(AppDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        private object FormatProduct(Product p) => new
        {
            p.Id,
            p.Name,
            p.Desc,
            p.CategoryId,
            p.Category,
            p.Quantity,
            p.Price,
            p.OldPrice,
            p.Threshold,
            p.Sku,
            p.Location,
            p.Brand,
            p.Badge,
            p.IsActive,
            // Première image (principale) pour compatibilité
            ImagePath = p.Images.FirstOrDefault(i => i.IsMain)?.ImagePath
                     ?? p.Images.OrderBy(i => i.Order).FirstOrDefault()?.ImagePath
                     ?? p.ImagePath,
            ImageUrl = _imageService.BuildPublicUrl(Request,
                           p.Images.FirstOrDefault(i => i.IsMain)?.ImagePath
                        ?? p.Images.OrderBy(i => i.Order).FirstOrDefault()?.ImagePath
                        ?? p.ImagePath),
            Images = p.Images.OrderBy(i => i.Order).Select(img => new
            {
                img.Id,
                img.ImagePath,
                img.IsMain,
                img.Order,
                Url = _imageService.BuildPublicUrl(Request, img.ImagePath)
            }),
            p.CreatedAt,
            p.UpdatedAt
        };

        // GET api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .ToListAsync();

            return Ok(products.Select(FormatProduct));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetProduct(int id)
        {
            var p = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (p == null) return NotFound();

            return Ok(FormatProduct(p));
        }

        // POST — multipart/form-data, champ "Images" accepte plusieurs fichiers
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var category = await _context.Categories.FindAsync(dto.CategoryId);
                if (category == null) return BadRequest("Catégorie introuvable.");

                var product = new Product
                {
                    Id = 0,
                    Name = dto.Name,
                    Desc = dto.Desc,
                    CategoryId = dto.CategoryId,
                    Category = category,
                    Quantity = dto.Quantity,
                    Price = dto.Price,
                    OldPrice = dto.OldPrice,
                    Threshold = dto.Threshold,
                    Sku = dto.Sku ?? string.Empty,
                    Location = dto.Location ?? string.Empty,
                    Brand = dto.Brand,
                    Badge = dto.Badge,
                    IsActive = dto.IsActive,
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                if (dto.Images != null && dto.Images.Count > 0)
                {
                    for (int i = 0; i < dto.Images.Count; i++)
                    {
                        var path = await _imageService.SaveImageAsync(dto.Images[i]);
                        if (path != null)
                        {
                            _context.ProductImages.Add(new ProductImage
                            {
                                ProductId = product.Id,
                                ImagePath = path,
                                IsMain = i == 0,
                                Order = i
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // Recharger avec les images pour la réponse
                await _context.Entry(product).Collection(p => p.Images).LoadAsync();

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, FormatProduct(product));
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            try
            {
                // Si de nouvelles images sont fournies, remplacer toutes les existantes
                if (dto.Images != null && dto.Images.Count > 0)
                {
                    foreach (var img in product.Images)
                        _imageService.DeleteImage(img.ImagePath);
                    _context.ProductImages.RemoveRange(product.Images);

                    for (int i = 0; i < dto.Images.Count; i++)
                    {
                        var path = await _imageService.SaveImageAsync(dto.Images[i]);
                        if (path != null)
                        {
                            _context.ProductImages.Add(new ProductImage
                            {
                                ProductId = product.Id,
                                ImagePath = path,
                                IsMain = i == 0,
                                Order = i
                            });
                        }
                    }
                }

                product.Name = dto.Name;
                product.Desc = dto.Desc;
                product.CategoryId = dto.CategoryId;
                product.Quantity = dto.Quantity;
                product.Price = dto.Price;
                product.OldPrice = dto.OldPrice;
                product.Threshold = dto.Threshold;
                product.Sku = dto.Sku ?? string.Empty;
                product.Location = dto.Location ?? string.Empty;
                product.Brand = dto.Brand;
                product.Badge = dto.Badge;
                product.IsActive = dto.IsActive;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await _context.Entry(product).Collection(p => p.Images).LoadAsync();

                return Ok(FormatProduct(product));
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        // POST api/products/{id}/images — ajouter des photos sans remplacer les existantes
        [HttpPost("{id}/images")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddImages(int id, [FromForm] List<IFormFile> images)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            try
            {
                int nextOrder = product.Images.Any() ? product.Images.Max(i => i.Order) + 1 : 0;

                foreach (var file in images)
                {
                    var path = await _imageService.SaveImageAsync(file);
                    if (path != null)
                    {
                        _context.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.Id,
                            ImagePath = path,
                            IsMain = !product.Images.Any(), // principale si c'est la première
                            Order = nextOrder++
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await _context.Entry(product).Collection(p => p.Images).LoadAsync();

                return Ok(FormatProduct(product));
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        // DELETE api/products/{id}/images/{imageId} — supprimer une photo précise
        [HttpDelete("{id}/images/{imageId}")]
        [Authorize(Roles = Roles.AdminOrGerant)]
        public async Task<IActionResult> DeleteImage(int id, int imageId)
        {
            var image = await _context.ProductImages
                .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == id);
            if (image == null) return NotFound();

            _imageService.DeleteImage(image.ImagePath);
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            // Si l'image supprimée était la principale, promouvoir la suivante
            if (image.IsMain)
            {
                var next = await _context.ProductImages
                    .Where(i => i.ProductId == id)
                    .OrderBy(i => i.Order)
                    .FirstOrDefaultAsync();
                if (next != null)
                {
                    next.IsMain = true;
                    await _context.SaveChangesAsync();
                }
            }

            return NoContent();
        }

        // PATCH api/products/{id}/images/{imageId}/main — définir comme image principale
        [HttpPatch("{id}/images/{imageId}/main")]
        public async Task<IActionResult> SetMainImage(int id, int imageId)
        {
            var images = await _context.ProductImages
                .Where(i => i.ProductId == id)
                .ToListAsync();

            var target = images.FirstOrDefault(i => i.Id == imageId);
            if (target == null) return NotFound();

            foreach (var img in images)
                img.IsMain = img.Id == imageId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.AdminOrGerant)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Suppliers)
                .Include(p => p.Reviews)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var movements = await _context.Movements.Where(m => m.ProductId == id).ToListAsync();
            _context.Movements.RemoveRange(movements);

            var stockMovements = await _context.StockMovements.Where(m => m.ProductId == id).ToListAsync();
            _context.StockMovements.RemoveRange(stockMovements);

            var orders = await _context.Orders.Where(o => o.ProductId == id).ToListAsync();
            _context.Orders.RemoveRange(orders);

            var provides = await _context.Provides.Where(p => p.ProductId == id).ToListAsync();
            _context.Provides.RemoveRange(provides);

            var inventoryLines = await _context.InventoryLines.Where(il => il.ProductId == id).ToListAsync();
            _context.InventoryLines.RemoveRange(inventoryLines);

            var reviews = await _context.Reviews.Where(r => r.ProductId == id).ToListAsync();
            _context.Reviews.RemoveRange(reviews);

            product.Suppliers.Clear();

            // Supprimer les fichiers images du disque
            foreach (var img in product.Images)
                _imageService.DeleteImage(img.ImagePath);
            // Les lignes ProductImages sont supprimées en cascade (DeleteBehavior.Cascade)

            // Supprimer aussi l'ancienne image legacy si présente
            _imageService.DeleteImage(product.ImagePath);

            await _context.SaveChangesAsync();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
