using CSharpFunctionalExtensions;
using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Questions;

public interface IQuestionsRepository
{
    Task<Result<Guid>> AddAsync(Question question, CancellationToken cancellationToken);

    Task<Result> UpdateAsync(Question question, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid questionId, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid questionId, CancellationToken cancellationToken);
}