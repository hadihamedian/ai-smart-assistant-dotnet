namespace SmartAssistant.Application.Common.Interfaces;

public interface IAIService
{
    Task<string> GenerateAnswerAsync(string question, IEnumerable<string> contextChunks, CancellationToken cancellationToken = default);
}