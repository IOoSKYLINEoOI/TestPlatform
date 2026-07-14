using TestPlatform.Core.Users;

namespace TestPlatform.Application.Users;

public interface IUsersRepository
{
    Task AddAsync(User user,  CancellationToken cancellationToken);
}