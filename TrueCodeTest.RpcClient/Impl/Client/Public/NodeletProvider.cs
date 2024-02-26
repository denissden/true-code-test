using TrueCodeTest.RpcClient.Impl.Client.Discovery;
using TrueCodeTest.RpcClient.Impl.Client.Rpc;
using TrueCodeTest.RpcClient.Interfaces;

namespace TrueCodeTest.RpcClient.Impl.Client.Public;

public class NodeletProvider : INodeletProvider
{
    private readonly DiscoveryClient _discoveryClient;
    private readonly RemoteNodeletClient _remoteNodeletClient;

    public NodeletProvider(DiscoveryClient discoveryClient, RemoteNodeletClient remoteNodeletClient)
    {
        _discoveryClient = discoveryClient;
        _remoteNodeletClient = remoteNodeletClient;
    }

    public async Task<IRpcNodelet> GetNodelet(string[] topics, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var discoveredNodelet = await _discoveryClient.DiscoverOneNodelet(topics, timeout, cancellationToken);

        return new Nodelet(discoveredNodelet, _remoteNodeletClient);
    }
}