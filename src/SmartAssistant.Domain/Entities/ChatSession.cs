namespace SmartAssistant.Domain.Entities;

public class ChatSession
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public string Question { get; private set; } = string.Empty;
    public string Answer { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private ChatSession() { }

    public static ChatSession Create(Guid documentId, string question, string answer)
    {
        return new ChatSession
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            Question = question,
            Answer = answer,
            CreatedAt = DateTime.UtcNow
        };
    }
}