using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionaireApi.Interfaces;
using Shared.Models;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
[ApiController]
public class UserController(
    IUserService userService,
    ILogger<UserController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            List<UserDto> users = await userService.GetUsers();

            if (users.Count == 0) return NotFound("No users found.");
            return Ok(users);
        }
        catch (Exception ex)
        {
            string message = "An error occurred while retrieving users.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUserRole([FromBody] UserDto? updatedUser)
    {
        try
        {
            if (updatedUser == null) return BadRequest("Updated user data cannot be null.");

            bool success = await userService.UpdateUser(updatedUser);
            if (!success) return NotFound($"User with email {updatedUser.Email} not found.");
            return Ok($"User with email {updatedUser.Email} updated successfully.");
        }
        catch (Exception ex)
        {
            string message = $"An error occurred while updating the user with email {updatedUser?.Email}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpDelete("{email}")]
    public async Task<IActionResult> DeleteUser(string email)
    {
        try
        {
            bool success = await userService.DeleteUser(email);
            if (!success) return NotFound($"User with email {email} not found.");
            return Ok($"User with email {email} deleted successfully.");
        }
        catch (Exception ex)
        {
            string message = $"An error occurred while deleting the user with email {email}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }
}