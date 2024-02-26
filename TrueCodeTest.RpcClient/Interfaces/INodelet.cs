namespace TrueCodeTest.RpcClient.Interfaces;

public interface INodelet
{
    public string Id { get; }
    public IEnumerable<string> SupportedTopics { get; }
}