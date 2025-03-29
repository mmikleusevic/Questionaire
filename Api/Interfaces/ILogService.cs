using Shared.Models;

namespace QuestionaireApi.Interfaces;

public interface ILogService
{
    void LogClientEntry(LogEntryDto logEntry);
}