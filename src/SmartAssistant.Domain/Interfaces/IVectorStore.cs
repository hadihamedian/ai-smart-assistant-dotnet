namespace SmartAssistant.Domain.Interfaces;

public interface IVectorStore
{
    Task UpsertChunksAsync(Guid documentId, IEnumerable<string> chunks, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> SearchSimilarChunksAsync(string query, int topK = 3, CancellationToken cancellationToken = default);
}