using Microsoft.AspNetCore.Mvc;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Services;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AnswerController(IAnswerService answerService,
    ILogger<AnswerController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Answer>>> GetAnswers()
    {
        try
        {
            List<Answer> answers = await answerService.GetAnswersAsync();
            if (answers.Count == 0) return NotFound("No answers found!");
            return Ok(answers);
        }
        catch (Exception ex)
        {
            string message = "An unexpected error occurred while retrieving the answers.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Answer>> GetAnswerById(int id)
    {
        try
        {
            Answer? answer = await answerService.GetAnswerByIdAsync(id);
            if (answer == null) return NotFound($"Answer with ID {id} not found.");

            return Ok(answer);
        }
        catch (Exception ex)
        {
            string message = $"An unexpected error occurred while retrieving the answer with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<Answer>> CreateAnswer(Answer? newAnswer)
    {
        if (newAnswer == null) return BadRequest("Answer data cannot be null.");

        try
        {
            await answerService.CreateAnswerAsync(newAnswer);
            return CreatedAtAction(nameof(GetAnswerById), new { id = newAnswer.Id }, newAnswer);
        }
        catch (Exception ex)
        {
            string message = "An unexpected error occurred while creating the answer.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAnswer(int id, Answer? updatedAnswer)
    {
        if (updatedAnswer == null) return BadRequest("Answer data cannot be null.");

        try
        {
            bool success = await answerService.UpdateAnswerAsync(id, updatedAnswer);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            string message = $"An unexpected error occurred while updating the answer with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAnswer(int id)
    {
        try
        {
            bool success = await answerService.DeleteAnswerAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            string message = $"An unexpected error occurred while deleting the answer with ID {id}.";
            logger.LogError(ex, message);
            return StatusCode(500, message);
        }
    }
}
