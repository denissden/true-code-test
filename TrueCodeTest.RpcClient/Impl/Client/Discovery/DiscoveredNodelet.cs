using RabbitMQ.Client.Events;
using TrueCodeTest.RpcClient.Contracts;

namespace TrueCodeTest.RpcClient.Impl.Client.Discovery;

public class DiscoveredNodelet
{
    public string[] SupportedTopics { get; set; }
    public string Nodelet { get; set; }
    public string NodeName { get; set; }
    public string RpcQueue { get; set; }
    public string CancelQueue { get; set; }
    public string DiscoveryQueue { get; set; }

    public static DiscoveredNodelet FromEvent(BasicDeliverEventArgs e)
    {
        var contractResponse = NodeDiscovery.Response.Parse(e.Body);
        return new DiscoveredNodelet
        {
            SupportedTopics = contractResponse.Topics,
            Nodelet = e.BasicProperties.Headers.GetStringHeader(Constants.HeaderNodelet),
            NodeName = e.BasicProperties.Headers.GetStringHeader(Constants.HeaderNodeName),
            RpcQueue = e.BasicProperties.Headers.GetStringHeader(Constants.HeaderRpcQueue),
            CancelQueue = e.BasicProperties.Headers.GetStringHeader(Constants.HeaderCancelQueue),
            DiscoveryQueue = e.BasicProperties.Headers.GetStringHeader(Constants.HeaderDiscoveryQueue)
        };
    }
}