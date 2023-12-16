using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrueCodeTest.StreamReader;
using TrueCodeTest.UserManager.Data;
using TrueCodeTest.UserManager.Data.Helpers;
using TrueCodeTest.UserManager.Services;

// Create services
var services = new ServiceCollection();

services.AddDbContext<UserManagerContext>(opt =>
    opt.UseSqlite(Environment.GetEnvironmentVariable("SQLITE_CONNECTION")));

services.AddScoped<UserRepository>();
services.AddScoped<CommandProcessor>();
services.AddScoped<CommandStreamProcessor>();
services.AddScoped<MessageReader>(_ => new MessageReader((byte)'\n'));

// Prepare
var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();
await DatabaseHelper.EnsureDbCreatedAndSeedWithCountOfAsync(scope.ServiceProvider);

// Run
var processor = serviceProvider.GetRequiredService<CommandStreamProcessor>();
await processor.ProcessStreamAsync(Console.OpenStandardInput(), PrintResult);

void PrintResult(object? result, bool success)
{
    if (success)
        Console.WriteLine(JsonSerializer.Serialize(result));
    else
        Console.WriteLine("Error: " + result);
}