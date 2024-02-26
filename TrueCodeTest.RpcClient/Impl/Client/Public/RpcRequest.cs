using TrueCodeTest.RpcClient.Impl.Client.Discovery;
using TrueCodeTest.RpcClient.Impl.Client.Rpc;
using TrueCodeTest.RpcClient.Interfaces;

namespace TrueCodeTest.RpcClient.Impl.Client.Public;

public class RpcRequest : IRemoteMethodHandler
{
    private readonly RpcRequestState _rpcRequestState;
    private readonly RemoteNodeletClient _nodeletClient;
    private readonly DiscoveredNodelet _discoveredNodelet;

    public RpcRequest(RpcRequestState rpcRequestState, RemoteNodeletClient nodeletClient, DiscoveredNodelet discoveredNodelet)
    {
        _rpcRequestState = rpcRequestState;
        _nodeletClient = nodeletClient;
        _discoveredNodelet = discoveredNodelet;
    }

    public async ValueTask<byte[]> GetOutputAsync(CancellationToken? cancellationToken = null)
    {
        if (cancellationToken is not null)
        {
            cancellationToken.Value.Register(() => _rpcRequestState.CompletionSource.TrySetCanceled(cancellationToken.Value), useSynchronizationContext: false);
        }

        var (e, body) = await _rpcRequestState.CompletionSource.Task;
        return body;
    }

    public Task CancelAsync(CancellationToken? cancellationToken = null)
    {
        var cancelTask = _nodeletClient.CancelRpc(
            correlationId: _rpcRequestState.CorrelationId,
            topic: _rpcRequestState.RequestTopic,
            routingKey: _discoveredNodelet.CancelQueue,
            exchange: "");
        
        if (cancellationToken is not null)
        {
            cancellationToken.Value.Register(() => _rpcRequestState.CancelCompletionSource?.TrySetCanceled(),
                useSynchronizationContext: false);
        }

        return cancelTask;
    }
}