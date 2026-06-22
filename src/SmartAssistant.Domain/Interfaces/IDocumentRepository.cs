using SmartAssistant.Domain.Entities;

namespace SmartAssistant.Domain.Interfaces;

public interface IDocumentRepository
{
    Task AddAsync(Document document, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Document>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddChatSessionAsync(ChatSession session, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatSession>> GetChatHistoryAsync(Guid documentId, CancellationToken cancellationToken = default);
}