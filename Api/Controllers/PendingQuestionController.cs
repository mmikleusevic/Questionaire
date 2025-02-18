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
            string message = "An error occurred while retrieving the questions.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
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
            string message = $"An error occurred while retrieving the pending question with ID {pendingQuestionId}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
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
            string message = $"An error occurred while approving the pending question with ID {pendingQuestionId}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePendingQuestion([FromBody] PendingQuestion? pendingQuestion)
    {
        if (pendingQuestion == null) return BadRequest("Pending question data cannot be null.");
        
        try
        {
            await pendingQuestionService.CreatePendingQuestion(pendingQuestion);
            return Created();
        }
        catch (Exception ex)
        {
            string message = "An error occurred while saving the pending question.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
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
            string message = $"An error occurred while updating the pending question with ID {pendingQuestionId}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
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
            string message = $"An error occurred while deleting the pending question with ID {pendingQuestionId}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }
}