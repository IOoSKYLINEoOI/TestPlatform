using TestPlatform.Core.Questions;
using TestPlatform.Core.Questions.AnswerDefinition;
using TestPlatform.Core.Questions.Enums;
using Xunit;

namespace TestPlatform.Core.Tests.Questions;

public class QuestionTests
{
    [Fact]
    public void Draft_CanBeEdited()
    {
        var question = CreateTextQuestion();
        var content = QuestionContent.Create("Updated question", "Explanation").Value;
        var answer = TextAnswerDefinition.Create("Updated answer").Value;

        var result = question.UpdateContent(content, answer);

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated question", question.Text);
    }

    [Fact]
    public void Published_CannotBeEdited()
    {
        var question = CreateTextQuestion();
        question.Publish();
        var content = QuestionContent.Create("Updated question", null).Value;

        var result = question.UpdateContent(content, TextAnswerDefinition.Create("Answer").Value);

        Assert.True(result.IsFailure);
        Assert.Equal("question.not_editable", result.Error);
    }

    [Fact]
    public void Question_CannotBePublishedTwice()
    {
        var question = CreateTextQuestion();

        Assert.True(question.Publish().IsSuccess);
        Assert.True(question.Publish().IsFailure);
    }

    [Fact]
    public void PublishedChoiceQuestion_CloneHasIndependentOptionIds()
    {
        var options = new[]
        {
            AnswerOption.Create("Correct", true, null).Value,
            AnswerOption.Create("Wrong", false, null).Value,
        };
        var definition = ChoiceAnswerDefinition.Create(
            ChoiceMode.Single,
            EvaluationMode.Strict,
            options).Value;
        var question = Question.Create(
            QuestionContent.Create("Question", null).Value,
            definition,
            Guid.NewGuid()).Value;
        question.Publish();

        var clone = question.CloneAsDraft(Guid.NewGuid()).Value;
        var clonedDefinition = Assert.IsType<ChoiceAnswerDefinition>(clone.AnswerDefinition);

        Assert.Equal(QuestionStatus.Draft, clone.Status);
        Assert.NotEqual(question.Id, clone.Id);
        Assert.Empty(definition.Options.Select(option => option.Id).Intersect(
            clonedDefinition.Options.Select(option => option.Id)));
    }

    private static Question CreateTextQuestion()
    {
        return Question.Create(
            QuestionContent.Create("Question", null).Value,
            TextAnswerDefinition.Create("Answer").Value,
            Guid.NewGuid()).Value;
    }
}
