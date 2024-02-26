// See https://aka.ms/new-console-template for more information

using System.Text;
using TrueCodeTest.StreamReader;

var messageReader = new MessageReader((byte)',',
    16 * 1024,
    true,
    true);

await ReadMessagesFromFile("../../../test1");
await ReadMessagesFromFile("../../../test2");
await ReadMessagesFromFile("../../../test3");

async Task ReadMessagesFromFile(string path)
{
    using var sr = new StreamReader(path);
    await foreach (var message in messageReader.ReadMessagesAsync(sr.BaseStream))
        Console.WriteLine(Encoding.UTF8.GetString(message));
}