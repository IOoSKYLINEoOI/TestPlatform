using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Questions.Extensions;
using TestPlatform.Contracts.Questions.DTOs;

namespace TestPlatform.Application.Questions.Features.GetByIdQuestionQuery;

public record GetByIdQuestionQuery(Guid Id) : IQuery;

public class GetByIdQuestionHandler(IQuestionsReadDbContext questionsReadDbContext)
    : IQueryHandler<GetByIdQuestionQuery, QuestionResponse>
{
    public async Task<Result<QuestionResponse>> Handle(GetByIdQuestionQuery query, CancellationToken cancellationToken)
    {
        var question = await questionsReadDbContext.ReadQuestions
            .AsNoTracking()
            .Include(q => q.Tags)
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

        if (question is null)
            return Result.Failure<QuestionResponse>(ErrorCodes.QuestionNotFound);

        var response = question.ToResponse();

        return Result.Success(response);
    }
}