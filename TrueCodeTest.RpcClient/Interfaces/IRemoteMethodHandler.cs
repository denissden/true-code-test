namespace TrueCodeTest.RpcClient.Interfaces;

public interface IRemoteMethodHandler
{
    ValueTask<byte[]> GetOutputAsync(CancellationToken? cancellationToken = null);
    Task CancelAsync(CancellationToken? cancellationToken = null);
}