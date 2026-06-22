using SmartAssistant.Domain.Entities;

namespace SmartAssistant.Domain.Interfaces;

public interface IChatSessionRepository
{
    Task AddAsync(ChatSession session, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatSession>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
}