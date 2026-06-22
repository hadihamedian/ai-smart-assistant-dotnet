namespace SmartAssistant.Domain.Entities;

public class Document
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }
    public int ChunkCount { get; private set; }

    private Document() { }

    public static Document Create(string fileName, string content, int chunkCount = 0)
    {
        return new Document
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            Content = content,
            UploadedAt = DateTime.UtcNow,
            ChunkCount = chunkCount
        };
    }
}