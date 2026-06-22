using MediatR;
using SmartAssistant.Application.Common.Interfaces;
using SmartAssistant.Domain.Entities;
using SmartAssistant.Domain.Interfaces;

namespace SmartAssistant.Application.Chat.Commands.AskQuestion;

public class AskQuestionHandler : IRequestHandler<AskQuestionCommand, AskQuestionResponse>
{
    private readonly IVectorStore _vectorStore;
    private readonly IAIService _aiService;
    private readonly IChatSessionRepository _chatSessionRepository;

    public AskQuestionHandler(IVectorStore vectorStore, IAIService aiService, IChatSessionRepository chatSessionRepository)
    {
        _vectorStore = vectorStore;
        _aiService = aiService;
        _chatSessionRepository = chatSessionRepository;
    }

    public async Task<AskQuestionResponse> Handle(AskQuestionCommand request, CancellationToken cancellationToken)
    {
        var similarChunks = await _vectorStore.SearchSimilarChunksAsync(request.Question, 3, cancellationToken);
        var answer = await _aiService.GenerateAnswerAsync(request.Question, similarChunks, cancellationToken);
        
        var chatSession = ChatSession.Create(request.DocumentId, request.Question, answer);
        await _chatSessionRepository.AddAsync(chatSession, cancellationToken);

        return new AskQuestionResponse(answer, similarChunks, chatSession.Id);
    }
}