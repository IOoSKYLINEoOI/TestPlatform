using TestPlatform.Contracts.Questions.DTOs;

namespace TestPlatform.Application.Questions;

public interface IQuestionsReadRepository
{
    Task<QuestionResponse?> ReadQuestionByIdAsync(Guid id, bool includeCorrectAnswer, CancellationToken cancellationToken);

    Task<List<QuestionResponse>> ReadAllQuestionsByTagsAsync(IReadOnlyList<Guid> tagIds, bool includeCorrectAnswer, CancellationToken cancellationToken);
}