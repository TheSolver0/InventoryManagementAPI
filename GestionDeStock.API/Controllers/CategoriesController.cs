using GestionDeStock.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Data;
using Microsoft.AspNetCore.Authorization;

namespace GestionDeStock.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();

            return Ok(categories);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            return Ok(category);
        }
       [HttpPost]
public async Task<ActionResult<Category>> CreateCategory([FromBody] Category category)
{
    _context.Categories.Add(category);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
}

[HttpPut("{id}")]
public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
{
    if (id != category.Id) return BadRequest();

    var existing = await _context.Categories.FindAsync(id);
    if (existing == null) return NotFound();

    existing.Title = category.Title;
    existing.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();
    return Ok(existing);
}

[HttpDelete("{id}")]
public async Task<IActionResult> DeleteCategory(int id)
{
    var category = await _context.Categories
        .Include(c => c.Products) // ✅ charge les produits liés
        .FirstOrDefaultAsync(c => c.Id == id);

    if (category == null)
        return NotFound(new { message = "Catégorie non trouvée." });

    if (category.Products.Any())
        return BadRequest(new { message = "Impossible de supprimer une catégorie contenant des produits." });

    _context.Categories.Remove(category);
    await _context.SaveChangesAsync();

    return NoContent();
}
    }

}