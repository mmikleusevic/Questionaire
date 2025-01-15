using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Models;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QuestionController(QuestionaireDbContext context) : ControllerBase
{
        [HttpGet]
        public async Task<ActionResult<List<Question>>> GetQuestions()
        {
                return Ok(await context.Questions.ToListAsync());
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<ActionResult<Question>> GetQuestionById(int id)
        {
                Question question = await context.Questions.FindAsync(id);

                if (question is null) return NotFound();
                        
                return Ok(question);
        }

        [HttpPost]
        public async Task<ActionResult<Question>> AddQuestion(Question? newQuestion)
        {
                if (newQuestion is null) return BadRequest();
                
                context.Questions.Add(newQuestion);
                await context.SaveChangesAsync();
                
                return CreatedAtAction(nameof(GetQuestionById), new { id = newQuestion.Id }, newQuestion);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateQuestion(int id, Question? updatedQuestion)
        {
                Question question = await context.Questions.FindAsync(id);
                if (question is null) return NotFound();
                
                updatedQuestion.QuestionText = question.QuestionText;
                
                await context.SaveChangesAsync();
                
                return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
                Question question = await context.Questions.FindAsync(id);
                if (question is null) return NotFound();
                
                context.Questions.Remove(question);
                await context.SaveChangesAsync();
                
                return NoContent();
        }
}