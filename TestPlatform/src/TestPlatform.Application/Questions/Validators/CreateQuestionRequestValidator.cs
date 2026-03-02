using FluentValidation;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Questions.Validators;

public class CreateQuestionRequestValidator : AbstractValidator<QuestionRequest>
{
    private const int MaxAnswers = 50;
    private const int MaxLengthText = 200;

    public CreateQuestionRequestValidator()
    {
        RuleFor(request => request.Text)
            .NotEmpty().WithMessage("Текст вопроса обязателен.")
            .MaximumLength(MaxLengthText).WithMessage("Текст вопроса не должен превышать 200 символов.");

        RuleFor(request => request.QuestionTypeId)
            .Must(type => Enum.IsDefined(typeof(QuestionType), type)).WithMessage("Некорректный тип вопроса.");

        When(IsChoiceQuestion, () =>
        {
            RuleFor(x => x.CreateAnswerOptions)
                .NotNull().WithMessage("Варианты ответа обязательны.")
                .Must(x => x.Count > 0).WithMessage("Должен быть хотя бы один вариант ответа.")
                .Must(x => x.Count <= MaxAnswers).WithMessage($"Нельзя добавить больше {MaxAnswers} вариантов ответа.");

            RuleForEach(x => x.CreateAnswerOptions)
                .ChildRules(answer =>
                {
                    answer.RuleFor(a => a.Text)
                        .NotEmpty().WithMessage("Текст варианта обязателен.")
                        .MaximumLength(MaxLengthText).WithMessage($"Текст варианта не должен превышать {MaxLengthText} символов.");
                });

            RuleFor(x => x.CreateAnswerOptions)
                .Must(HaveCorrectAnswer).WithMessage("Должен быть выбран правильный вариант ответа.");
        });

        When(IsSingleChoice, () =>
        {
            RuleFor(x => x.CreateAnswerOptions)
                .Must(HaveExactlyOneCorrect).WithMessage("Для вопроса с одним вариантом должен быть ровно один правильный ответ.");
        });
    }

    private static bool IsChoiceQuestion(QuestionRequest request)
    {
        return request.QuestionTypeId == (int)QuestionType.SingleChoice ||
               request.QuestionTypeId == (int)QuestionType.MultipleChoice;
    }

    private static bool IsSingleChoice(QuestionRequest request)
    {
        return request.QuestionTypeId == (int)QuestionType.SingleChoice;
    }

    private static bool HaveCorrectAnswer(List<CreateAnswerOptionRequest> answers)
    {
        return answers.Any(a => a.IsCorrect);
    }

    private static bool HaveExactlyOneCorrect(List<CreateAnswerOptionRequest> answers)
    {
        return answers.Count(a => a.IsCorrect) == 1;
    }
}