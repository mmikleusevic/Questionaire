using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;
using QuestionaireApi.Services;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QuestionController(IQuestionService questionService,
    ILogger<QuestionController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<QuestionDto>>> GetQuestions(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 50)
    {
        try
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Page number and page size must be greater than 0.");
            
            PaginatedResponse<QuestionDto> response = await questionService.GetQuestionsAsync(pageNumber, pageSize);

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

    [HttpGet("{id:int}")]
    public async Task<ActionResult<QuestionDto>> GetQuestionById(int id)
    {
        try
        {
            Question? question = await questionService.GetQuestionByIdAsync(id);
            if (question is null) return NotFound($"Question with ID {id} not found.");
            return Ok(question);
        }
        catch (Exception ex)
        {
            string message = $"An error occurred while retrieving the question with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpPost("random")]
    public async Task<ActionResult<List<QuestionDto>>> GetRandomUniqueQuestions([FromBody] GetRandomUniqueQuestionsRequestDto? request)
    {
        if (request == null) return BadRequest("Get random unique questions data cannot be null.");

        try
        {
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

    [HttpPost]
    public async Task<IActionResult> CreateQuestion([FromBody] Question? newQuestion)
    {
        if (newQuestion == null) return BadRequest("Question data cannot be null.");

        try
        {
            await questionService.CreateQuestionAsync(newQuestion);
            return Created();
        }
        catch (Exception ex)
        {
            string message = "An error occurred while creating the question.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateQuestion(int id, [FromBody] QuestionDto? updatedQuestion)
    {
        if (updatedQuestion == null) return BadRequest("Updated question data cannot be null.");

        try
        {
            bool success = await questionService.UpdateQuestionAsync(id, updatedQuestion);
            if (!success) return NotFound($"Question with ID {id} not found.");
            return Ok($"Question with ID {id} updated successfully.");
        }
        catch (Exception ex)
        {
            string message =  $"An error occurred while updating the question with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        try
        {
            bool success = await questionService.DeleteQuestionAsync(id);
            if (!success) return NotFound($"Question with ID {id} not found.");
            return Ok($"Question with ID {id} deleted successfully.");
        }
        catch (Exception ex)
        {
            string message =  $"An error occurred while deleting the question with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }
}
