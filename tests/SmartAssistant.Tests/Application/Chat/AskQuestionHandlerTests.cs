using FluentAssertions;
using Moq;
using SmartAssistant.Application.Chat.Commands.AskQuestion;
using SmartAssistant.Application.Common.Interfaces;
using SmartAssistant.Domain.Entities;
using SmartAssistant.Domain.Interfaces;

namespace SmartAssistant.Tests.Application.Chat;

public class AskQuestionHandlerTests
{
    private readonly Mock<IVectorStore> _vectorStoreMock = new();
    private readonly Mock<IAIService> _aiServiceMock = new();
    private readonly Mock<IChatSessionRepository> _chatSessionRepositoryMock = new();

    [Fact]
    public async Task Handle_Should_Return_Answer_And_Save_Session()
    {
        var handler = new AskQuestionHandler(_vectorStoreMock.Object, _aiServiceMock.Object, _chatSessionRepositoryMock.Object);
        var documentId = Guid.NewGuid();
        var command = new AskQuestionCommand(documentId, "What is this?");
        var expectedAnswer = "This is a test document.";
        var expectedChunks = new List<string> { "chunk1", "chunk2" };

        _vectorStoreMock.Setup(v => v.SearchSimilarChunksAsync(command.Question, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedChunks);
            
        _aiServiceMock.Setup(a => a.GenerateAnswerAsync(command.Question, expectedChunks, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAnswer);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Answer.Should().Be(expectedAnswer);
        result.Sources.Should().BeEquivalentTo(expectedChunks);
        result.SessionId.Should().NotBeEmpty();
        
        _chatSessionRepositoryMock.Verify(r => r.AddAsync(It.Is<ChatSession>(s => 
            s.Id == result.SessionId && 
            s.DocumentId == documentId && 
            s.Answer == expectedAnswer), It.IsAny<CancellationToken>()), Times.Once);
    }
}