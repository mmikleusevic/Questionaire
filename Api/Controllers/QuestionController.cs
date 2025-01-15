using Microsoft.AspNetCore.Mvc;
using QuestionaireApi.Models;

namespace QuestionaireApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QuestionController : ControllerBase
{
        [HttpGet]
        public ActionResult<List<Question>> GetQuestions()
        {
                //TODO: fix
                return Ok(new List<Question>());
        }

        [HttpGet]
        [Route("{id:int}")]
        public ActionResult<Question> GetQuestionById(int id)
        {
                return Ok(new Question());
        }

        [HttpPost]
        public ActionResult<Question> AddQuestion(Question? newQuestion)
        {
                if (newQuestion is null) return BadRequest();
                
                return CreatedAtAction(nameof(GetQuestionById), new { id = newQuestion.Id }, newQuestion);
        }

        [HttpPut("{id:int}")]
        public IActionResult UpdateQuestion(int id, Question? updatedQuestion)
        {
                return NoContent();
        }

        [HttpDelete("{id:int}")]
        public IActionResult DeleteQuestion(int id)
        {
                return NoContent();
        }
}