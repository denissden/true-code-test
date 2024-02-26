using RabbitMQ.Client;
using TrueCodeTest.RpcClient.Config;
using TrueCodeTest.RpcClient.Interfaces;

namespace TrueCodeTest.RpcClient.Impl.Server;

public class RpcNode : IDisposable, ISubscriber
{
    private readonly IModel[] _channels;
    private readonly IConnection _connection;
    private readonly NodeConfig _nodeConfig;
    private readonly RpcNodelet[] _nodelets;

    public RpcNode(IConnection connection, NodeConfig nodeConfig)
    {
        _connection = connection;
        _nodeConfig = nodeConfig;
        _channels = new IModel[nodeConfig.ChannelNumber];
        _nodelets = new RpcNodelet[nodeConfig.ChannelNumber];
    }

    public IList<RpcNodelet> Nodelets => _nodelets;
    public bool IsInitialized { get; private set; }

    public void Dispose()
    {
        _connection.Dispose();
    }

    public static RpcNode Create(IConnection connection, NodeConfig nodeConfig)
    {
        var node = new RpcNode(connection, nodeConfig);
        node.Init();
        return node;
    }
    
    public void HandleRpc(string sharedQueue, string topic, Func<ReadOnlyMemory<byte>, CancellationToken, Task<byte[]>> handler)
    {
        foreach (var nodelet in _nodelets)
        {
            nodelet.HandleRpc(sharedQueue, topic, handler);
        }
    }

    /// <summary>
    ///     Initializes the RpcNode by creating channels, infrastructure, and nodelets.
    /// </summary>
    private void Init()
    {
        CreateChannels();
        CreateInfrastructure();
        CreateNodelets();

        IsInitialized = true;
    }

    /// <summary>
    ///     This method creates the channels used by the RPC nodelets.
    /// </summary>
    private void CreateChannels()
    {
        for (var i = 0; i < _nodeConfig.ChannelNumber; i++) _channels[i] = _connection.CreateModel();
    }

    /// <summary>
    ///     Creates the infrastructure for discovering nodes in the RPC system.
    /// </summary>
    private void CreateInfrastructure()
    {
        var firstChannel = _channels.First();
        firstChannel.ExchangeDeclare(_nodeConfig.NodeDiscoveryExchange, ExchangeType.Fanout, false, true);

        firstChannel.ExchangeDeclare(_nodeConfig.RpcExchange, ExchangeType.Direct, false, true);
    }

    /// <summary>
    ///     Creates the nodelets for handling RPC operations on each channel.
    /// </summary>
    private void CreateNodelets()
    {
        for (var i = 0; i < _nodeConfig.ChannelNumber; i++)
        {
            var channel = _channels[i];
            _nodelets[i] = RpcNodelet.Create(channel, _nodeConfig);
        }
    }
}