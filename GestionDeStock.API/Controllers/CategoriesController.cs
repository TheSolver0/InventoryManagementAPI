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

[HttpDelete("{id}")]
public async Task<IActionResult> DeleteCategory(int id)
{
    var category = await _context.Categories.FindAsync(id);
    if (category == null) return NotFound();

    _context.Categories.Remove(category);
    await _context.SaveChangesAsync();
    return NoContent();
}
    }

}