using TestPlatform.Application.Exams;
using TestPlatform.Contracts.Exams.DTOs;

namespace TestPlatform.Infrastructure.Postgres.Exams;

public class ExamsReadRepository : IExamsReadRepository
{
    public Task<ExamFullResponse?> ReadExamByIdAsync(Guid? id, bool includeCorrectAnswer, CancellationToken cancellationToken) => throw new NotImplementedException();
}