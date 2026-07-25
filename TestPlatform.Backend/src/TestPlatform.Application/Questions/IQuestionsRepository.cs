using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Questions;

public interface IQuestionsRepository
{
    Task<Question?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Question question, CancellationToken cancellationToken);
}
