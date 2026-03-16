using Microsoft.EntityFrameworkCore;
using TestPlatform.Infrastructure.Postgres.Questions.Entities;
using TestPlatform.Infrastructure.Postgres.Tags.Entities;
using TestPlatform.Infrastructure.Postgres.Tests.Entities;

namespace TestPlatform.Infrastructure.Postgres;

public class DbInitializer
{
    public static async Task InitializeAsync(TestPlatformDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Tags.AnyAsync())
        {
            var tags = new List<TagEntity>
            {
                new TagEntity
                {
                    Id = Guid.Parse("21b84196-baec-458d-ba2c-3e92c0e3b7a6"),
                    Name = "Математика",
                    Description = "Алгебра, геометрия и основы математического анализа",
                },
                new TagEntity
                {
                    Id = Guid.Parse("35531abe-b952-4c62-ac16-dc989fc07ed5"),
                    Name = "Физика",
                    Description = "Основы механики, электричества и оптики",
                },
                new TagEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Химия",
                    Description = "Основы неорганической и органической химии",
                },
                new TagEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Биология",
                    Description = "Строение организмов и биологические процессы",
                },
                new TagEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "История",
                    Description = "Основные исторические события и эпохи",
                },
                new TagEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "География",
                    Description = "Природа, население и экономика стран мира",
                },
                new TagEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Русский язык",
                    Description = "Грамматика, орфография и пунктуация",
                },
                new TagEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Литература",
                    Description = "Произведения и авторы мировой и русской литературы",
                },
                new TagEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Информатика",
                    Description = "Основы алгоритмов, программирования и компьютерных технологий",
                },
                new TagEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Английский язык",
                    Description = "Лексика, грамматика и понимание английского языка",
                },
            };

            await context.Tags.AddRangeAsync(tags);
            await context.SaveChangesAsync();
        }

        if (!await context.Questions.AnyAsync())
        {
            var unitTestingTag = await context.Tags.FirstAsync(t => t.Id == Guid.Parse("21b84196-baec-458d-ba2c-3e92c0e3b7a6"));
            var aspNetTag = await context.Tags.FirstAsync(t => t.Id == Guid.Parse("35531abe-b952-4c62-ac16-dc989fc07ed5"));

            var questions = new List<QuestionEntity>
            {
                new QuestionEntity
                {
                    Id = Guid.Parse("441eba08-c9b8-4ef4-bf89-e95124ed074e"),
                    Text = "Что такое unit-тест?",
                    QuestionTypeId = 1,
                    Points = 5,
                    Tags = new List<TagEntity> { unitTestingTag },
                    AnswersOptions = new List<AnswerOptionEntity>
                    {
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Тест проверяет отдельный метод или класс", IsCorrect = true },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Тестирует весь проект целиком", IsCorrect = false },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Проверка дизайна приложения", IsCorrect = false },
                    },
                },
                new QuestionEntity
                {
                    Id = Guid.Parse("6b0f80ba-c4ab-46be-b412-f00bdb695717"),
                    Text = "Что такое integration-тест?",
                    QuestionTypeId = 1,
                    Points = 5,
                    Tags = new List<TagEntity> { unitTestingTag },
                    AnswersOptions = new List<AnswerOptionEntity>
                    {
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Тестирование взаимодействия нескольких компонентов", IsCorrect = true },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Тестирование одного метода", IsCorrect = false },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Тестирование UI", IsCorrect = false },
                    },
                },
                new QuestionEntity
                {
                    Id = Guid.Parse("6d9dd809-e7b6-41d2-8981-63173baaa2b6"),
                    Text = "Что такое middleware в ASP.NET Core?",
                    QuestionTypeId = 1,
                    Points = 5,
                    Tags = new List<TagEntity> { aspNetTag },
                    AnswersOptions = new List<AnswerOptionEntity>
                    {
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Компонент обработки HTTP-запросов и ответов", IsCorrect = true },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "База данных приложения", IsCorrect = false },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "UI-компонент", IsCorrect = false },
                    },
                },
                new QuestionEntity
                {
                    Id = Guid.Parse("91da71dc-00e7-4e7c-865b-0626d1f9de00"),
                    Text = "Что делает метод ConfigureServices в ASP.NET Core?",
                    QuestionTypeId = 1,
                    Points = 5,
                    Tags = new List<TagEntity> { aspNetTag },
                    AnswersOptions = new List<AnswerOptionEntity>
                    {
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Регистрирует сервисы для DI", IsCorrect = true },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Обрабатывает HTTP-запросы", IsCorrect = false },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Запускает базу данных", IsCorrect = false },
                    },
                },
                new QuestionEntity
                {
                    Id = Guid.Parse("9eec1790-6275-4558-8883-3150e80aefe1"),
                    Text = "Что такое TDD (Test Driven Development)?",
                    QuestionTypeId = 1,
                    Points = 5,
                    Tags = new List<TagEntity> { unitTestingTag },
                    AnswersOptions = new List<AnswerOptionEntity>
                    {
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Разработка через написание тестов до кода", IsCorrect = true },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Разработка без тестов", IsCorrect = false },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Ручное тестирование", IsCorrect = false },
                    },
                },
                new QuestionEntity
                {
                    Id = Guid.Parse("a2c5c76e-077d-4c82-a1bc-e34fadc3be9b"),
                    Text = "Что делает метод UseRouting() в ASP.NET Core?",
                    QuestionTypeId = 1,
                    Points = 5,
                    Tags = new List<TagEntity> { aspNetTag },
                    AnswersOptions = new List<AnswerOptionEntity>
                    {
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Добавляет обработку маршрутизации для запросов", IsCorrect = true },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Подключает базу данных", IsCorrect = false },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Создает UI-компоненты", IsCorrect = false },
                    },
                },
                new QuestionEntity
                {
                    Id = Guid.Parse("b68c6041-9ceb-43ff-a0c9-b19763e2a85b"),
                    Text = "Что такое мок (mock) в unit-тестировании?",
                    QuestionTypeId = 1,
                    Points = 5,
                    Tags = new List<TagEntity> { unitTestingTag },
                    AnswersOptions = new List<AnswerOptionEntity>
                    {
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Поддельный объект для тестирования зависимостей", IsCorrect = true },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Настоящий сервис из продакшена", IsCorrect = false },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Метод приложения", IsCorrect = false },
                    },
                },
                new QuestionEntity
                {
                    Id = Guid.Parse("c451d0f1-4eae-4b4e-bbda-c5dd6524bcc2"),
                    Text = "Что делает метод UseEndpoints() в ASP.NET Core?",
                    QuestionTypeId = 1,
                    Points = 5,
                    Tags = new List<TagEntity> { aspNetTag },
                    AnswersOptions = new List<AnswerOptionEntity>
                    {
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Настраивает конечные точки приложения", IsCorrect = true },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Запускает DI контейнер", IsCorrect = false },
                        new AnswerOptionEntity{ Id = Guid.NewGuid(), Text = "Создает тесты", IsCorrect = false },
                    },
                },
                new QuestionEntity
                {
                    Id = Guid.Parse("ef07942f-c3dc-4000-81a2-63f6398f299f"),
                    Text = "Что делает Assert.AreEqual в unit-тесте?",
                    QuestionTypeId = 1,
                    Points = 5,
                    Tags = new List<TagEntity> { unitTestingTag },
                    AnswersOptions = new List<AnswerOptionEntity>
                    {
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Проверяет, что два значения равны", IsCorrect = true },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Запускает тест", IsCorrect = false },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Создает мок объект", IsCorrect = false },
                    },
                },
                new QuestionEntity
                {
                    Id = Guid.Parse("f427dd40-d448-46d0-a64a-b89ad015139f"),
                    Text = "Что такое dependency injection в ASP.NET Core?",
                    QuestionTypeId = 1,
                    Points = 5,
                    Tags = new List<TagEntity> { aspNetTag },
                    AnswersOptions = new List<AnswerOptionEntity>
                    {
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Внедрение зависимостей через контейнер сервисов", IsCorrect = true },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Ручное создание объектов внутри методов", IsCorrect = false },
                        new AnswerOptionEntity { Id = Guid.NewGuid(), Text = "Тестирование методов", IsCorrect = false },
                    },
                },
            };

            await context.Questions.AddRangeAsync(questions);
            await context.SaveChangesAsync();
        }

        if (!await context.Tests.AnyAsync())
        {
            var allQuestionIds = new Dictionary<string, Guid>
            {
                ["mock"] = Guid.Parse("441eba08-c9b8-4ef4-bf89-e95124ed074e"),
                ["configureServices"] = Guid.Parse("6b0f80ba-c4ab-46be-b412-f00bdb695717"),
                ["integration"] = Guid.Parse("6d9dd809-e7b6-41d2-8981-63173baaa2b6"),
                ["unitTest"] = Guid.Parse("91da71dc-00e7-4e7c-865b-0626d1f9de00"),
                ["dependencyInjection"] = Guid.Parse("9eec1790-6275-4558-8883-3150e80aefe1"),
                ["useEndpoints"] = Guid.Parse("a2c5c76e-077d-4c82-a1bc-e34fadc3be9b"),
                ["tdd"] = Guid.Parse("b68c6041-9ceb-43ff-a0c9-b19763e2a85b"),
                ["useRouting"] = Guid.Parse("c451d0f1-4eae-4b4e-bbda-c5dd6524bcc2"),
                ["assert"] = Guid.Parse("ef07942f-c3dc-4000-81a2-63f6398f299f"),
                ["middleware"] = Guid.Parse("f427dd40-d448-46d0-a64a-b89ad015139f"),
            };

            var questions = await context.Questions
                .Where(q => allQuestionIds.Values.Contains(q.Id))
                .ToListAsync();


            var test1 = new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "Unit Testing Basics",
                Description = "Тест на знание основ модульного тестирования",
                TimeLimitSeconds = 600,
                Questions = questions.Where(q => new[]
                {
                    allQuestionIds["mock"],
                    allQuestionIds["unitTest"],
                    allQuestionIds["integration"],
                    allQuestionIds["tdd"],
                    allQuestionIds["assert"],
                }.Contains(q.Id)).ToList(),
            };

            var test2 = new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "ASP.NET Core Middleware & Routing",
                Description = "Тест на знание ASP.NET Core Middleware и маршрутизации",
                TimeLimitSeconds = 900,
                Questions = questions.Where(q => new[]
                {
                    allQuestionIds["middleware"],
                    allQuestionIds["useRouting"],
                    allQuestionIds["useEndpoints"],
                    allQuestionIds["configureServices"],
                    allQuestionIds["dependencyInjection"],
                }.Contains(q.Id)).ToList(),
            };

            var test3 = new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "Full ASP.NET Core & Unit Testing Test",
                Description = "Тест на знание ASP.NET Core и Unit Testing, полный набор вопросов",
                TimeLimitSeconds = 1200,
                Questions = questions,
            };

            await context.Tests.AddRangeAsync(test1, test2, test3);
            await context.SaveChangesAsync();
        }

    }
}