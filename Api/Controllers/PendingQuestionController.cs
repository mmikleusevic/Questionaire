using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class PendingQuestionController(
    IPendingQuestionService pendingQuestionService,
    ILogger<PendingQuestionController> logger) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedResponse<PendingQuestion>>> GetPendingQuestions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Page number and page size must be greater than 0.");

            PaginatedResponse<PendingQuestionDto> response =
                await pendingQuestionService.GetPendingQuestions(pageNumber, pageSize, User);

            if (response.Items.Count == 0) return NotFound("No pending questions found.");

            return Ok(response);
        }
        catch (Exception ex)
        {
            string message = "An error occurred while retrieving the pending questions.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreatePendingQuestion([FromBody] PendingQuestionDto? newPendingQuestion)
    {
        try
        {
            if (newPendingQuestion == null) return BadRequest("Pending question data cannot be null.");

            await pendingQuestionService.CreatePendingQuestion(newPendingQuestion, User);
            return Created();
        }
        catch (Exception ex)
        {
            string message = "An error occurred while saving the pending question.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpPut("approve/{id}")]
    [Authorize(Roles = "Admin, SuperAdmin")]
    public async Task<IActionResult> ApprovePendingQuestion(int id)
    {
        try
        {
            bool success = await pendingQuestionService.ApprovePendingQuestion(id, User);
            if (!success) return NotFound($"Pending question with ID {id} not found.");
            return Ok("Pending question approved successfully.");
        }
        catch (Exception ex)
        {
            string message = $"An error occurred while approving the pending question with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePendingQuestion(int id,
        [FromBody] PendingQuestionDto? updatedPendingQuestion)
    {
        try
        {
            if (updatedPendingQuestion == null) return BadRequest("Update pending question data cannot be null.");

            bool success = await pendingQuestionService.UpdatePendingQuestion(id, updatedPendingQuestion, User);
            if (!success) return NotFound($"Pending question with ID {id} not found.");

            return Ok($"Pending question with ID {id} updated successfully.");
        }
        catch (Exception ex)
        {
            string message = $"An error occurred while updating the pending question with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePendingQuestion(int id)
    {
        try
        {
            bool success = await pendingQuestionService.DeletePendingQuestion(id, User);
            if (!success) return NotFound($"Pending question with ID {id} not found.");
            return Ok($"Pending question with ID {id} deleted successfully.");
        }
        catch (Exception ex)
        {
            string message = $"An error occurred while deleting the pending question with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }
}