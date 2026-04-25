using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Exams.Services;
using TestPlatform.Contracts.Exams.DTOs;
using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams.Features.UpdateExamPassingRuleCommand;

public record UpdateExamPassingRuleCommand(Guid Id, UpdateExamPassingRuleRequest Request) : ICommand;

public class UpdateExamPassingRuleHandler : ICommandHandler<UpdateExamPassingRuleCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExamAccessService _examAccessService;
    private readonly ILogger<UpdateExamPassingRuleHandler> _logger;

    public UpdateExamPassingRuleHandler(
        IUnitOfWork unitOfWork,
        IExamAccessService examAccessService,
        ILogger<UpdateExamPassingRuleHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _examAccessService = examAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateExamPassingRuleCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _examAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
            return accessResult;

        var exam = accessResult.Value;

        var passingRuleResult = ExamPassingRule.Create(command.Request.MinScore, command.Request.MinPercent);
        if (passingRuleResult.IsFailure)
            return passingRuleResult;

        var result = exam.ChangePassingRule(passingRuleResult.Value);
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Exam {ExamId} passing rule updated.", command.Id);

        return Result.Success();
    }
}