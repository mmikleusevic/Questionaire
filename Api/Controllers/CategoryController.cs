using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Models;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(QuestionaireDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Category>>> GetCategories()
    {
        return Ok(await context.Categories.ToListAsync());
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<Category>> GetCategoryById(int id)
    {
        Category category = await context.Categories.FindAsync(id);

        if (category is null) return NotFound();
                        
        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<Category>> AddCategory(Category? newCategory)
    {
        if (newCategory is null) return BadRequest();
                
        context.Categories.Add(newCategory);
        await context.SaveChangesAsync();
                
        return CreatedAtAction(nameof(GetCategoryById), new { id = newCategory.Id }, newCategory);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, Category? updatedCategory)
    {
        Category category = await context.Categories.FindAsync(id);
                
        if (category is null) return NotFound();
                
        category.CategoryName = updatedCategory.CategoryName;
                
        await context.SaveChangesAsync();
                
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        Category category = await context.Categories.FindAsync(id);
                
        if (category is null) return NotFound();
                
        context.Categories.Remove(category);
        await context.SaveChangesAsync();
                
        return NoContent();
    }
}