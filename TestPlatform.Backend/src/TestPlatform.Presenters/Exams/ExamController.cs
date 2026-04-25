using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Exams.Features.AddExamQuestionCommand;
using TestPlatform.Application.Exams.Features.ArchiveExamCommand;
using TestPlatform.Application.Exams.Features.CreateExamCommand;
using TestPlatform.Application.Exams.Features.DeleteExamCoverImageCommand;
using TestPlatform.Application.Exams.Features.DeleteExamQuestionCommand;
using TestPlatform.Application.Exams.Features.DeleteExamScheduleCommand;
using TestPlatform.Application.Exams.Features.DeleteExamTimeLimitCommand;
using TestPlatform.Application.Exams.Features.GetByIdExamQuery;
using TestPlatform.Application.Exams.Features.PublishExamCommand;
using TestPlatform.Application.Exams.Features.UpdateExamCoverImageCommand;
using TestPlatform.Application.Exams.Features.UpdateExamDetailsCommand;
using TestPlatform.Application.Exams.Features.UpdateExamPassingRuleCommand;
using TestPlatform.Application.Exams.Features.UpdateExamScheduleCommand;
using TestPlatform.Application.Exams.Features.UpdateExamTimeLimitCommand;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Exams.DTOs;

namespace TestPlatform.Presenters.Exams;

