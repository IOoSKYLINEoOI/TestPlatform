using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams;

public interface IExamsRepository
{
    Task<Exam?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Exam exam, CancellationToken cancellationToken);
}