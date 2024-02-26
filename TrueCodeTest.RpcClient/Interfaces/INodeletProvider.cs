using TrueCodeTest.RpcClient.Impl.Client.Public;

namespace TrueCodeTest.RpcClient.Interfaces;

public interface INodeletProvider
{
    Task<IRpcNodelet> GetNodelet(string[] topics, TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);
}