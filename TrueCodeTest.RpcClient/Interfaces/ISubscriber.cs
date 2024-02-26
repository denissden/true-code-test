namespace TrueCodeTest.RpcClient.Interfaces;

public interface ISubscriber
{
    void HandleRpc(string sharedQueue, string topic,
        Func<ReadOnlyMemory<byte>, CancellationToken, Task<byte[]>> handler);
}