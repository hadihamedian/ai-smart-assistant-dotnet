using MediatR;
using SmartAssistant.Application.Common.Interfaces;
using SmartAssistant.Domain.Interfaces;

namespace SmartAssistant.Application.Documents.Commands.UploadDocument;

public class UploadDocumentHandler : IRequestHandler<UploadDocumentCommand, Guid>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IVectorStore _vectorStore;
    private readonly IDocumentParser _documentParser;

    public UploadDocumentHandler(IDocumentRepository documentRepository, IVectorStore vectorStore, IDocumentParser documentParser)
    {
        _documentRepository = documentRepository;
        _vectorStore = vectorStore;
        _documentParser = documentParser;
    }

    public async Task<Guid> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var text = await _documentParser.ParseAsync(request.FileStream, cancellationToken);
        var chunks = ChunkText(text, 2000, 200);
        var document = Domain.Entities.Document.Create(request.FileName, text, chunks.Count);

        await _documentRepository.AddAsync(document, cancellationToken);
        await _vectorStore.UpsertChunksAsync(document.Id, chunks, cancellationToken);

        return document.Id;
    }

    private List<string> ChunkText(string text, int chunkSize, int overlap)
    {
        var chunks = new List<string>();
        for (int i = 0; i < text.Length; i += chunkSize - overlap)
        {
            chunks.Add(text.Substring(i, Math.Min(chunkSize, text.Length - i)));
        }
        return chunks;
    }
}