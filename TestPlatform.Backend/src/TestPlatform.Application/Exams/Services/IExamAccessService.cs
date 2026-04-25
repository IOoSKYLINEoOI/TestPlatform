using CSharpFunctionalExtensions;
using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams.Services;

public interface IExamAccessService
{
    Task<Result<Exam>> GetForModifyAsync(Guid examId, CancellationToken ct);
}