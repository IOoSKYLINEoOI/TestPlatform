using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Mappers;
using TestPlatform.Application.Common.Error;
using TestPlatform.Contracts.Attempts.DTOs;

namespace TestPlatform.Application.Attempts.Features.GetByIdAttemptQuery;

public record GetByIdAttemptQuery(Guid Id) : IQuery;

public class GetByIdAttemptHandler(IAttemptsReadDbContext attemptsDbContext) : IQueryHandler<GetByIdAttemptQuery, AttemptResponse>
{
    public async Task<Result<AttemptResponse>> Handle(GetByIdAttemptQuery query, CancellationToken cancellationToken)
    {
        var response = await attemptsDbContext.ReadAttempts
            .AsNoTracking()
            .Where(a => a.Id == query.Id)
            .Select(a => new AttemptResponse(
                a.Id,
                a.TotalQuestions,
                a.AttemptAnswers.Count,
                a.StartedAt,
                a.FinishedAt,
                a.Status.ToDto(),
                a.Type.ToDto()))
            .FirstOrDefaultAsync(cancellationToken);

        return response is null
            ? Result.Failure<AttemptResponse>(ErrorCodes.AttemptNotFound)
            : Result.Success(response);
    }
}