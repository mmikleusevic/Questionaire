using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QuestionController(IQuestionService questionService)  : ControllerBase
{
        [HttpGet]
        public async Task<ActionResult<List<Question>>> GetQuestions()
        {
                return Ok(await questionService.GetQuestionsAsync());
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<ActionResult<Question>> GetQuestionById(int id)
        {
                Question question = await questionService.GetQuestionByIdAsync(id);
                if (question is null) return NotFound();
                return Ok(question);
        }
        
        [HttpGet]
        [Route("{userId}/{numberOfQuestions:int}")]
        public async Task<ActionResult<Question>> GetRandomUniqueQuestions(string userId, int numberOfQuestions)
        {
                List<Question> questions = await questionService.GetRandomUniqueQuestions(userId, numberOfQuestions);
                if (questions.Count == 0) return NotFound();
                return Ok(questions);
        }

        [HttpPost]
        public async Task<ActionResult<Question>> AddQuestion(Question? newQuestion)
        {
                if (newQuestion is null) return BadRequest();

                Question question = await questionService.AddQuestionAsync(newQuestion);
                return CreatedAtAction(nameof(GetQuestionById), new { id = question.Id }, question);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateQuestion(int id, Question? updatedQuestion)
        {
                if (updatedQuestion is null) return BadRequest();
        
                bool success = await questionService.UpdateQuestionAsync(id, updatedQuestion);
                if (!success) return NotFound();
                return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
                bool success = await questionService.DeleteQuestionAsync(id);
                if (!success) return NotFound();
                return NoContent();
        }
}