using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TestPlatform.Web.OpenApi;

public sealed class ApiDocumentationOperationFilter : IOperationFilter
{
    private static readonly IReadOnlyDictionary<string, Documentation> DocumentationByAction =
        new Dictionary<string, Documentation>(StringComparer.Ordinal)
        {
            ["AttemptsController.GetMyAttempts"] = new("Мои попытки", "Возвращает историю попыток текущего пользователя с фильтрацией и пагинацией."),
            ["AttemptsController.GetById"] = new("Получить попытку", "Возвращает текущее состояние попытки по идентификатору."),
            ["AttemptsController.Start"] = new("Начать попытку", "Создаёт попытку прохождения теста или экзамена."),
            ["AttemptsController.SaveAnswer"] = new("Сохранить ответ", "Создаёт или обновляет ответ на вопрос в попытке."),
            ["AttemptsController.RemoveAnswer"] = new("Удалить ответ", "Удаляет сохранённый ответ на вопрос в попытке."),
            ["AttemptsController.Finish"] = new("Завершить попытку", "Завершает попытку и возвращает рассчитанный результат."),
            ["AttemptsController.Abandon"] = new("Прервать попытку", "Завершает попытку без результата."),
            ["AttemptsController.Cancel"] = new("Отменить попытку", "Администратор отменяет попытку пользователя."),
            ["AttemptsController.GetResult"] = new("Получить результат попытки", "Возвращает подробный результат завершённой попытки."),
            ["ExamsController.GetCatalog"] = new("Каталог экзаменов", "Возвращает опубликованные экзамены с пагинацией."),
            ["ExamsController.GetById"] = new("Получить экзамен", "Возвращает полную конфигурацию экзамена для редактора."),
            ["ExamsController.Create"] = new("Создать экзамен", "Создаёт черновик экзамена."),
            ["ExamsController.UpdateDetails"] = new("Изменить экзамен", "Обновляет основные сведения об экзамене."),
            ["ExamsController.Publish"] = new("Опубликовать экзамен", "Делает экзамен доступным пользователям."),
            ["ExamsController.Archive"] = new("Архивировать экзамен", "Скрывает экзамен из каталога и прекращает его использование."),
            ["ExamAttemptsController.GetAll"] = new(
                "Попытки экзамена",
                "Административный список попыток прохождения указанного экзамена. Доступен автору экзамена и администратору."),
            ["ExamSettingsController.UpdateTimeLimit"] = new("Установить лимит времени экзамена", "Задаёт ограничение времени на прохождение экзамена."),
            ["ExamSettingsController.DeleteTimeLimit"] = new("Удалить лимит времени экзамена", "Снимает ограничение времени на прохождение экзамена."),
            ["ExamSettingsController.UpdateCoverImage"] = new("Установить обложку экзамена", "Привязывает изображение обложки к экзамену."),
            ["ExamSettingsController.DeleteCoverImage"] = new("Удалить обложку экзамена", "Удаляет обложку экзамена."),
            ["ExamSettingsController.UpdateSchedule"] = new("Настроить расписание экзамена", "Задаёт период доступности экзамена."),
            ["ExamSettingsController.DeleteSchedule"] = new("Удалить расписание экзамена", "Снимает ограничения по датам доступности экзамена."),
            ["ExamSettingsController.UpdatePassingRule"] = new("Настроить проходной балл", "Задаёт правило успешного прохождения экзамена."),
            ["ExamSettingsController.UpdateAttemptsLimit"] = new("Установить лимит попыток экзамена", "Задаёт максимальное количество попыток для экзамена."),
            ["ExamSettingsController.UpdateReviewPolicy"] = new("Настроить просмотр результатов", "Задаёт политику просмотра результатов экзамена."),
            ["ExamSectionsController.Create"] = new("Добавить раздел экзамена", "Создаёт раздел в экзамене."),
            ["ExamSectionsController.Update"] = new("Изменить раздел экзамена", "Обновляет название и настройки раздела экзамена."),
            ["ExamSectionsController.Delete"] = new("Удалить раздел экзамена", "Удаляет раздел из экзамена."),
            ["ExamSectionsController.AddQuestion"] = new("Добавить вопрос в раздел", "Добавляет вопрос в раздел экзамена."),
            ["ExamSectionsController.RemoveQuestion"] = new("Удалить вопрос из раздела", "Удаляет вопрос из раздела экзамена."),
            ["ImageController.Upload"] = new("Загрузить изображение", "Загружает изображение для использования в контенте."),
            ["ImageController.Get"] = new("Получить изображение", "Возвращает файл изображения."),
            ["ImageController.GetUrl"] = new("Получить ссылку на изображение", "Возвращает временную ссылку для загрузки изображения."),
            ["ImageController.Delete"] = new("Удалить изображение", "Удаляет изображение, если у пользователя есть доступ."),
            ["QuestionsController.GetById"] = new("Получить вопрос", "Возвращает вопрос для редактирования."),
            ["QuestionsController.GetAll"] = new("Список вопросов", "Возвращает вопросы с фильтрацией и пагинацией."),
            ["QuestionsController.Create"] = new("Создать вопрос", "Создаёт вопрос в статусе черновика."),
            ["QuestionsController.Update"] = new("Изменить вопрос", "Обновляет содержимое и настройки вопроса."),
            ["QuestionsController.Publish"] = new("Опубликовать вопрос", "Делает вопрос доступным для использования."),
            ["QuestionsController.Archive"] = new("Архивировать вопрос", "Исключает вопрос из доступных для использования."),
            ["QuestionsController.Clone"] = new("Клонировать вопрос", "Создаёт копию существующего вопроса."),
            ["TagsController.GetById"] = new("Получить тег", "Возвращает тег по идентификатору."),
            ["TagsController.GetAll"] = new("Список тегов", "Возвращает теги с поиском и пагинацией."),
            ["TagsController.GetSuggestions"] = new("Подсказки тегов", "Возвращает не более десяти тегов для автодополнения."),
            ["TagsController.GetUsage"] = new("Использование тега", "Возвращает количество вопросов с указанным тегом."),
            ["TagsController.GetQuestions"] = new("Вопросы по тегу", "Возвращает вопросы, содержащие указанный тег."),
            ["TagsController.Create"] = new("Создать тег", "Создаёт тег для классификации вопросов."),
            ["TagsController.Update"] = new("Изменить тег", "Обновляет название и описание тега."),
            ["TagsController.Merge"] = new("Объединить теги", "Переносит вопросы из исходного тега в целевой и удаляет исходный тег."),
            ["TagsController.Delete"] = new("Удалить тег", "Удаляет неиспользуемый тег."),
            ["TestsController.GetById"] = new("Получить тест", "Возвращает тест по идентификатору."),
            ["TestsController.GetAll"] = new("Список тестов", "Возвращает тесты с фильтрацией и пагинацией."),
            ["TestsController.Create"] = new("Создать тест", "Создаёт черновик теста."),
            ["TestsController.Publish"] = new("Опубликовать тест", "Делает тест доступным пользователям."),
            ["TestsController.Archive"] = new("Архивировать тест", "Скрывает тест и прекращает его использование."),
            ["TestsController.AddQuestion"] = new("Добавить вопрос в тест", "Добавляет вопрос в тест."),
            ["TestsController.DeleteQuestion"] = new("Удалить вопрос из теста", "Удаляет вопрос из теста."),
            ["TestAttemptsController.GetAll"] = new(
                "Попытки теста",
                "Административный список попыток прохождения указанного теста. Доступен автору теста и администратору."),
            ["UsersController.GetCurrent"] = new("Текущий пользователь", "Возвращает профиль авторизованного пользователя."),
            ["UsersController.Create"] = new("Создать учётную запись", "Создаёт учётную запись сотрудника в системе идентификации."),
        };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var action = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;
        if (action is null)
        {
            return;
        }

        var key = $"{action.ControllerTypeInfo.Name}.{action.ActionName}";
        operation.OperationId ??= $"{action.ControllerName}_{action.ActionName}";

        if (DocumentationByAction.TryGetValue(key, out var documentation))
        {
            operation.Summary = documentation.Summary;
            operation.Description = documentation.Description;
        }
        else
        {
            operation.Summary ??= $"{context.ApiDescription.HttpMethod} {context.ApiDescription.RelativePath}";
            operation.Description ??= "Операция API требует аутентификации.";
        }

        AddResponse(operation, "401", "Unauthorized");
        AddResponse(operation, "403", "Forbidden");
        AddResponse(operation, "500", "Internal Server Error");
    }

    private static void AddResponse(OpenApiOperation operation, string statusCode, string description)
    {
        var responses = operation.Responses;
        if (responses is not null && !responses.ContainsKey(statusCode))
        {
            responses.Add(statusCode, new OpenApiResponse { Description = description });
        }
    }

    private sealed record Documentation(string Summary, string Description);
}
