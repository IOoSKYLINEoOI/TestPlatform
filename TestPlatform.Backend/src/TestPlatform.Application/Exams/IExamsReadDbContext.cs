using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams;

public interface IExamsReadDbContext
{
    IQueryable<Exam> ReadExams { get; }
}
