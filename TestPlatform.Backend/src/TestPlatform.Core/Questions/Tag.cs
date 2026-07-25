using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Questions;

public class Tag
{
    private const int MaxLengthName = 100;
    private const int MaxLengthDescription = 250;

    private Tag() { }

    private Tag(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        NormalizedName = NormalizeName(name);
        Description = description;
    }

    public Guid Id { get; }

    public string Name { get; private set; } = null!;

    public string NormalizedName { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public static Result<Tag> Create(
        string name,
        string description)
    {
        var validation = Validate(name, description);
        if (validation.IsFailure)
        {
            return Result.Failure<Tag>(validation.Error);
        }

        return Result.Success(new Tag(Guid.NewGuid(), name.Trim(), description.Trim()));
    }

    public Result Update(string name, string description)
    {
        var validation = Validate(name, description);
        if (validation.IsFailure)
        {
            return validation;
        }

        Name = name.Trim();
        NormalizedName = NormalizeName(name);
        Description = description.Trim();

        return Result.Success();
    }

    private static Result Validate(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > MaxLengthName)
        {
            return Result.Failure("tag.invalid_name");
        }

        if (string.IsNullOrWhiteSpace(description) || description.Length > MaxLengthDescription)
        {
            return Result.Failure("tag.invalid_description");
        }

        return Result.Success();
    }

    private static string NormalizeName(string name) => name.Trim().ToUpperInvariant();
}
