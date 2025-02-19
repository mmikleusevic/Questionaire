using Microsoft.AspNetCore.Mvc;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;

namespace QuestionaireApi.Controllers;

public class RoleController(IRoleService roleService,
    ILogger<RoleController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        try
        {
            List<Role> roles = await roleService.GetRolesAsync();
            if (roles.Count == 0) return NotFound("No roles found.");
            return Ok(roles);
        }
        catch (Exception ex)
        {
            string message =  "An error occurred while retrieving the roles.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRole(int id)
    {
        try
        {
            Role? role = await roleService.GetRoleByIdAsync(id);
            if (role == null) return NotFound($"Role with ID {id} not found.");
            return Ok(role);
        }
        catch (Exception ex)
        {
            string message = $"An error occurred while retrieving the role with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] Role? newRole)
    {
        if (newRole == null) return BadRequest("Role data cannot be null.");
        
        try
        {
            await roleService.CreateRoleAsync(newRole);
            return Created();
        }
        catch (Exception ex)
        {
            string message = "An error occurred while saving the role.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] Role? updatedRole)
    {
        if (updatedRole == null) return BadRequest("Update role data cannot be null.");
        
        try
        {
            bool success = await roleService.UpdateRoleAsync(id, updatedRole);
            if (!success) return NotFound($"Role with ID {id} not found.");
            return Ok($"Role with ID {id} updated successfully.");
        }
        catch (Exception ex)
        {
            string message = $"An error occurred while updating the role with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        try
        {
            bool success = await roleService.DeleteRoleAsync(id);
            if (!success) return NotFound($"Role with ID {id} not found.");
            
            return Ok($"Role with ID {id} deleted successfully.");
        }
        catch (Exception ex)
        {
            string message = $"An error occurred while deleting the role with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }
}