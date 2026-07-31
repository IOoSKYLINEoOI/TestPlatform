using Microsoft.AspNetCore.Mvc;
using TestPlatform.Application.Common.Error;

namespace TestPlatform.Api.Common;

public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ToErrorResult(string error)
    {
        int status = error switch
        {
            ErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorCodes.Forbidden or
            ErrorCodes.FileForbidden => StatusCodes.Status403Forbidden,
            ErrorCodes.ExamNotFound or
            ErrorCodes.TestNotFound or
            ErrorCodes.QuestionNotFound or
            ErrorCodes.TagNotFound or
            ErrorCodes.AttemptNotFound or
            ErrorCodes.FileNotFound => StatusCodes.Status404NotFound,
            ErrorCodes.TagAlreadyExists or
            ErrorCodes.TagInUse or
            ErrorCodes.IdentityUsernameAlreadyExists or
            ErrorCodes.IdentityEmployeeNumberAlreadyExists or
            ErrorCodes.AttemptNotFinished or
            ErrorCodes.AttemptReviewNotAvailable or
            ErrorCodes.ExamAttemptsLimitReached => StatusCodes.Status409Conflict,
            ErrorCodes.FileInUse => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };

        return Problem(
            statusCode: status,
            title: GetTitle(status),
            detail: GetDetail(error),
            extensions: new Dictionary<string, object?> { ["code"] = error });
    }

    private static string GetTitle(int status) => status switch
    {
        StatusCodes.Status401Unauthorized => "Authentication is required.",
        StatusCodes.Status403Forbidden => "Access is forbidden.",
        StatusCodes.Status404NotFound => "The requested resource was not found.",
        StatusCodes.Status409Conflict => "The request conflicts with the current resource state.",
        _ => "The request is invalid.",
    };

    private static string GetDetail(string error) => error switch
    {
        "test.questions_required" => "Нельзя опубликовать тест без вопросов.",
        "test.invalid_status_transition" => "Тест можно опубликовать только из статуса «Черновик».",
        "test.not_editable" => "Опубликованный или архивный тест нельзя изменить.",
        "test.partial_evaluation_not_supported" => "Тест не поддерживает вопрос с частичным оцениванием.",
        "test.question_already_added" => "Этот вопрос уже добавлен в тест.",
        "test.questions_limit_reached" => "Достигнуто максимальное число вопросов в тесте.",
        "test.question_not_found" => "Вопрос не найден в составе теста.",
        "test.invalid_question_order" => "Передан некорректный порядок вопросов.",
        "test.invalid_title" => "Укажите корректное название теста.",
        "test.invalid_description" => "Укажите корректное описание теста.",
        "test.not_published" => "Тест ещё не опубликован.",
        "exam.insufficient_questions" => "Нельзя опубликовать экзамен: добавьте достаточное число вопросов в каждую секцию.",
        "exam.passing_rule_required" => "Для экзамена нужно указать правило прохождения.",
        "exam.review_requires_end_date" => "Для выбранной политики просмотра результатов укажите дату окончания экзамена.",
        "exam.passing_score_exceeds_maximum" => "Проходной балл не может быть больше максимального балла экзамена.",
        "exam.invalid_status_transition" => "Экзамен можно опубликовать только из статуса «Черновик».",
        "exam.not_editable" => "Опубликованный или архивный экзамен нельзя изменить.",
        "exam.invalid_title" => "Укажите корректное название экзамена.",
        "exam.invalid_description" => "Укажите корректное описание экзамена.",
        "exam.invalid_time_limit" => "Время экзамена должно быть не меньше допустимого значения.",
        "exam.invalid_attempts_limit" => "Количество попыток должно быть не меньше одной.",
        "exam.section.not_found" => "Секция экзамена не найдена.",
        "exam.section.invalid_name" => "Укажите корректное название секции.",
        "exam.section.invalid_selection_count" => "Количество выбираемых вопросов в секции должно быть больше нуля.",
        "exam.section.invalid_score" => "Баллы за вопрос должны быть больше нуля.",
        "exam.section.insufficient_pool" => "В секции недостаточно вопросов для случайного выбора.",
        "exam.section.question_already_added" or "exam.question.already_in_pool" => "Этот вопрос уже добавлен в секцию.",
        "exam.section.pool_limit_reached" or "exam.questions.limit_reached" => "Достигнут лимит вопросов экзамена.",
        "exam.not_published" => "Экзамен ещё не опубликован.",
        "exam.attempts_limit_reached" => "Достигнут лимит попыток прохождения экзамена.",
        "question.not_editable" => "Опубликованный или архивный вопрос нельзя изменить.",
        "question.invalid_status_transition" => "Недопустимое изменение статуса вопроса.",
        "question.not_published" => "В тест или экзамен можно добавить только опубликованный вопрос.",
        "question.tags_not_found" => "Один или несколько выбранных тегов не найдены.",
        "question.file_unavailable" => "Не удалось использовать выбранное изображение.",
        "question.text_required" => "Введите текст вопроса.",
        "question.text_too_long" => "Текст вопроса слишком длинный.",
        "question.answer_definition_required" => "Настройте правильный ответ.",
        "question.answer.too_few_options" => "Добавьте минимум два варианта ответа.",
        "question.answer.too_many_options" => "Добавлено слишком много вариантов ответа.",
        "question.answer.correct_option_required" => "Выберите хотя бы один правильный вариант ответа.",
        "question.answer.single_choice_requires_one_correct" => "Для одиночного выбора нужен ровно один правильный вариант.",
        "question.answer.duplicate_options" => "Варианты ответа не должны повторяться.",
        "tag.already_exists" => "Тег с таким названием уже существует.",
        "tag.in_use" => "Тег используется в вопросах и не может быть удалён.",
        "tag.merge_same_target" => "Нельзя объединить тег с самим собой.",
        "tag.invalid_name" => "Укажите корректное название тега.",
        "tag.invalid_description" => "Укажите корректное описание тега.",
        "file.too_large" => "Размер изображения превышает допустимый лимит.",
        "file.invalid_extension" or "file.invalid_format" => "Поддерживаются только допустимые форматы изображений.",
        "file.empty" => "Нельзя загрузить пустой файл.",
        "file.forbidden" => "У вас нет прав для работы с этим файлом.",
        "file.in_use" => "Изображение используется в контенте и не может быть удалено.",
        "identity.username_already_exists" => "Пользователь с таким логином уже существует.",
        "identity.employee_number_already_exists" => "Пользователь с таким табельным номером уже существует.",
        "attempt.not_finished" => "Попытка ещё не завершена.",
        "attempt.review_not_available" => "Просмотр результатов этой попытки пока недоступен.",
        _ => $"Операция не может быть выполнена ({error}).",
    };
}
