using TestPlatform.Core.Exams;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Attempts;

public interface IAttemptSourcesReadDbContext
{
    IQueryable<Test> ReadTests { get; }
    IQueryable<Exam> ReadExams { get; }
}
