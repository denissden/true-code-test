using Microsoft.EntityFrameworkCore;
using TrueCodeTest.UserManager.Data.Models;

namespace TrueCodeTest.UserManager.Data;

public class UserManagerContext : DbContext
{
    public DbSet<Tag> Tags { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<TagToUser> UserTags { get; set; }
    
    public UserManagerContext(DbContextOptions<UserManagerContext> options) : base(options)
    {
        
    }
}