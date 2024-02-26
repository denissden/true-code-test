using TrueCodeTest.RpcClient.Impl.Client.Discovery;
using TrueCodeTest.RpcClient.Impl.Client.Rpc;
using TrueCodeTest.RpcClient.Interfaces;

namespace TrueCodeTest.RpcClient.Impl.Client.Public;

public class Nodelet : IRpcNodelet
{
    private readonly DiscoveredNodelet _discoveredNodelet;
    private readonly RemoteNodeletClient _remoteNodeletClient;

    public Nodelet(DiscoveredNodelet discoveredNodelet, RemoteNodeletClient remoteNodeletClient)
    {
        _discoveredNodelet = discoveredNodelet;
        _remoteNodeletClient = remoteNodeletClient;
    }

    public IRemoteMethodHandler Execute(byte[] body, string topic, CancellationToken? cancellationToken = null)
    {
        var rpcRequestState = _remoteNodeletClient.ExecuteRpc(body, topic, _discoveredNodelet.RpcQueue, "");
        cancellationToken?.Register(() =>
                _remoteNodeletClient.CancelRpc(rpcRequestState.CorrelationId, topic, _discoveredNodelet.CancelQueue,
                    ""),
            false
        );

        return new RpcRequest(rpcRequestState, _remoteNodeletClient, _discoveredNodelet);
    }

    public string Id => _discoveredNodelet.Nodelet;
    public IEnumerable<string> SupportedTopics => _discoveredNodelet.SupportedTopics;
}