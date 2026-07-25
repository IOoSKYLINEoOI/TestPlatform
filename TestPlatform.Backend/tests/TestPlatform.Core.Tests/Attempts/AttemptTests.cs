using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;
using TestPlatform.Core.Questions;
using TestPlatform.Core.Questions.AnswerDefinition;
using TestPlatform.Core.Questions.Enums;
using Xunit;

namespace TestPlatform.Core.Tests.Attempts;

public class AttemptTests
{
    [Fact]
    public void Create_CalculatesTotalsFromSelectedQuestions()
    {
        var selections = new[]
        {
            new AttemptQuestionSelection(Guid.NewGuid(), 1, 2),
            new AttemptQuestionSelection(Guid.NewGuid(), 2, 2),
            new AttemptQuestionSelection(Guid.NewGuid(), 3, 2),
        };

        var result = Attempt.Create(
            Guid.NewGuid(),
            AttemptType.Exam,
            Guid.NewGuid(),
            selections,
            null,
            minPassingScore: 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalQuestions);
        Assert.Equal(6, result.Value.TotalMaxScore);
    }

    [Fact]
    public void SaveAnswer_RejectsQuestionOutsideAttemptSnapshot()
    {
        var selectedQuestionId = Guid.NewGuid();
        var attempt = Attempt.Create(
            Guid.NewGuid(),
            AttemptType.Test,
            Guid.NewGuid(),
            [new AttemptQuestionSelection(selectedQuestionId, 1, 1)],
            null).Value;
        attempt.Start();

        var answer = AttemptAnswer.CreateText(Guid.NewGuid(), "Ответ").Value;
        var result = attempt.SaveAnswer(answer);

        Assert.True(result.IsFailure);
        Assert.Equal("attempt.question_not_in_attempt", result.Error);
    }

    [Fact]
    public void Abandon_MakesAttemptFinal()
    {
        var attempt = Attempt.Create(
            Guid.NewGuid(),
            AttemptType.Exam,
            Guid.NewGuid(),
            [new AttemptQuestionSelection(Guid.NewGuid(), 1, 1)],
            null,
            minPassingScore: 1).Value;
        attempt.Start();

        var abandonResult = attempt.Abandon();
        var secondAbandonResult = attempt.Abandon();

        Assert.True(abandonResult.IsSuccess);
        Assert.True(secondAbandonResult.IsFailure);
        Assert.Equal(AttemptStatus.ABANDONED, attempt.Status);
        Assert.NotNull(attempt.FinishedAt);
    }

