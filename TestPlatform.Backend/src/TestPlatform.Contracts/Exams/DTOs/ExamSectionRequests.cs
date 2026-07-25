namespace TestPlatform.Contracts.Exams.DTOs;

public record CreateExamSectionRequest(
    string Name,
    int QuestionsToSelect,
    int ScorePerQuestion);

public record UpdateExamSectionRequest(
    string Name,
    int QuestionsToSelect,
    int ScorePerQuestion);

public record AddExamSectionQuestionRequest(Guid QuestionId);

public record UpdateExamAttemptsLimitRequest(int AttemptsLimit);
