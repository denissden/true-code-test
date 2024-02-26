namespace TrueCodeTest.RpcClient.Interfaces;

public interface IRpcNodelet : INodelet
{
    IRemoteMethodHandler Execute(byte[] body, string topic, CancellationToken? cancellationToken = null);
}