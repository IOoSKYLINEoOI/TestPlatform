using TestPlatform.Application.Users;
using TestPlatform.Core.Users;

namespace TestPlatform.Infrastructure.Postgres.Users;

public class UsersRepository : IUsersRepository
{
    private readonly TestPlatformDbContext _context;

    public UsersRepository(TestPlatformDbContext context) => _context = context;

    public async Task AddAsync(User user, CancellationToken cancellationToken)
        => await _context.Users.AddAsync(user, cancellationToken);

    public void Detach(User user) => _context.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
}
