using TrueCodeTest.RpcClient.Impl.Client.Discovery;
using TrueCodeTest.RpcClient.Impl.Client.Rpc;
using TrueCodeTest.RpcClient.Interfaces;

namespace TrueCodeTest.RpcClient.Impl.Client.Public;

public class RpcRequest : IRemoteMethodHandler
{
    private readonly DiscoveredNodelet _discoveredNodelet;
    private readonly RemoteNodeletClient _nodeletClient;
    private readonly RpcRequestState _rpcRequestState;

    public RpcRequest(RpcRequestState rpcRequestState, RemoteNodeletClient nodeletClient,
        DiscoveredNodelet discoveredNodelet)
    {
        _rpcRequestState = rpcRequestState;
        _nodeletClient = nodeletClient;
        _discoveredNodelet = discoveredNodelet;
    }

    public async ValueTask<byte[]> GetOutputAsync(CancellationToken? cancellationToken = null)
    {
        if (cancellationToken is not null)
            cancellationToken.Value.Register(
                () => _rpcRequestState.CompletionSource.TrySetCanceled(cancellationToken.Value), false);

        var (e, body) = await _rpcRequestState.CompletionSource.Task;
        return body;
    }

    public Task CancelAsync(CancellationToken? cancellationToken = null)
    {
        var cancelTask = _nodeletClient.CancelRpc(
            _rpcRequestState.CorrelationId,
            _rpcRequestState.RequestTopic,
            _discoveredNodelet.CancelQueue,
            "");

        if (cancellationToken is not null)
            cancellationToken.Value.Register(() => _rpcRequestState.CancelCompletionSource?.TrySetCanceled(),
                false);

        return cancelTask;
    }
}