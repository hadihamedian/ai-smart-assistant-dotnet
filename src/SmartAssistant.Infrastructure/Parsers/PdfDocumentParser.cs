using System.Text;
using SmartAssistant.Application.Common.Interfaces;
using UglyToad.PdfPig;

namespace SmartAssistant.Infrastructure.Parsers;

public class PdfDocumentParser : IDocumentParser
{
    public Task<string> ParseAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        using var pdfDocument = PdfDocument.Open(fileStream);
        var textBuilder = new StringBuilder();
        
        foreach (var page in pdfDocument.GetPages())
        {
            textBuilder.Append(page.Text);
        }
        
        return Task.FromResult(textBuilder.ToString());
    }
}