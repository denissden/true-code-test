using System.Text.Json;
using TrueCodeTest.StreamReader;
using TrueCodeTest.UserManager.Data.Models;
using TrueCodeTest.UserManager.Dto;

namespace TrueCodeTest.UserManager.Services;

public class CommandStreamProcessor
{
    private readonly CommandProcessor _processor;
    private readonly MessageReader _reader;

    public CommandStreamProcessor(MessageReader reader, CommandProcessor processor)
    {
        _reader = reader;
        _processor = processor;
    }

    /// <summary>
    ///     Processes a stream asynchronously by reading messages from it and handling them using the specified handler
    ///     function.
    /// </summary>
    /// <param name="stream">The stream to read messages from.</param>
    /// <param name="handler">
    ///     The handler function to execute for each message. It takes two parameters: the result of handling
    ///     the command, and a boolean value indicating whether the command was successfully handled or not.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ProcessStreamAsync(Stream stream, Action<object?, bool> handler)
    {
        await foreach (var message in _reader.ReadMessagesAsync(stream))
            try
            {
                var command = JsonSerializer.Deserialize<CommandDto>(message);

                if (command != null)
                {
                    var result = await HandleCommand(command);
                    handler(result, true);
                }
            }
            catch (JsonException e)
            {
                handler(e, false);
            }
    }

    private async Task<object?> HandleCommand(CommandDto command)
    {
        var result = await _processor.ProcessCommandAsync(command);

        return result switch
        {
            User u => UserDto.FromUser(u),
            IEnumerable<User> i => i.Select(UserDto.FromUser).ToList(),
            _ => result
        };
    }
}