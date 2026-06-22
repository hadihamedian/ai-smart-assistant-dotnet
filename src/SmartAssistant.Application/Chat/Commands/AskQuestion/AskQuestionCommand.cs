using MediatR;

namespace SmartAssistant.Application.Chat.Commands.AskQuestion;

public record AskQuestionResponse(string Answer, IEnumerable<string> Sources, Guid SessionId);

public record AskQuestionCommand(Guid DocumentId, string Question) : IRequest<AskQuestionResponse>;