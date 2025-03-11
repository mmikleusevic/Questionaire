using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class QuestionController(
    IQuestionService questionService,
    ILogger<QuestionController> logger) : ControllerBase
{
    [HttpPost("paged")]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedResponse<QuestionDto>>> GetQuestions(
        [FromBody] QuestionsRequestDto questionsRequestDto)
    {
        try
        {
            if (questionsRequestDto.PageNumber < 1 || questionsRequestDto.PageSize < 1)
                return BadRequest("Page number and page size must be greater than 0.");

            PaginatedResponse<QuestionDto> response = await questionService.GetQuestions(questionsRequestDto, User);

            if (response.Items.Count == 0) return NotFound("No questions found.");

            return Ok(response);
        }
        catch (Exception ex)
        {
            string message = "An error occurred while retrieving questions.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpPost("random")]
    [AllowAnonymous]
    public async Task<ActionResult<List<QuestionDto>>> GetRandomUniqueQuestions(
        [FromBody] GetRandomUniqueQuestionsRequestDto? request)
    {
        try
        {
            if (request == null) return BadRequest("Get random unique questions data cannot be null.");

            List<QuestionDto> questions = await questionService.GetRandomUniqueQuestions(request);
            if (questions.Count == 0) return NotFound("No questions found.");
            return Ok(questions);
        }
        catch (Exception ex)
        {
            string message = "An error occurred while retrieving random unique questions.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin, SuperAdmin")]
    public async Task<IActionResult> UpdateQuestion(int id, [FromBody] QuestionDto? updatedQuestion)
    {
        try
        {
            if (updatedQuestion == null) return BadRequest("Updated question data cannot be null.");

            bool success = await questionService.UpdateQuestion(id, updatedQuestion, User);
            if (!success) return NotFound($"Question with ID {id} not found.");
            return Ok($"Question with ID {id} updated successfully.");
        }
        catch (Exception ex)
        {
            string message = $"An error occurred while updating the question with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin, SuperAdmin")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        try
        {
            bool success = await questionService.DeleteQuestion(id, User);
            if (!success) return NotFound($"Question with ID {id} not found.");
            return Ok($"Question with ID {id} deleted successfully.");
        }
        catch (Exception ex)
        {
            string message = $"An error occurred while deleting the question with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }
}