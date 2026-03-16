using CSharpFunctionalExtensions;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Application.Attempts.SourceService;
using TestPlatform.Application.Exams;
using TestPlatform.Application.Tests;
using TestPlatform.Contracts.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Features.FinishAttemptCommand;

public class AttemptSourceService : IAttemptSourceService
{
    private readonly ITestsReadRepository _testsReadRepository;
    private readonly IExamsReadRepository _examsReadRepository;

    public AttemptSourceService(
        ITestsReadRepository testsReadRepository,
        IExamsReadRepository examsReadRepository)
    {
        _testsReadRepository = testsReadRepository;
        _examsReadRepository = examsReadRepository;
    }

    public async Task<Result<IAttemptSource>> GetSourceAsync(
        AttemptTypeDto type,
        Guid sourceId,
        CancellationToken cancellationToken)
    {
        if (type == AttemptTypeDto.TEST)
        {
            var test = await _testsReadRepository.ReadTestByIdAsync(sourceId, true, cancellationToken);

            if (test is null)
                return Result.Failure<IAttemptSource>("Тест не найден");

            var source = new TestAttemptSource(test);

            return Result.Success<IAttemptSource>(source);
        }

        if (type == AttemptTypeDto.EXAM)
        {
            var exam = await _examsReadRepository.ReadExamByIdAsync(sourceId, true, cancellationToken);

            if (exam is null)
                return Result.Failure<IAttemptSource>("Экзамен не найден");

            var source = new ExamAttemptSource(exam);

            return Result.Success<IAttemptSource>(source);
        }

        return Result.Failure<IAttemptSource>("Неизвестный тип попытки");
    }
}