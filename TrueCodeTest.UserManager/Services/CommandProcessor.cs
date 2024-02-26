using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrueCodeTest.UserManager.Data;
using TrueCodeTest.UserManager.Data.Models;
using TrueCodeTest.UserManager.Data.Specifications;
using TrueCodeTest.UserManager.Dto;

namespace TrueCodeTest.UserManager.Services;

public class CommandProcessor
{
    private readonly UserManagerContext _context;

    public CommandProcessor(UserManagerContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Processes a command asynchronously.
    /// </summary>
    /// <param name="command">The command to be processed.</param>
    /// <returns>
    ///     The result of the processing. It can be an instance of <see cref="User" /> or null.
    /// </returns>
    public async Task<object?> ProcessCommandAsync(CommandDto command)
    {
        return command switch
        {
            GetByIdAndDomainCommand c => await _context.Users
                .WithSpecification(new GetUserByIdAndDomainSpec(c.Id, c.Domain))
                .SingleOrDefaultAsync(),

            ListByDomainCommand c => await _context.Users
                .WithSpecification(new GetUsersByDomainPaginatedSpec(c.Domain, c.PageNumber, c.PageSize))
                .ToListAsync(),

            FindByTag c => await _context.Users
                .WithSpecification(new GetUsersByTagAndDomainSpec(c.Domain, c.Tag))
                .ToListAsync(),

            _ => null
        };
    }
}