using System.Collections.Concurrent;
using SmartAssistant.Domain.Entities;
using SmartAssistant.Domain.Interfaces;

namespace SmartAssistant.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ConcurrentDictionary<Guid, Document> _documents = new();
    private readonly ConcurrentDictionary<Guid, ChatSession> _chatSessions = new();

    public Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        _documents.TryAdd(document.Id, document);
        return Task.CompletedTask;
    }

    public Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _documents.TryGetValue(id, out var document);
        return Task.FromResult(document);
    }

    public Task<IEnumerable<Document>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_documents.Values.AsEnumerable());
    }

    public Task AddChatSessionAsync(ChatSession session, CancellationToken cancellationToken = default)
    {
        _chatSessions.TryAdd(session.Id, session);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ChatSession>> GetChatHistoryAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var history = _chatSessions.Values.Where(c => c.DocumentId == documentId).OrderBy(c => c.CreatedAt);
        return Task.FromResult(history.AsEnumerable());
    }
}