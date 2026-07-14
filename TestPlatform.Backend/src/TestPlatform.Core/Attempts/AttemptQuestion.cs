using TestPlatform.Core.Questions;

namespace TestPlatform.Core.Attempts;

public record AttemptQuestion(
    int Order,
    decimal Score,
    Question Question);