    [Fact]
    public void Finish_CorrectTextAnswer_EarnsQuestionScore()
    {
        var question = Question.Create(
            QuestionContent.Create("Столица Франции", null).Value,
            TextAnswerDefinition.Create("Париж").Value,
            Guid.NewGuid()).Value;
        var attempt = Attempt.Create(
            Guid.NewGuid(),
            AttemptType.Exam,
            Guid.NewGuid(),
            [new AttemptQuestionSelection(question.Id, 1, 3)],
            null,
            minPassingScore: 3).Value;
        attempt.Start();
        attempt.SaveAnswer(AttemptAnswer.CreateText(question.Id, "париж").Value);

        var result = attempt.Finish([new AttemptQuestion(1, 3, question)]);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, attempt.AttemptResult!.EarnedPoints);
        Assert.Equal(1, attempt.AttemptResult.CorrectAnswers);
        Assert.True(attempt.AttemptResult.Passed);
    }

    [Fact]
    public void Finish_WrongTextAnswer_EarnsNothing()
    {
        var question = Question.Create(
            QuestionContent.Create("Столица Франции", null).Value,
            TextAnswerDefinition.Create("Париж").Value,
            Guid.NewGuid()).Value;
        var attempt = Attempt.Create(
            Guid.NewGuid(),
            AttemptType.Test,
            Guid.NewGuid(),
            [new AttemptQuestionSelection(question.Id, 1, 1)],
            null).Value;
        attempt.Start();
        attempt.SaveAnswer(AttemptAnswer.CreateText(question.Id, "Лондон").Value);

        attempt.Finish([new AttemptQuestion(1, 1, question)]);

        Assert.Equal(0, attempt.AttemptResult!.EarnedPoints);
        Assert.Equal(0, attempt.AttemptResult.CorrectAnswers);
    }

    [Fact]
    public void Finish_CorrectChoiceAnswer_EarnsQuestionScore()
    {
        var correct = AnswerOption.Create("Correct", true, null).Value;
        var wrong = AnswerOption.Create("Wrong", false, null).Value;
        var question = CreateQuestion(ChoiceAnswerDefinition.Create(
            ChoiceMode.Single,
            EvaluationMode.Strict,
            [correct, wrong]).Value);
        var attempt = CreateExamAttempt(question, 4);
        attempt.SaveAnswer(AttemptAnswer.CreateChoice(question.Id, [correct.Id]).Value);

        attempt.Finish([new AttemptQuestion(1, 4, question)]);

        Assert.Equal(4, attempt.AttemptResult!.EarnedPoints);
        Assert.Equal(1, attempt.AttemptResult.CorrectAnswers);
    }

    [Fact]
    public void Finish_CorrectNumberAnswer_EarnsQuestionScore()
    {
        var question = CreateQuestion(NumberAnswerDefinition.Create(42.125m).Value);
        var attempt = CreateExamAttempt(question, 2);
        attempt.SaveAnswer(AttemptAnswer.CreateNumber(question.Id, 42.125m).Value);

        attempt.Finish([new AttemptQuestion(1, 2, question)]);

        Assert.Equal(2, attempt.AttemptResult!.EarnedPoints);
    }

    [Fact]
    public void Finish_CorrectMatchingAnswer_EarnsQuestionScore()
    {
        var left1 = MatchingItem.Create("L1", null).Value;
        var left2 = MatchingItem.Create("L2", null).Value;
        var right1 = MatchingItem.Create("R1", null).Value;
        var right2 = MatchingItem.Create("R2", null).Value;
        var definition = MatchingAnswerDefinition.Create(
            EvaluationMode.Strict,
            [left1, left2],
            [right1, right2],
            [new MatchingPair(left1.Id, right2.Id), new MatchingPair(left2.Id, right1.Id)]).Value;
        var question = CreateQuestion(definition);
        var attempt = CreateExamAttempt(question, 5);
        attempt.SaveAnswer(AttemptAnswer.CreateMatching(
            question.Id,
            [new AttemptMatchingPair(left1.Id, right2.Id), new AttemptMatchingPair(left2.Id, right1.Id)]).Value);

        attempt.Finish([new AttemptQuestion(1, 5, question)]);

        Assert.Equal(5, attempt.AttemptResult!.EarnedPoints);
    }

    [Fact]
    public void Start_RejectsAttemptAfterSourceClosingTime()
    {
        var result = Attempt.Create(
            Guid.NewGuid(),
            AttemptType.Exam,
            Guid.NewGuid(),
            [new AttemptQuestionSelection(Guid.NewGuid(), 1, 1)],
            null,
            minPassingScore: 1,
            latestFinishAt: DateTime.UtcNow.AddMinutes(-1)).Value.Start();

        Assert.True(result.IsFailure);
        Assert.Equal("attempt.source_closed", result.Error);
    }

    private static Question CreateQuestion(
        TestPlatform.Core.Questions.AnswerDefinition.Abstractions.QuestionAnswerDefinition definition) =>
        Question.Create(
            QuestionContent.Create("Question", null).Value,
            definition,
            Guid.NewGuid()).Value;

    private static Attempt CreateExamAttempt(Question question, decimal score)
    {
        var attempt = Attempt.Create(
            Guid.NewGuid(),
            AttemptType.Exam,
            Guid.NewGuid(),
            [new AttemptQuestionSelection(question.Id, 1, score)],
            null,
            minPassingScore: score).Value;
        attempt.Start();
        return attempt;
    }
}