[ApiController]
[Route("exams")]
public class ExamController : ControllerBase
{
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        OperationId = "GetByIdExam",
        Summary = "Получить экзамен по Id.",
        Description = "Возвращает экзамен с деталями и списком вопросов")]
    public async Task<IActionResult> GetById(
        [FromServices] IQueryHandler<ExamFullResponse, GetByIdExamQuery> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetByIdExamQuery(id);

        var result = await handler.Handle(query, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(result.Error);
    }

    [Authorize(Roles = "Teacher, Admin")]
    [HttpPost]
    [SwaggerOperation(
        OperationId = "CreateExam",
        Summary = "Создать черновик экзамена",
        Description = "Создаёт новый экзамен с указаным названием и описанием и статусом Draft")]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<Guid, CreateExamCommand> handler,
        [FromBody] ExamRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateExamCommand(request);

        var result = await handler.Handle(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value },
            result.Value);
    }

    [HttpPatch("{id:guid}")]
    [SwaggerOperation(
        OperationId = "UpdateExamDetails",
        Summary = "Обновить детали экзамена",
        Description = "Обновить существующий экзамен по Id с новыми данными: название, описание.")]
    public async Task<IActionResult> UpdateDetails(
        [FromServices] ICommandHandler<UpdateExamDetailsCommand> handler,
        [FromRoute] Guid id,
        [FromBody] UpdateExamDetailsRequest detailsRequest,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExamDetailsCommand(id, detailsRequest);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error switch
            {
                ErrorCodes.Unauthorized => Unauthorized(),
                ErrorCodes.Forbidden => Forbid(),
                ErrorCodes.ExamNotFound => NotFound(),
                _ => BadRequest(result.Error)
            };
    }

    [HttpPut("{id:guid}/time-limit")]
    [SwaggerOperation(
        OperationId = "UpdateExamTimeLimit",
        Summary = "Обновить временное ограничение для экзамена",
        Description = "Обновляет существующий экзамен по Id с новыми данными: временное ограничение.")]
    public async Task<IActionResult> UpdateTimeLimit(
        [FromServices] ICommandHandler<UpdateExamTimeLimitCommand> handler,
        [FromRoute] Guid id,
        [FromBody] UpdateExamTimeLimitRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExamTimeLimitCommand(id, request);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error switch
            {
                ErrorCodes.Unauthorized => Unauthorized(),
                ErrorCodes.Forbidden => Forbid(),
                ErrorCodes.ExamNotFound => NotFound(),
                _ => BadRequest(result.Error)
            };
    }

    [HttpDelete("{id:guid}/time-limit")]
    [SwaggerOperation(
        OperationId = "DeleteExamTimeLimit",
        Summary = "Удалить временное ограничение для экзамена",
        Description = "Обновляет существующий экзамен по Id с новыми данными: временное ограничение.")]
    public async Task<IActionResult> DeleteTimeLimit(
        [FromServices] ICommandHandler<DeleteExamTimeLimitCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteExamTimeLimitCommand(id);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error switch
            {
                ErrorCodes.Unauthorized => Unauthorized(),
                ErrorCodes.Forbidden => Forbid(),
                ErrorCodes.ExamNotFound => NotFound(),
                _ => BadRequest(result.Error)
            };
    }

    [HttpPut("{id:guid}/cover-image")]
    [SwaggerOperation(
        OperationId = "UpdateExamCoverImage",
        Summary = "Обновить изображение для экзамена",
        Description = "Обновляет существующий экзамен по Id с новыми данными: изображение.")]
    public async Task<IActionResult> UpdateCoverImage(
        [FromServices] ICommandHandler<UpdateExamCoverImageCommand> handler,
        [FromRoute] Guid id,
        [FromBody] UpdateExamCoverImageRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExamCoverImageCommand(id, request);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error switch
            {
                ErrorCodes.Unauthorized => Unauthorized(),
                ErrorCodes.Forbidden => Forbid(),
                ErrorCodes.ExamNotFound => NotFound(),
                _ => BadRequest(result.Error)
            };
    }

    [HttpDelete("{id:guid}/cover-image")]
    [SwaggerOperation(
        OperationId = "DeleteExamCoverImage",
        Summary = "Удалить изображение для экзамена",
        Description = "Обновляет существующий экзамен по Id с новыми данными: изображение.")]
    public async Task<IActionResult> DeleteCoverImage(
        [FromServices] ICommandHandler<DeleteExamCoverImageCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteExamCoverImageCommand(id);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error switch
            {
                ErrorCodes.Unauthorized => Unauthorized(),
                ErrorCodes.Forbidden => Forbid(),
                ErrorCodes.ExamNotFound => NotFound(),
                _ => BadRequest(result.Error)
            };
    }

    [HttpPut("{id:guid}/schedule")]
    [SwaggerOperation(
        OperationId = "UpdateExamSchedule",
        Summary = "Обновить расписание для экзамена",
        Description = "Обновляет существующий экзамен по Id с новыми данными: расписание.")]
    public async Task<IActionResult> UpdateSchedule(
        [FromServices] ICommandHandler<UpdateExamScheduleCommand> handler,
        [FromRoute] Guid id,
        [FromBody] UpdateExamScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExamScheduleCommand(id, request);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error switch
            {
                ErrorCodes.Unauthorized => Unauthorized(),
                ErrorCodes.Forbidden => Forbid(),
                ErrorCodes.ExamNotFound => NotFound(),
                _ => BadRequest(result.Error)
            };
    }

    [HttpDelete("{id:guid}/schedule")]
    [SwaggerOperation(
        OperationId = "DeleteExamSchedule",
        Summary = "Удалить расписание для экзамена",
        Description = "Обновляет существующий экзамен по Id с новыми данными: расписание.")]
    public async Task<IActionResult> DeleteSchedule(
        [FromServices] ICommandHandler<DeleteExamScheduleCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteExamScheduleCommand(id);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error switch
            {
                ErrorCodes.Unauthorized => Unauthorized(),
                ErrorCodes.Forbidden => Forbid(),
                ErrorCodes.ExamNotFound => NotFound(),
                _ => BadRequest(result.Error)
            };
    }

    [HttpPut("{id:guid}/passing-rule")]
    [SwaggerOperation(
        OperationId = "UpdateExamPassingRule",
        Summary = "Обновить правила прохождения для экзамена",
        Description = "Обновляет существующий экзамен по Id с новыми данными: правила прохождения.")]
    public async Task<IActionResult> UpdatePassingRule(
        [FromServices] ICommandHandler<UpdateExamPassingRuleCommand> handler,
        [FromRoute] Guid id,
        [FromBody] UpdateExamPassingRuleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExamPassingRuleCommand(id, request);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error switch
            {
                ErrorCodes.Unauthorized => Unauthorized(),
                ErrorCodes.Forbidden => Forbid(),
                ErrorCodes.ExamNotFound => NotFound(),
                _ => BadRequest(result.Error)
            };
    }

    [Authorize(Roles = "Teacher, Admin")]
    [HttpPost("{id:guid}/publish")]
    [SwaggerOperation(
        OperationId = "PublishExam",
        Summary = "Опубликовать экзамена",
        Description = "Проверяет обязательные условия для публикации экзамена и меняет статус на Publish")]
    public async Task<IActionResult> Publish(
        [FromServices] ICommandHandler<PublishExamCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new PublishExamCommand(id);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error switch
            {
                ErrorCodes.Unauthorized => Unauthorized(),
                ErrorCodes.Forbidden => Forbid(),
                ErrorCodes.ExamNotFound => NotFound(),
                _ => BadRequest(result.Error)
            };
    }

    [Authorize(Roles = "Teacher, Admin")]
    [HttpPost("{id:guid}/archive")]
    [SwaggerOperation(
        OperationId = "ArchiveExam",
        Summary = "Архивировать экзамена",
        Description = "Архивирует экзамен после публикации.")]
    public async Task<IActionResult> Archive(
        [FromServices] ICommandHandler<ArchiveExamCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ArchiveExamCommand(id);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error switch
            {
                ErrorCodes.Unauthorized => Unauthorized(),
                ErrorCodes.Forbidden => Forbid(),
                ErrorCodes.ExamNotFound => NotFound(),
                _ => BadRequest(result.Error)
            };
    }

    [Authorize(Roles = "Teacher, Admin")]
    [HttpPost("{id:guid}/questions")]
    [SwaggerOperation(
        OperationId = "AddExamQuestion",
        Summary = "Добавить вопрос в экзамен.",
        Description = "Добавляет вопрос в конец экзамена")]
    public async Task<IActionResult> AddQuestion(
        [FromServices] ICommandHandler<AddExamQuestionCommand> handler,
        [FromRoute] Guid id,
        [FromBody] AddExamQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddExamQuestionCommand(id, request);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error switch
            {
                ErrorCodes.Unauthorized => Unauthorized(),
                ErrorCodes.Forbidden => Forbid(),
                ErrorCodes.ExamNotFound or ErrorCodes.QuestionNotFound => NotFound(),
                _ => BadRequest(result.Error)
            };
    }

    [Authorize(Roles = "Teacher, Admin")]
    [HttpDelete("{id:guid}/questions/{questionId:guid}")]
    [SwaggerOperation(
        OperationId = "DeleteExamQuestion",
        Summary = "Удалить вопрос из экзамен.",
        Description = "Удаляет вопрос из экзамена экзамена")]
    public async Task<IActionResult> DeleteQuestion(
        [FromServices] ICommandHandler<DeleteExamQuestionCommand> handler,
        [FromRoute] Guid id,
        [FromRoute] Guid questionId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteExamQuestionCommand(id, questionId);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error switch
            {
                ErrorCodes.Unauthorized => Unauthorized(),
                ErrorCodes.Forbidden => Forbid(),
                ErrorCodes.ExamNotFound or ErrorCodes.QuestionNotInExam => NotFound(),
                _ => BadRequest(result.Error)
            };
    }
}