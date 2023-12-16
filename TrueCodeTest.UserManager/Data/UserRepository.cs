using Microsoft.EntityFrameworkCore;
using TrueCodeTest.UserManager.Data.Models;

namespace TrueCodeTest.UserManager.Data;

public class UserRepository
{
    private readonly UserManagerContext _context;

    public UserRepository(UserManagerContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAndDomain(Guid userId, string domain)
    {
        return await _context.Users
            .Include(u => u.Tags)
            .ThenInclude(ut => ut.Tag)
            .SingleOrDefaultAsync(u => u.UserId == userId && u.Domain == domain);
    }

    public async Task<List<User>> GetUsersByDomain(string domain, int pageNumber, int pageSize)
    {
        return await _context.Users
            .Include(u => u.Tags)
            .ThenInclude(ut => ut.Tag)
            .Where(u => u.Domain == domain)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<User>> GetUsersByTagAndDomain(string tagValue, string domain)
    {
        return await _context.Users
            .Include(u => u.Tags)
            .ThenInclude(ut => ut.Tag)
            .Where(u => u.Domain == domain && u.Tags.Any(ut => ut.Tag.Value == tagValue))
            .ToListAsync();
    }
}