# MessageReader

## Overview
The `TrueCodeTest.StreamReader` namespace contains the `MessageReader` class, designed to asynchronously read and parse messages from a stream, separated by a byte delimiter.

## Features
- **Delimiter-based Message Parsing:** Messages in the stream are separated using a specified byte delimiter.
- **Buffer Management:** Efficient handling of memory using buffer pooling.
- **Configurable Buffer Size:** Allows specifying the size of the buffer for stream reading.
- **Handling Partial and Empty Messages:** Options to treat the last chunk of data as a complete message and to allow/disallow empty messages.

## Usage
To use the `MessageReader` class, follow these steps:

1. **Instantiate `MessageReader`:**
   Create an instance of `MessageReader` by specifying the delimiter and other optional parameters like `bufferSize`, `lastChunkIsMessage`, and `allowEmptyMessages`.

   Example:
   ```csharp
   var messageReader = new MessageReader((byte)',', 
       bufferSize: 16*1024,
       lastChunkIsMessage: true, 
       allowEmptyMessages: true);
   ```

2. **Read Messages Asynchronously:**
   Use `ReadMessagesAsync` method to asynchronously read messages from a given stream. This method returns an asynchronous enumerable of byte arrays, each representing a message.

   Example:
   ```csharp
   await foreach (var message in messageReader.ReadMessagesAsync(stream))
   {
       // Process each message
   }
   ```

3. **Handling Stream Data:**
   The `ReadMessagesAsync` method will continue reading messages from the stream until the end of the stream is reached or the operation is canceled.

## Example
A sample usage is demonstrated for reading messages from files:

```csharp
await ReadMessagesFromFile("../../../test1");
await ReadMessagesFromFile("../../../test2");
await ReadMessagesFromFile("../../../test3");

async Task ReadMessagesFromFile(string path)
{
    using var sr = new StreamReader(path);
    await foreach (var message in messageReader.ReadMessagesAsync(sr.BaseStream))
    {
        Console.WriteLine(Encoding.UTF8.GetString(message));
    }
}
```

## Dependencies
- .NET Core 8.0
