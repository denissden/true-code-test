using System.Buffers;
using System.Runtime.CompilerServices;

namespace TrueCodeTest.StreamReader;

public class MessageReader(
    byte delimiter,
    int bufferSize = 8192,
    bool lastChunkIsMessage = true,
    bool allowEmptyMessages = false)
{
    private readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;
    private readonly MemoryStream _memoryStream = new();

    /// <summary>
    ///     Asynchronously reads messages from a stream until the end of the stream is reached or cancellation is requested.
    /// </summary>
    /// <param name="stream">The stream from which to read messages.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>An asynchronous enumerable sequence of byte arrays, each representing a message.</returns>
    public async IAsyncEnumerable<byte[]> ReadMessagesAsync(Stream stream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var buffer = _bufferPool.Rent(bufferSize);
        var messageBuffer = new List<byte>();
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            var messageStart = 0;

            for (var i = 0; i < bytesRead; i++)
                if (buffer[i] == delimiter)
                {
                    var count = i - messageStart;
                    if (count != 0 || allowEmptyMessages)
                    {
                        messageBuffer.AddRange(new ArraySegment<byte>(buffer, messageStart, count));
                        yield return messageBuffer.ToArray();
                        messageBuffer.Clear();
                    }

                    messageStart = i + 1;
                }

            if (messageStart < bytesRead)
                messageBuffer.AddRange(new ArraySegment<byte>(buffer, messageStart, bytesRead - messageStart));
        }

        if (messageBuffer.Count > 0 && lastChunkIsMessage) yield return messageBuffer.ToArray();
    }
}