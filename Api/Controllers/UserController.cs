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
            if (!success) return NotFound($"User with username {updatedUser.UserName} not found.");
            return Ok($"User with username {updatedUser.UserName} updated successfully.");
        }
        catch (Exception ex)
        {
            string message = $"An error occurred while updating the user with username {updatedUser?.UserName}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpDelete("{userName}")]
    public async Task<IActionResult> DeleteUser(string userName)
    {
        try
        {
            bool success = await userService.DeleteUser(userName);
            if (!success) return NotFound($"User with username {userName} not found or user was not deleted.");
            return Ok($"User with username {userName} deleted successfully.");
        }
        catch (Exception ex)
        {
            string message = $"An error occurred while deleting the user with username {userName}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }
}