using FluentAssertions;
using Document = SmartAssistant.Domain.Entities.Document;

namespace SmartAssistant.Tests.Domain.Entities;

public class DocumentTests
{
    [Fact]
    public void Create_Should_Set_Properties_Correctly()
    {
        var fileName = "test.pdf";
        var content = "test content";
        var chunkCount = 5;

        var document = Document.Create(fileName, content, chunkCount);

        document.Id.Should().NotBeEmpty();
        document.FileName.Should().Be(fileName);
        document.Content.Should().Be(content);
        document.ChunkCount.Should().Be(chunkCount);
        document.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}