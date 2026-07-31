using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TestPlatform.Web.OpenApi;

public sealed class QuestionRequestExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath;
        var method = context.ApiDescription.HttpMethod;

        var isQuestionRequest = (path is "questions" or "questions/{id}")
            && (method is "POST" or "PUT");

        if (!isQuestionRequest)
        {
            return;
        }

        var requestBody = operation.RequestBody;
        if (requestBody is null
            || !requestBody.Content!.TryGetValue("application/json", out var mediaType))
        {
            return;
        }

#pragma warning disable SA1118
        mediaType.Examples = new Dictionary<string, IOpenApiExample>
        {
            ["text"] = Example("Текстовый вопрос", """
                { "kind": "text", "text": "Столица Франции?", "explanation": "Париж — столица Франции.", "imageId": null, "tagIds": [], "correctAnswer": "Париж" }
                """),
            ["number"] = Example("Числовой вопрос", """
                { "kind": "number", "text": "Сколько будет 2 + 2?", "explanation": "Сумма равна 4.", "imageId": null, "tagIds": [], "correctAnswer": 4 }
                """),
            ["choice"] = Example("Вопрос с вариантами", """
                { "kind": "choice", "text": "Выберите чётное число", "explanation": "2 делится на 2 без остатка.", "imageId": null, "tagIds": [], "mode": 0, "evaluationMode": 0, "options": [{ "text": "2", "isCorrect": true, "imageId": null }, { "text": "3", "isCorrect": false, "imageId": null }] }
                """),
            ["matching"] = Example("Вопрос на сопоставление", """
                { "kind": "matching", "text": "Соотнесите страну и столицу", "explanation": "Париж — столица Франции.", "imageId": null, "tagIds": [], "evaluationMode": 0, "leftItems": [{ "id": "11111111-1111-1111-1111-111111111111", "text": "Франция", "imageId": null }], "rightItems": [{ "id": "22222222-2222-2222-2222-222222222222", "text": "Париж", "imageId": null }], "pairs": [{ "leftId": "11111111-1111-1111-1111-111111111111", "rightId": "22222222-2222-2222-2222-222222222222" }] }
                """),
        };
#pragma warning restore SA1118
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
