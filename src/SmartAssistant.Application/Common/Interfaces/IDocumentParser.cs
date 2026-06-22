namespace SmartAssistant.Application.Common.Interfaces;

public interface IDocumentParser
{
    Task<string> ParseAsync(Stream fileStream, CancellationToken cancellationToken = default);
}