using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Exams;

public class ExamSchedule
{
    public DateTime? AvailableFrom { get; private set; }

    public DateTime? AvailableTo { get; private set; }

    private ExamSchedule() { }

    private ExamSchedule(DateTime? from, DateTime? to)
    {
        AvailableFrom = from;
        AvailableTo = to;
    }

    public static Result<ExamSchedule> Create(DateTime? from, DateTime? to)
    {
        if (from is null && to is null)
        {
            return Result.Failure<ExamSchedule>("exam.schedule.boundary_required");
        }

        if (from > to)
        {
            return Result.Failure<ExamSchedule>("exam.schedule.invalid_range");
        }

        return Result.Success(new ExamSchedule(from, to));
    }
}
