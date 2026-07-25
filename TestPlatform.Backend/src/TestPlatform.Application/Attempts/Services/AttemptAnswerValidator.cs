using CSharpFunctionalExtensions;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Questions;
using TestPlatform.Core.Questions.AnswerDefinition;
using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Application.Attempts.Services;

public static class AttemptAnswerValidator
{
    public static Result Validate(Question question, AttemptAnswer answer)
    {
        return question.AnswerDefinition switch
        {
            ChoiceAnswerDefinition definition => ValidateChoice(definition, answer),
            TextAnswerDefinition => answer.TextAnswer is not null
                ? Result.Success()
                : Result.Failure("attempt.answer.type_mismatch"),
            NumberAnswerDefinition => answer.NumberAnswer.HasValue
                ? Result.Success()
                : Result.Failure("attempt.answer.type_mismatch"),
            MatchingAnswerDefinition definition => ValidateMatching(definition, answer),
            _ => Result.Failure("attempt.answer.unsupported_type"),
        };
    }

    private static Result ValidateChoice(ChoiceAnswerDefinition definition, AttemptAnswer answer)
    {
        if (answer.SelectedOptionIds.Count == 0)
        {
            return Result.Failure("attempt.answer.type_mismatch");
        }

        var optionIds = definition.Options.Select(x => x.Id).ToHashSet();
        if (answer.SelectedOptionIds.Any(id => !optionIds.Contains(id)))
        {
            return Result.Failure("attempt.answer.option_not_found");
        }

        if (definition.Mode == ChoiceMode.Single && answer.SelectedOptionIds.Count != 1)
        {
            return Result.Failure("attempt.answer.single_choice_requires_one_option");
        }

        return Result.Success();
    }

    private static Result ValidateMatching(MatchingAnswerDefinition definition, AttemptAnswer answer)
    {
        if (answer.MatchingPairs.Count == 0)
        {
            return Result.Failure("attempt.answer.type_mismatch");
        }

        var leftIds = definition.LeftItems.Select(x => x.Id).ToHashSet();
        var rightIds = definition.RightItems.Select(x => x.Id).ToHashSet();
        return answer.MatchingPairs.Any(x =>
            !leftIds.Contains(x.LeftOptionId) || !rightIds.Contains(x.RightOptionId))
                ? Result.Failure("attempt.answer.matching_item_not_found")
                : Result.Success();
    }
}
