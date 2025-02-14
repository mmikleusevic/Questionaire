using Microsoft.AspNetCore.Mvc;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService,
    ILogger<UserController> logger) : ControllerBase
{
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                List<User> users = await userService.GetUsersAsync();
                if (users.Count == 0) return NotFound("No users found!");
                return Ok(users);
            }
            catch (Exception ex)
            {
                string message =  "An error occurred while retrieving the users";
                logger.LogError(ex, message);
                return StatusCode(500, message);
            }
        }
        
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(int userId)
        {
            try
            {
                User? user = await userService.GetUserByIdAsync(userId);
                if (user == null) return NotFound($"User with ID {userId} not found.");
                return Ok(user);
            }
            catch (Exception ex)
            {
                string message = $"An error occurred while retrieving the user with ID {userId}.";
                logger.LogError(ex, message);
                return StatusCode(500, message);
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User? user)
        {
            if (user == null) return BadRequest("User data cannot be null.");
            
            try
            {
                await userService.CreateUserAsync(user);
                return CreatedAtAction(nameof(GetUser), new { userId = user.Id }, user);
            }
            catch (Exception ex)
            {
                string message = "An error occurred while saving the user.";
                logger.LogError(ex, message);
                return StatusCode(500, message);
            }
        }
        
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] User? updatedUser)
        {
            if (updatedUser == null) return BadRequest("Update user data cannot be null.");
            
            try
            {
                bool success = await userService.UpdateUserAsync(userId, updatedUser);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                string message = $"An error occurred while updating the user with ID {userId}.";
                logger.LogError(ex, message);
                return StatusCode(500, message);
            }
        }
        
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                bool success = await userService.DeleteUserAsync(userId);
                if (!success) return NotFound();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                string message = $"An error occurred while deleting the user with ID {userId}.";
                logger.LogError(ex, message);
                return StatusCode(500, message);
            }
        }
}