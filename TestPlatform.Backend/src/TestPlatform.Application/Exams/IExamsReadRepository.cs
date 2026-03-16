using TestPlatform.Contracts.Exams.DTOs;

namespace TestPlatform.Application.Exams;

public interface IExamsReadRepository
{
    Task<ExamFullResponse?> ReadExamByIdAsync(Guid? id, bool includeCorrectAnswer, CancellationToken cancellationToken);

    //Task<List<ExamResponse>> ReadAllTestAsync(CancellationToken cancellationToken);
}