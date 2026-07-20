using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TestPlatform.Web.Swagger;

public sealed class QuestionRequestExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath;
        var method = context.ApiDescription.HttpMethod;

        if (path is not ("questions" or "questions/{id}") || method is not ("POST" or "PUT"))
        {
            return;
        }

        if (operation.RequestBody?.Content.TryGetValue("application/json", out var mediaType) != true)
        {
            return;
        }

        mediaType.Examples = new Dictionary<string, IOpenApiExample>
        {
            ["text"] = Example("Текстовый вопрос", """
                { "kind": "text", "text": "Столица Франции?", "imageId": null, "tagIds": [], "correctAnswer": "Париж" }
                """),
            ["number"] = Example("Числовой вопрос", """
                { "kind": "number", "text": "Сколько будет 2 + 2?", "imageId": null, "tagIds": [], "correctAnswer": 4 }
                """),
            ["choice"] = Example("Вопрос с вариантами", """
                { "kind": "choice", "text": "Выберите чётное число", "imageId": null, "tagIds": [], "mode": 0, "evaluationMode": 0, "options": [{ "text": "2", "isCorrect": true, "imageId": null }, { "text": "3", "isCorrect": false, "imageId": null }] }
                """),
            ["matching"] = Example("Вопрос на сопоставление", """
                { "kind": "matching", "text": "Соотнесите страну и столицу", "imageId": null, "tagIds": [], "evaluationMode": 0, "leftItems": [{ "id": "11111111-1111-1111-1111-111111111111", "text": "Франция", "isCorrect": true, "imageId": null }], "rightItems": [{ "id": "22222222-2222-2222-2222-222222222222", "text": "Париж", "isCorrect": true, "imageId": null }], "pairs": [{ "leftId": "11111111-1111-1111-1111-111111111111", "rightId": "22222222-2222-2222-2222-222222222222" }] }
                """),
        };
    }

    private static OpenApiExample Example(string summary, string json)
    {
        return new OpenApiExample
        {
            Summary = summary,
            Value = JsonNode.Parse(json),
        };
    }
}
