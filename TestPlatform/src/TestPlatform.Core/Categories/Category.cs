using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Categories;

public class Category
{
    private const int MaxLengthName = 100;
    private const int MaxLengthDescription = 250;

    private Category(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public static Result<Category> Create(string name, string description)
    {
        var validation = Validate(name, description);
        if (validation.IsFailure)
            return Result.Failure<Category>(validation.Error);

        var category = new Category(Guid.NewGuid(), name, description);

        return Result.Success(category);
    }

    public static Result<Category> CreateWithId(Guid id, string name, string description)
    {
        var validation = Validate(name, description);
        if (validation.IsFailure)
            return Result.Failure<Category>(validation.Error);

        var category = new Category(id, name, description);

        return Result.Success(category);
    }

    public static Category FromPersistence(Guid id, string name, string description)
    {
        return new Category(id, name, description);
    }

    private static Result Validate(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > MaxLengthName)
        {
            return Result.Failure($"'{nameof(name)}' не может быть null или пустым, длиннее чем {MaxLengthName} символов.");
        }

        if (string.IsNullOrWhiteSpace(description) || description.Length > MaxLengthDescription)
        {
            return Result.Failure($"'{nameof(description)}' не может быть null или пустым, длиннее чем {MaxLengthDescription} символов.");
        }

        return Result.Success();
    }
}