using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Contracts.Exams.DTOs;
using TestPlatform.Contracts.Questions.DTOs;

namespace TestPlatform.Application.Attempts.SourceService;

public class ExamAttemptSource : IAttemptSource
{
    private readonly ExamFullResponse _exam;

    public ExamAttemptSource(ExamFullResponse exam)
    {
        _exam = exam;
    }

    public Guid Id => _exam.Id;

    public int? TimeLimitSeconds => _exam.TimeLimitSeconds;

    public IReadOnlyCollection<QuestionResponse> Questions => _exam.Questions;
}