using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams;

public interface IExamsRepository
{
    Task AddAsync(Exam exam, CancellationToken cancellationToken);

    Task<Exam?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}