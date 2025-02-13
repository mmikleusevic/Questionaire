using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategoryService categoryService,
    ILogger<CategoryController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        try
        {
            List<CategoryDto> categories = await categoryService.GetCategoriesAsync();
            if (categories.Count == 0) return NotFound("No categories found.");
            return Ok(categories);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving categories.");
            return StatusCode(500, "An error occurred while retrieving categories.");
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(int id)
    {
        try
        {
            Category? category = await categoryService.GetCategoryByIdAsync(id);
            if (category is null) return NotFound($"Category with ID {id} not found.");
            return Ok(category);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while retrieving the category with ID {id}.");
            return StatusCode(500, $"An error occurred while retrieving the category with ID {id}.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] Category? newCategory)
    {
        if (newCategory == null) return BadRequest("Category data cannot be null.");

        try
        {
            await categoryService.CreateCategoryAsync(newCategory);
            return CreatedAtAction(nameof(GetCategoryById), new { newCategory = newCategory.Id }, newCategory);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the category.");
            return StatusCode(500, "An error occurred while creating the category.");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category? updatedCategory)
    {
        if (updatedCategory == null) return BadRequest("Updated category data cannot be null.");

        try
        {
            bool success = await categoryService.UpdateCategoryAsync(id, updatedCategory);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while updating the category with ID {id}.");
            return StatusCode(500, $"An error occurred while updating the category with ID {id}.");
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            bool success = await categoryService.DeleteCategoryAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while deleting the category with ID {id}.");
            return StatusCode(500, $"An error occurred while deleting the category with ID {id}.");
        }
    }
}
