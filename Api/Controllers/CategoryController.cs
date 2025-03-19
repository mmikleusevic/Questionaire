using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionaireApi.Interfaces;
using Shared.Models;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class CategoryController(
    ICategoryService categoryService,
    ILogger<CategoryController> logger) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            CategoriesDto categoriesDto = await categoryService.GetCategories();
            if (categoriesDto.FlatCategories.Count == 0 ||
                categoriesDto.NestedCategories.Count == 0) return NotFound("No categories found.");

            return Ok(categoriesDto);
        }
        catch (Exception ex)
        {
            string message = "An error occurred while retrieving categories.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpGet("nested")]
    [AllowAnonymous]
    public async Task<IActionResult> GetNestedCategories()
    {
        try
        {
            List<CategoryExtendedDto> categories = await categoryService.GetNestedCategories();
            if (categories.Count == 0) return NotFound("No categories found.");
            return Ok(categories);
        }
        catch (Exception ex)
        {
            string message = "An error occurred while retrieving categories.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpGet("flat")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFlatCategories()
    {
        try
        {
            List<CategoryExtendedDto> categories = await categoryService.GetFlatCategories();
            if (categories.Count == 0) return NotFound("No categories found.");
            return Ok(categories);
        }
        catch (Exception ex)
        {
            string message = "An error occurred while retrieving categories.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin, SuperAdmin")]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryExtendedDto? newCategory)
    {
        try
        {
            if (newCategory == null) return BadRequest("Category data cannot be null.");

            await categoryService.CreateCategory(newCategory);
            return Created();
        }
        catch (Exception ex)
        {
            string message = "An error occurred while creating the category.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin, SuperAdmin")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryExtendedDto? updatedCategory)
    {
        try
        {
            if (updatedCategory == null) return BadRequest("Updated category data cannot be null.");

            bool success = await categoryService.UpdateCategory(id, updatedCategory);
            if (!success) return NotFound($"Category with ID {id} not found.");
            return Ok($"Category with ID {id} updated successfully.");
        }
        catch (Exception ex)
        {
            string message = $"An error occurred while updating the category with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin, SuperAdmin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            bool success = await categoryService.DeleteCategory(id);
            if (!success) return NotFound($"Category with ID {id} not found.");
            return Ok($"Category with ID {id} deleted successfully.");
        }
        catch (Exception ex)
        {
            string message = $"An error occurred while deleting the category with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }
}