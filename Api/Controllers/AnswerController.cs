using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AnswerController(IAnswerService answerService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Answer>>> GetAnswers()
    {
        return Ok(await answerService.GetAnswersAsync());
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<Answer>> GetAnswerById(int id)
    {
        Answer answer = await answerService.GetAnswerByIdAsync(id);
        if (answer is null) return NotFound();
        return Ok(answer);
    }

    [HttpPost]
    public async Task<ActionResult<Answer>> AddAnswer(Answer? newAnswer)
    {
        if (newAnswer is null) return BadRequest();
        
        Answer answer = await answerService.AddAnswerAsync(newAnswer);
        return CreatedAtAction(nameof(GetAnswerById), new { id = answer.Id }, answer);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAnswer(int id, Answer? updatedAnswer)
    {
        if (updatedAnswer is null) return BadRequest();
        
        bool success = await answerService.UpdateAnswerAsync(id, updatedAnswer);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAnswer(int id)
    {
        bool success = await answerService.DeleteAnswerAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}