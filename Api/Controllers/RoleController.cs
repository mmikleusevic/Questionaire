using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionaireApi.Interfaces;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
[ApiController]
public class RoleController(
    IRoleService roleService,
    ILogger<UserController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        try
        {
            IList<string> roles = await roleService.GetRoles();

            if (roles.Count == 0) return NotFound("No roles found.");
            return Ok(roles);
        }
        catch (Exception ex)
        {
            string message = "An error occurred while retrieving roles.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }
}