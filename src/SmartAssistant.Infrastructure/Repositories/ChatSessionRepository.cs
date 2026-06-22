using System.Collections.Concurrent;
using SmartAssistant.Domain.Entities;
using SmartAssistant.Domain.Interfaces;

namespace SmartAssistant.Infrastructure.Repositories;

public class ChatSessionRepository : IChatSessionRepository
{
    private readonly ConcurrentDictionary<Guid, ChatSession> _chatSessions = new();

    public Task AddAsync(ChatSession session, CancellationToken cancellationToken = default)
    {
        _chatSessions.TryAdd(session.Id, session);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ChatSession>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var history = _chatSessions.Values.Where(c => c.DocumentId == documentId).OrderBy(c => c.CreatedAt);
        return Task.FromResult(history.AsEnumerable());
    }
}