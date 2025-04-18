using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionaireApi.Interfaces;
using Shared.Models;

namespace QuestionaireApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class LogsController(
    ILogger<LogsController> logger,
    ILogService logService) : ControllerBase
{
    [HttpPost]
    public IActionResult PostLogEntry([FromBody] LogEntryDto? logEntry)
    {
        if (logEntry == null || string.IsNullOrWhiteSpace(logEntry.Message))
        {
            return BadRequest("Invalid log entry format.");
        }

        try
        {
            logService.LogClientEntry(logEntry);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while processing client log entry via LogService.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An error occurred while processing the log entry.");
        }
    }
}