using System.Text;
using MediatR;
using SmartAssistant.Domain.Interfaces;
using UglyToad.PdfPig;

namespace SmartAssistant.Application.Documents.Commands.UploadDocument;

public class UploadDocumentHandler : IRequestHandler<UploadDocumentCommand, Guid>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IVectorStore _vectorStore;

    public UploadDocumentHandler(IDocumentRepository documentRepository, IVectorStore vectorStore)
    {
        _documentRepository = documentRepository;
        _vectorStore = vectorStore;
    }

    public async Task<Guid> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        // 1. Extract text from PDF
        var text = ExtractTextFromPdf(request.FileStream);

        // 2. Split into chunks (500 tokens, 50 overlap - simplified by character count for demo)
        var chunks = ChunkText(text, 2000, 200);

        // 3. Create Document Entity
        var document = Domain.Entities.Document.Create(request.FileName, text, chunks.Count);
        
        // 4. Store in DB
        await _documentRepository.AddAsync(document, cancellationToken);

        // 5. Store embeddings in Qdrant
        await _vectorStore.UpsertChunksAsync(document.Id, chunks, cancellationToken);

        return document.Id;
    }

    private string ExtractTextFromPdf(Stream pdfStream)
    {
        using var pdfDocument = PdfDocument.Open(pdfStream);
        var textBuilder = new StringBuilder();
        foreach (var page in pdfDocument.GetPages())
        {
            textBuilder.Append(page.Text);
        }
        return textBuilder.ToString();
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