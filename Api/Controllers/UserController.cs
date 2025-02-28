using Microsoft.AspNetCore.Mvc;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

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
                List<User> users = await userService.GetUsers();
                if (users.Count == 0) return NotFound("No users found.");
                return Ok(users);
            }
            catch (Exception ex)
            {
                string message =  "An error occurred while retrieving the users.";
                logger.LogError(ex, message);
                return StatusCode(500, message);
            }
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                UserDto? user = await userService.GetUserById(id);
                if (user == null) return NotFound($"User with ID {id} not found.");
                return Ok(user);
            }
            catch (Exception ex)
            {
                string message = $"An error occurred while retrieving the user with ID {id}.";
                logger.LogError(ex, message);
                return StatusCode(500, message);
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserDto? newUser)
        {
            if (newUser == null) return BadRequest("User data cannot be null.");
            
            try
            {
                await userService.CreateUser(newUser);
                return Created();
            }
            catch (Exception ex)
            {
                string message = "An error occurred while saving the user.";
                logger.LogError(ex, message);
                return StatusCode(500, message);
            }
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto? updatedUser)
        {
            if (updatedUser == null) return BadRequest("Update user data cannot be null.");
            
            try
            {
                bool success = await userService.UpdateUser(id, updatedUser);
                if (!success) return NotFound($"User with ID {id} not found.");
                return Ok($"User with ID {id} updated successfully.");
            }
            catch (Exception ex)
            {
                string message = $"An error occurred while updating the user with ID {id}.";
                logger.LogError(ex, message);
                return StatusCode(500, message);
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                bool success = await userService.DeleteUser(id);
                if (!success) return NotFound($"User with ID {id} not found.");
                
                return Ok($"User with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                string message = $"An error occurred while deleting the user with ID {id}.";
                logger.LogError(ex, message);
                return StatusCode(500, message);
            }
        }
}