namespace TrueCodeTest.RpcClient;

public static class Constants
{
    public const string NodeDiscoveryExchange = "__node_discovery__";
    public const string RpcExchange = "__rpc__";


    public const string NodeDiscoveryRequestTopic = "nodeDiscovery.req";
    public const string NodeDiscoveryResponseTopic = "nodeDiscovery.rsp";

    public const string DefaultNodeName = "default";


    public const string HeaderTopic = "x-topic";
    public const string HeaderDiscoveryQueue = "x-discovery-queue";
    public const string HeaderNodeName = "x-node-name";
    public const string HeaderNodelet = "x-nodelet";
    public const string HeaderRpcQueue = "x-rpc-queue";
    public const string HeaderCancelQueue = "x-cancel-queue";
    public const string HeaderClientName = "x-client-name";
    public const string HeaderClientId = "x-client-id";


    public const string HeaderRpcMessageType = "x-rpc-message-type";
    public const string HeaderStatusCode = "x-status-code";
    public const string HeaderTimeout = "x-timeout";

    public static class RpcMessageTypes
    {
        public const string Request = "request";
        public const string Response = "response";
        public const string Cancel = "cancel";
        public const string CancelResponse = "cancel_response";
    }

    public static class StatusCodes
    {
        public const int Ok = 200;
        public const int NotFound = 404;
        public const int Conflict = 409;
        public const int RequestTimeout = 408;
        public const int ServerError = 500;
    }
}