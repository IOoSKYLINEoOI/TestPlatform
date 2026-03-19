using CSharpFunctionalExtensions;
using TestPlatform.Core.Users;

namespace TestPlatform.Application.Users;

public interface IUsersRepository
{
    Task<Result<Guid>> AddAsync(User user,  CancellationToken cancellationToken);
}