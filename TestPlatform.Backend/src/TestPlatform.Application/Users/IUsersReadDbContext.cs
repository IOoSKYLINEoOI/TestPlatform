using TestPlatform.Core.Users;

namespace TestPlatform.Application.Users;

public interface IUsersReadDbContext
{
    IQueryable<User> ReadUsers { get; }
}