namespace TrueCodeTest.RpcClient.Config;

public class ClientConfig
{
    public bool IsAsync { get; set; }
    public string ClientName { get; set; } = Constants.DefaultNodeName;

    public string NodeDiscoveryExchange { get; set; } = Constants.NodeDiscoveryExchange;
    public string NodeDiscoveryRequestTopic { get; set; } = Constants.NodeDiscoveryRequestTopic;
    public string NodeDiscoveryResponseTopic { get; set; } = Constants.NodeDiscoveryResponseTopic;
}