using FluentAssertions;
using Moq;
using SmartAssistant.Application.Common.Interfaces;
using SmartAssistant.Application.Documents.Commands.UploadDocument;
using SmartAssistant.Domain.Entities;
using SmartAssistant.Domain.Interfaces;

namespace SmartAssistant.Tests.Application.Documents;

public class UploadDocumentHandlerTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock = new();
    private readonly Mock<IVectorStore> _vectorStoreMock = new();
    private readonly Mock<IDocumentParser> _documentParserMock = new();

    [Fact]
    public async Task Handle_Should_Parse_Save_And_UpsertChunks()
    {
        var handler = new UploadDocumentHandler(_documentRepositoryMock.Object, _vectorStoreMock.Object, _documentParserMock.Object);
        var command = new UploadDocumentCommand("test.pdf", Stream.Null);
        var parsedText = new string('A', 2500); // برای تولید حداقل دو چانک
        
        _documentParserMock.Setup(p => p.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(parsedText);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        _documentRepositoryMock.Verify(r => r.AddAsync(It.Is<Document>(d => d.Id == result && d.FileName == "test.pdf"), It.IsAny<CancellationToken>()), Times.Once);
        _vectorStoreMock.Verify(v => v.UpsertChunksAsync(result, It.Is<IEnumerable<string>>(c => c.Any()), It.IsAny<CancellationToken>()), Times.Once);
    }
}