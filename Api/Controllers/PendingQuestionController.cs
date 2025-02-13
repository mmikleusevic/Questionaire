using Microsoft.AspNetCore.Mvc;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PendingQuestionController(IPendingQuestionService pendingQuestionService,
    ILogger<PendingQuestionController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PendingQuestion>>> GetPendingQuestions()
    {
        try
        {
            List<PendingQuestion> pendingQuestions = await pendingQuestionService.GetPendingQuestionsAsync();
            if (pendingQuestions.Count == 0) return NotFound("No pending questions found!");
            return Ok(pendingQuestions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving the questions.");
            return StatusCode(500, new { Message = "An error occurred while retrieving the questions.", Details = ex.Message });
        }
    }
    
    [HttpGet("{pendingQuestionId}")]
    public async Task<IActionResult> GetPendingQuestionById(int pendingQuestionId)
    {
        try
        {
            PendingQuestion? pendingQuestion = await pendingQuestionService.GetPendingQuestionAsync(pendingQuestionId);
            if (pendingQuestion == null) return NotFound($"Question with ID {pendingQuestionId} not found.");
            return Ok(pendingQuestion);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while retrieving the pending question with ID {pendingQuestionId}.");
            return StatusCode(500, new { Message = $"An error occurred while retrieving the pending question with ID {pendingQuestionId}.", Details = ex.Message });
        }
    }
    
    [HttpPost("{pendingQuestionId}")]
    public async Task<ActionResult<List<PendingQuestion>>> ApproveQuestion(int pendingQuestionId)
    {
        try
        {
            await pendingQuestionService.ApproveQuestion(pendingQuestionId);
            return Ok(new { Message = "Question approved successfully." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while approving the pending question with ID {pendingQuestionId}.");
            return StatusCode(500, new { Message = $"An error occurred while approving the pending question with ID {pendingQuestionId}.", Details = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePendingQuestion([FromBody] PendingQuestion? pendingQuestion)
    {
        if (pendingQuestion == null) return BadRequest("Pending question data cannot be null.");
        
        try
        {
            await pendingQuestionService.CreatePendingQuestion(pendingQuestion);
            return CreatedAtAction(nameof(GetPendingQuestionById), new { id = pendingQuestion.Id }, pendingQuestion);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while saving the pending question.");
            return StatusCode(500, new { Message = "An error occurred while saving the pending question.", Details = ex.Message });
        }
    }

    [HttpPut("{pendingQuestionId}")]
    public async Task<IActionResult> UpdatePendingQuestion(int pendingQuestionId, [FromBody] UpdatePendingQuestionRequestDto? updateRequest)
    {
        if (updateRequest == null) return BadRequest("Update pending question data cannot be null.");
        
        try
        {
            bool success = await pendingQuestionService.UpdatePendingQuestion(pendingQuestionId, updateRequest);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while updating the pending question with ID {pendingQuestionId}.");
            return StatusCode(500, new { Message = $"An error occurred while updating the pending question with ID {pendingQuestionId}.", Details = ex.Message });
        }
    }
    
    [HttpDelete("{pendingQuestionId}")]
    public async Task<IActionResult> DeletePendingQuestion(int pendingQuestionId)
    {
        try
        {
            bool success = await pendingQuestionService.DeletePendingQuestion(pendingQuestionId);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while deleting the pending question with ID {pendingQuestionId}.");
            return StatusCode(500, new { Message = $"An error occurred while deleting the pending question with ID {pendingQuestionId}.", Details = ex.Message });
        }
    }
}