using RabbitMQ.Client;
using TrueCodeTest.RpcClient.Config;
using TrueCodeTest.RpcClient.Impl;
using TrueCodeTest.RpcClient.Impl.Client;
using TrueCodeTest.RpcClient.Impl.Client.Discovery;
using TrueCodeTest.RpcClient.Impl.Client.Public;
using TrueCodeTest.RpcClient.Impl.Client.Rpc;
using TrueCodeTest.RpcClient.Impl.Server;
using TrueCodeTest.RpcClient.Interfaces;

namespace TrueCodeTest.RpcClient;

public class Hub : IDisposable
{
    private readonly ConnectionFactory _connectionFactory;
    private DiscoveryClient _discoveryClient;
    private RemoteNodeletClient _remoteNodeletClient;

    private Hub(ConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IConnection DefaultConnection { get; private set; }
    public RpcNode DefaultNode { get; private set; }
    
    public INodeletProvider NodeletProvider { get; private set; }

    public void Dispose()
    {
        DefaultNode.Dispose();
    }

    public static Hub Connect(ConnectionFactory connectionFactory)
    {
        var hub = new Hub(connectionFactory);
        hub.Init();
        return hub;
    }

    private void Init()
    {
        DefaultConnection = _connectionFactory.CreateConnection();
        DefaultNode = RpcNode.Create(DefaultConnection, new NodeConfig
        {
            IsAsync = _connectionFactory.DispatchConsumersAsync,
            ChannelNumber = _connectionFactory.ConsumerDispatchConcurrency
        });
        var clientConfig = new ClientConfig
        {
            IsAsync = _connectionFactory.DispatchConsumersAsync,
        };
        _discoveryClient = DiscoveryClient.Create(DefaultConnection.CreateModel(), clientConfig);
        _remoteNodeletClient = RemoteNodeletClient.Create(DefaultConnection.CreateModel(), clientConfig);
        NodeletProvider = new NodeletProvider(_discoveryClient, _remoteNodeletClient);
    }
}