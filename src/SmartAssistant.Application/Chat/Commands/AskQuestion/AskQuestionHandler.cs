using MediatR;
using SmartAssistant.Application.Common.Interfaces;
using SmartAssistant.Domain.Entities;
using SmartAssistant.Domain.Interfaces;

namespace SmartAssistant.Application.Chat.Commands.AskQuestion;

public class AskQuestionHandler : IRequestHandler<AskQuestionCommand, AskQuestionResponse>
{
    private readonly IVectorStore _vectorStore;
    private readonly IAIService _aiService;
    private readonly IDocumentRepository _documentRepository;

    public AskQuestionHandler(IVectorStore vectorStore, IAIService aiService, IDocumentRepository documentRepository)
    {
        _vectorStore = vectorStore;
        _aiService = aiService;
        _documentRepository = documentRepository;
    }

    public async Task<AskQuestionResponse> Handle(AskQuestionCommand request, CancellationToken cancellationToken)
    {
        // 1. Search Qdrant for top 3 similar chunks
        var similarChunks = await _vectorStore.SearchSimilarChunksAsync(request.Question, 3, cancellationToken);

        // 2. Generate Answer via Semantic Kernel (OpenAI/Ollama)
        var answer = await _aiService.GenerateAnswerAsync(request.Question, similarChunks, cancellationToken);

        // 3. Save Chat Session
        var chatSession = ChatSession.Create(request.DocumentId, request.Question, answer);
        await _documentRepository.AddChatSessionAsync(chatSession, cancellationToken);

        return new AskQuestionResponse(answer, similarChunks, chatSession.Id);
    }
}