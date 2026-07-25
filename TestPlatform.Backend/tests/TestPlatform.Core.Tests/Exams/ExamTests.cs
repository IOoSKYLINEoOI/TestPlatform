using TestPlatform.Core.Exams;
using TestPlatform.Core.Exams.Enums;
using Xunit;

namespace TestPlatform.Core.Tests.Exams;

public class ExamTests
{
    [Fact]
    public void TotalMaxScore_DependsOnSelectionRule_NotPoolSize()
    {
        var exam = CreateExam();
        var sectionId = exam.AddSection("Основы", 3, 2).Value;

        for (var i = 0; i < 10; i++)
        {
            exam.AddQuestionToSection(sectionId, Guid.NewGuid());
        }

        Assert.Equal(3, exam.TotalQuestions);
        Assert.Equal(6, exam.TotalMaxScore);
    }

    [Fact]
    public void Exam_CannotBePublished_WhenSectionPoolIsTooSmall()
    {
        var exam = CreateExam();
        exam.ChangePassingRule(ExamPassingRule.Create(2, null).Value);
        exam.AddSection("Основы", 3, 1);

        var result = exam.Publish();

        Assert.True(result.IsFailure);
        Assert.Equal("exam.section.insufficient_pool", result.Error);
    }

    [Fact]
    public void PublishedExam_CannotBeEdited()
    {
        var exam = CreatePublishableExam();
        exam.Publish();

        var result = exam.ChangeAttemptsLimit(2);

        Assert.True(result.IsFailure);
        Assert.Equal("exam.not_editable", result.Error);
        Assert.Equal(ExamStatus.Published, exam.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public void AttemptsLimit_MustBeInAllowedRange(int limit)
    {
        var exam = CreateExam();

        var result = exam.ChangeAttemptsLimit(limit);

        Assert.True(result.IsFailure);
        Assert.Equal(1, exam.AttemptsLimit);
    }

    [Fact]
    public void PassingRule_RequiresExactlyOnePositiveThreshold()
    {
        Assert.True(ExamPassingRule.Create(null, null).IsFailure);
        Assert.True(ExamPassingRule.Create(1, 50).IsFailure);
        Assert.True(ExamPassingRule.Create(0, null).IsFailure);
        Assert.True(ExamPassingRule.Create(null, 0).IsFailure);
        Assert.True(ExamPassingRule.Create(1, null).IsSuccess);
        Assert.True(ExamPassingRule.Create(null, 50).IsSuccess);
    }

    [Fact]
    public void AfterExamClosedReview_RequiresScheduleEndDate()
    {
        var exam = CreatePublishableExam();
        exam.ChangeReviewPolicy(ExamReviewPolicy.AfterExamClosed);

        var result = exam.Publish();

        Assert.True(result.IsFailure);
        Assert.Equal("exam.review_requires_end_date", result.Error);
    }

    [Fact]
    public void DraftSection_CanBeUpdated()
    {
        var exam = CreateExam();
        var sectionId = exam.AddSection("Old", 2, 1).Value;

        var result = exam.UpdateSection(sectionId, "New", 3, 4);

        Assert.True(result.IsSuccess);
        Assert.Equal("New", exam.Sections.Single().Name);
        Assert.Equal(12, exam.TotalMaxScore);
    }

    private static Exam CreatePublishableExam()
    {
        var exam = CreateExam();
        var sectionId = exam.AddSection("Основы", 3, 2).Value;
        exam.AddQuestionToSection(sectionId, Guid.NewGuid());
        exam.AddQuestionToSection(sectionId, Guid.NewGuid());
        exam.AddQuestionToSection(sectionId, Guid.NewGuid());
        exam.ChangePassingRule(ExamPassingRule.Create(4, null).Value);
        exam.ChangeReviewPolicy(ExamReviewPolicy.Immediately);
        return exam;
    }

    private static Exam CreateExam() =>
        Exam.Create("Экзамен", "Описание", Guid.NewGuid()).Value;
}
