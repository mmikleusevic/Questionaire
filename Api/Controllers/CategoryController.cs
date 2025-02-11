using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        List<CategoryDto> categories = await categoryService.GetCategoriesAsync();
        if (categories.Count == 0) return NotFound("No categories found!");
        return Ok(categories);
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<Category>> GetCategoryById(int id)
    {
        Category category = await categoryService.GetCategoryByIdAsync(id);
        if (category is null) return NotFound();
        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<Category>> AddCategory(Category? newCategory)
    {
        if (newCategory is null) return BadRequest();
        
        Category category = await categoryService.AddCategoryAsync(newCategory);
        return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, Category? updatedCategory)
    {
        if (updatedCategory is null) return BadRequest();
        
        bool success = await categoryService.UpdateCategoryAsync(id, updatedCategory);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        bool success = await categoryService.DeleteCategoryAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}