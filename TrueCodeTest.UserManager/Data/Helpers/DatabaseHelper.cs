using Microsoft.Extensions.DependencyInjection;
using TrueCodeTest.UserManager.Data.Models;

namespace TrueCodeTest.UserManager.Data.Helpers;

public class DatabaseHelper
{
    /// <summary>
    ///     Ensures that the database is created and seeded with the specified count of users asynchronously.
    /// </summary>
    /// <param name="sp">The service provider.</param>
    /// <param name="countOfUsers">The number of users to seed the database with (default is 500).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task EnsureDbCreatedAndSeedWithCountOfAsync(IServiceProvider sp, int countOfUsers = 500)
    {
        using var context = sp.GetRequiredService<UserManagerContext>();

        if (await context.Database.EnsureCreatedAsync()) await DataSeed(context, countOfUsers);
    }

    public static async Task DataSeed(UserManagerContext context, int countOfUsers)
    {
        string[] names =
        {
            "Olivia", "Emma", "Ava", "Sophia", "Isabella",
            "Charlotte", "Amelia", "Mia", "Harper", "Evelyn",
            "Abigail", "Emily", "Ella", "Elizabeth", "Camila",
            "Luna", "Sofia", "Avery", "Mila", "Aria"
        };

        string[] surnames =
        {
            "Smith", "Johnson", "Williams", "Brown", "Jones",
            "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
            "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
            "Thomas", "Taylor", "Moore", "Jackson", "Martin"
        };

        string[] domains =
        {
            "Technology", "Health", "Finance", "Education", "Entertainment"
        };

        string[] tags =
        {
            "Innovation", "Wellness", "Technology", "Learning", "Movies",
            "Software", "Fitness", "Economy", "Science", "Music"
        };

        var rnd = new Random();

        var tagModels = tags.Select(t => new Tag
        {
            Domain = domains.OrderBy(x => rnd.Next()).First(),
            Value = t
        });
        await context.AddRangeAsync(tagModels);

        var userModels = Enumerable.Range(1, countOfUsers).Select(t => new User
        {
            Domain = domains.OrderBy(x => rnd.Next()).First(),
            Name = names.OrderBy(x => rnd.Next()).First() + " " + surnames.OrderBy(x => rnd.Next()).First()
        });
        await context.AddRangeAsync(userModels);

        var userTags = userModels.SelectMany(
            u => Enumerable.Range(1, rnd.Next(1, 5))
                .Select(_ => new TagToUser
                {
                    User = u,
                    Tag = tagModels.OrderBy(x => rnd.Next()).First()
                })
        );
        await context.AddRangeAsync(userTags);

        await context.SaveChangesAsync();
    }
}