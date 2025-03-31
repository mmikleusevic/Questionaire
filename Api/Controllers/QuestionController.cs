using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionaireApi.Interfaces;
using Shared.Models;
using SharedStandard.Models;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class QuestionController(
    IQuestionService questionService,
    ILogger<QuestionController> logger) : ControllerBase
{
    [HttpPost("paged")]
    [Authorize(Roles = "Admin, SuperAdmin, User")]
    public async Task<ActionResult<PaginatedResponse<QuestionExtendedDto>>> GetQuestions(
        [FromBody] QuestionsRequestDto questionsRequestDto)
    {
        try
        {
            if (questionsRequestDto.PageNumber < 1 || questionsRequestDto.PageSize < 1)
                return BadRequest("Page number and page size must be greater than 0.");

            PaginatedResponse<QuestionExtendedDto> response =
                await questionService.GetQuestions(questionsRequestDto, User);

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
    public async Task<ActionResult<List<QuestionExtendedDto>>> GetRandomUniqueQuestions(
        [FromBody] UniqueQuestionsRequestDto? request)
    {
        try
        {
            if (request == null) return BadRequest("Get random unique questions data cannot be null.");

            List<QuestionExtendedDto> questions = await questionService.GetRandomUniqueQuestions(request);
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

    [HttpPut("approve/{id}")]
    [Authorize(Roles = "Admin, SuperAdmin")]
    public async Task<IActionResult> ApproveQuestion(int id)
    {
        try
        {
            bool success = await questionService.ApproveQuestion(id, User);
            if (!success) return NotFound($"Question with ID {id} not found.");
            return Ok("Question approved successfully.");
        }
        catch (Exception ex)
        {
            string message = $"An error occurred while approving the question with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpPost("create")]
    [Authorize(Roles = "Admin, SuperAdmin, User")]
    public async Task<IActionResult> CreateQuestion([FromBody] QuestionExtendedDto? newQuestion)
    {
        try
        {
            if (newQuestion == null) return BadRequest("Question data cannot be null.");

            await questionService.CreateQuestion(newQuestion, User);
            return Created();
        }
        catch (Exception ex)
        {
            string message = "An error occurred while saving the question.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin, SuperAdmin")]
    public async Task<IActionResult> UpdateQuestion(int id, [FromBody] QuestionExtendedDto? updatedQuestion)
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
    [Authorize(Roles = "Admin, SuperAdmin, User")]
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