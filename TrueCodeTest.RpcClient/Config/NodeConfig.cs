namespace TrueCodeTest.RpcClient.Config;

public class NodeConfig
{
    public bool IsAsync { get; set; }
    public int ChannelNumber { get; set; } = 1;
    public ushort NodeletPrefetchCount { get; set; } = 10;

    // NODE
    public string NodeName { get; set; } = Constants.DefaultNodeName;
    public string NodeDiscoveryExchange { get; set; } = Constants.NodeDiscoveryExchange;
    public string NodeDiscoveryRequestTopic { get; set; } = Constants.NodeDiscoveryRequestTopic;
    public string NodeDiscoveryResponseTopic { get; set; } = Constants.NodeDiscoveryResponseTopic;

    // RPC
    public string RpcExchange { get; set; } = Constants.RpcExchange;
}