using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Exams;

public class ExamSchedule
{
    public DateTime? AvailableFrom { get; }

    public DateTime? AvailableTo { get; }

    private ExamSchedule() { }

    private ExamSchedule(DateTime? from, DateTime? to)
    {
        AvailableFrom = from;
        AvailableTo = to;
    }

    public static Result<ExamSchedule> Create(DateTime? from, DateTime? to)
    {
        if (from is null && to is null)
            return Result.Failure<ExamSchedule>("Нужно задать хотя бы одну границу периода");

        if (from > to)
            return Result.Failure<ExamSchedule>("Некорректный период доступности");

        return Result.Success(new ExamSchedule(from, to));
    }
}
