using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionaireApi.Interfaces;
using SharedStandard.Models;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[AllowAnonymous]
[ApiController]
public class UserQuestionHistoryController(
    IUserQuestionHistoryService userQuestionHistoryService,
    ILogger<CategoryController> logger) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUserHistory(
        [FromBody] UserQuestionHistoryDto? userQuestionHistory)
    {
        try
        {
            if (userQuestionHistory == null) return BadRequest("User question history data cannot be null.");

            await userQuestionHistoryService.CreateUserQuestionHistory(userQuestionHistory.UserId,
                userQuestionHistory.QuestionIds);

            return Created();
        }
        catch (Exception ex)
        {
            string message = "An error occurred while creating user question history.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }
}