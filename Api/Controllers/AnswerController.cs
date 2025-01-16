using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Models;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AnswerController(QuestionaireDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Answer>>> GetAnswers()
    {
        return Ok(await context.Answers.ToListAsync());
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<Answer>> GetAnswerById(int id)
    {
        Answer answer = await context.Answers.FindAsync(id);

        if (answer is null) return NotFound();
                        
        return Ok(answer);
    }

    [HttpPost]
    public async Task<ActionResult<Answer>> AddAnswer(Answer? newAnswer)
    {
        if (newAnswer is null) return BadRequest();
                
        context.Answers.Add(newAnswer);
        await context.SaveChangesAsync();
                
        return CreatedAtAction(nameof(GetAnswerById), new { id = newAnswer.Id }, newAnswer);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAnswer(int id, Answer? updatedAnswer)
    {
        Answer answer = await context.Answers.FindAsync(id);
        if (answer is null) return NotFound();
                
        answer.AnswerText = updatedAnswer.AnswerText;
                
        await context.SaveChangesAsync();
                
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAnswer(int id)
    {
        Answer answer = await context.Answers.FindAsync(id);
        
        if (answer is null) return NotFound();
                
        context.Answers.Remove(answer);
        await context.SaveChangesAsync();
                
        return NoContent();
    }
}