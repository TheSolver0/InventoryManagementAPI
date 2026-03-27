using GestionDeStock.API.Models;
using GestionDeStock.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Data;
using GestionDeStock.API.Services;

namespace GestionDeStock.API.Controllers
{
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

    // GET api/products — avec URL image absolue pour le site PHP
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetProducts()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .ToListAsync();

        var result = products.Select(p => new
        {
            p.Id, p.Name, p.Desc, p.CategoryId, p.Category,
            p.Quantity, p.Price, p.Threshold, p.Sku, p.Location,
            p.ImagePath,
            ImageUrl = _imageService.BuildPublicUrl(Request, p.ImagePath), // 👈 URL absolue
            p.CreatedAt, p.UpdatedAt
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetProduct(int id)
    {
        var p = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (p == null) return NotFound();

        return Ok(new
        {
            p.Id, p.Name, p.Desc, p.CategoryId, p.Category,
            p.Quantity, p.Price, p.Threshold, p.Sku, p.Location,
            p.ImagePath,
            ImageUrl = _imageService.BuildPublicUrl(Request, p.ImagePath),
            p.CreatedAt, p.UpdatedAt
        });
    }

    // POST — multipart/form-data obligatoire pour l'image
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateProduct([FromForm] ProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null) return BadRequest("Catégorie introuvable.");

            var imagePath = await _imageService.SaveImageAsync(dto.Image);

            var product = new Product
            {
                Id = 0,
                Name = dto.Name,
                Desc = dto.Desc,
                CategoryId = dto.CategoryId,
                Category = category,
                Quantity = dto.Quantity,
                Price = dto.Price,
                Threshold = dto.Threshold,
                Sku = dto.Sku,
                Location = dto.Location,
                ImagePath = imagePath,
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new
            {
                product.Id, product.Name, product.ImagePath,
                ImageUrl = _imageService.BuildPublicUrl(Request, product.ImagePath)
            });
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
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        try
        {
            // Remplace l'image seulement si une nouvelle est envoyée
            if (dto.Image != null)
            {
                _imageService.DeleteImage(product.ImagePath); // supprime l'ancienne
                product.ImagePath = await _imageService.SaveImageAsync(dto.Image);
            }

            product.Name = dto.Name;
            product.Desc = dto.Desc;
            product.CategoryId = dto.CategoryId;
            product.Quantity = dto.Quantity;
            product.Price = dto.Price;
            product.Threshold = dto.Threshold;
            product.Sku = dto.Sku;
            product.Location = dto.Location;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                product.Id, product.Name, product.ImagePath,
                ImageUrl = _imageService.BuildPublicUrl(Request, product.ImagePath)
            });
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        _imageService.DeleteImage(product.ImagePath); // 👈 supprime le fichier aussi
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

}