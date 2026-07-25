using CSharpFunctionalExtensions;
using TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Request;
using TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Response;
using TestPlatform.Core.Attempts;

namespace TestPlatform.Application.Attempts.Extensions;

public static class AttemptAnswerMappingExtensions
{
    public static Result<AttemptAnswer> ToDomain(this AttemptAnswerRequest request)
    {
        return request switch
        {
            ChoiceAttemptAnswerRequest choice =>
                AttemptAnswer.CreateChoice(
                    choice.QuestionId,
                    choice.SelectedOptionIds),

            TextAttemptAnswerRequest text =>
                AttemptAnswer.CreateText(
                    text.QuestionId,
                    text.TextAnswer),

            NumberAttemptAnswerRequest number =>
                AttemptAnswer.CreateNumber(
                    number.QuestionId,
                    number.NumberAnswer),

            MatchingAttemptAnswerRequest matching =>
                AttemptAnswer.CreateMatching(
                    matching.QuestionId,
                    matching.MatchingPairs.Select(x =>
                        new AttemptMatchingPair(
                            x.LeftOptionId,
                            x.RightOptionId))),

            _ => Result.Failure<AttemptAnswer>(
                "Unknown attempt answer type.")
        };
    }

    public static AttemptAnswerResponse ToResponse(this AttemptAnswer answer)
    {
        if (answer.SelectedOptionIds.Count > 0)
        {
            return new ChoiceAttemptAnswerResponse(
                answer.QuestionId,
                answer.SelectedOptionIds);
        }

        if (answer.TextAnswer is not null)
        {
            return new TextAttemptAnswerResponse(
                answer.QuestionId,
                answer.TextAnswer);
        }

        if (answer.NumberAnswer.HasValue)
        {
            return new NumberAttemptAnswerResponse(
                answer.QuestionId,
                answer.NumberAnswer.Value);
        }

        return new MatchingAttemptAnswerResponse(
            answer.QuestionId,
            answer.MatchingPairs
                .Select(x => new AttemptMatchingPairResponse(
                    x.LeftOptionId,
                    x.RightOptionId))
                .ToList());
    }
}
