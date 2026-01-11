using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Models.Category;

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

    public string Name { get; }

    public string Description { get; }

    public static Result<Category> Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > MaxLengthName)
        {
            return Result.Failure<Category>($"'{nameof(name)}' не может быть null или пустым, длиннее чем {MaxLengthName} символов.");
        }

        if (string.IsNullOrWhiteSpace(description) || description.Length > MaxLengthDescription)
        {
            return Result.Failure<Category>($"'{nameof(description)}' не может быть null или пустым, длиннее чем {MaxLengthDescription} символов.");
        }

        var category = new Category(Guid.NewGuid(), name, description);

        return Result.Success(category);
    }

    public static Category FromPersistence(Guid id, string name, string description)
    {
        return new Category(id, name, description);
    }
}