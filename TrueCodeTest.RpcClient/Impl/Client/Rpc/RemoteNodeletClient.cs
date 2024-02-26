using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TrueCodeTest.RpcClient.Config;
using TrueCodeTest.RpcClient.Exceptions;

namespace TrueCodeTest.RpcClient.Impl.Client.Rpc;

public class RemoteNodeletClient
{
    private readonly IModel _channel;
    private readonly ClientConfig _clientConfig;

    private readonly Dictionary<string, RpcRequestState> _correlationIdToRequest = new();

    private IBasicConsumer _replyToConsumer;
    private string _replyToQueue;

    public RemoteNodeletClient(IModel channel, ClientConfig clientConfig)
    {
        _channel = channel;
        _clientConfig = clientConfig;
        Id = Guid.NewGuid();
    }

    public Guid Id { get; init; }

    public static RemoteNodeletClient Create(IModel channel, ClientConfig clientConfig)
    {
        var client = new RemoteNodeletClient(channel, clientConfig);
        client.Init();
        return client;
    }

    public RpcRequestState ExecuteRpc(byte[] body, string topic, string routingKey, string exchange)
    {
        var request = new RpcRequestState();

        Guid correlationId;
        do
        {
            correlationId = Guid.NewGuid();
        } while (!_correlationIdToRequest.TryAdd(correlationId.ToString(), request));

        request.CorrelationId = correlationId.ToString();
        request.RequestTopic = topic;

        var props = _channel.CreateBasicProperties();
        props.CorrelationId = correlationId.ToString();
        props.ReplyTo = _replyToQueue;
        props.Headers = GetHeaders(topic, Constants.RpcMessageTypes.Request);

        _channel.BasicPublish(exchange, routingKey, true, props, body);
        request.StartRequest();

        return request;
    }

    public Task CancelRpc(string correlationId, string topic, string routingKey, string exchange)
    {
        var request = _correlationIdToRequest.GetValueOrDefault(correlationId);
        if (request is null)
            return Task.CompletedTask;

        var props = _channel.CreateBasicProperties();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyToQueue;
        props.Headers = GetHeaders(topic, Constants.RpcMessageTypes.Cancel);

        _channel.BasicPublish(exchange, routingKey, true, props, new ReadOnlyMemory<byte>());
        request.StartCancelRequest();

        return request.CancelCompletionSource!.Task;
    }

    public void Kill(string correlationId)
    {
        var request = _correlationIdToRequest.GetValueOrDefault(correlationId);
        if (request is null)
            throw new ArgumentException($"No active request with correlationId={correlationId}");

        request.SetCancelled();
    }

    private Dictionary<string, object> GetHeaders(string topic, string messageType)
    {
        return new Dictionary<string, object>
        {
            [Constants.HeaderTopic] = topic,
            [Constants.HeaderRpcMessageType] = messageType,
            [Constants.HeaderClientName] = _clientConfig.ClientName,
            [Constants.HeaderClientId] = Id.ToString()
        };
    }

    private void Init()
    {
        InitResponseQueue();
        InitUnroutableHandler();
    }

    private void InitResponseQueue()
    {
        if (_clientConfig.IsAsync)
        {
            var asyncConsumer = new AsyncEventingBasicConsumer(_channel);
            asyncConsumer.Received += (sender, @event) =>
                Task.Factory.StartNew(() => OnResponseReceived(sender, @event));
            _replyToConsumer = asyncConsumer;
        }
        else
        {
            var basicConsumer = new EventingBasicConsumer(_channel);
            basicConsumer.Received += OnResponseReceived;
            _replyToConsumer = basicConsumer;
        }

        var rpcQueueDeclareOk = _channel.QueueDeclare();
        _replyToQueue = rpcQueueDeclareOk.QueueName;
        _channel.BasicConsume(_replyToQueue, false, _replyToConsumer);
    }

    private void InitUnroutableHandler()
    {
        _channel.ConfirmSelect();
        _channel.BasicReturn += (sender, args) =>
        {
            var correlationId = args.BasicProperties.CorrelationId;
            var messageType = args.BasicProperties.Headers.GetStringHeader(Constants.HeaderRpcMessageType);

            if (_correlationIdToRequest.TryGetValue(correlationId, out var request))
            {
                if (messageType == Constants.RpcMessageTypes.Request)
                    request.SetException(new RpcException("The request was unroutable"));
                else if (messageType == Constants.RpcMessageTypes.Cancel)
                    request.SetCancelException(new RpcException("The request was unroutable"));
            }
        };
    }

    private void OnResponseReceived(object? sender, BasicDeliverEventArgs e)
    {
        var consumer = sender as IBasicConsumer;
        Debug.Assert(consumer is not null);

        var messageType = e.BasicProperties.Headers.GetStringHeader(Constants.HeaderRpcMessageType);

        var isResultSuccess = false;
        if (messageType == Constants.RpcMessageTypes.Response)
            isResultSuccess = HandleRpcResponse(e);
        else if (messageType == Constants.RpcMessageTypes.CancelResponse)
            isResultSuccess = HandleCancellationResponse(e);

        if (isResultSuccess)
            consumer.Model.BasicAck(e.DeliveryTag, false);
        else
            consumer.Model.BasicNack(e.DeliveryTag, false, false);
    }

    private bool HandleRpcResponse(BasicDeliverEventArgs e)
    {
        var request = _correlationIdToRequest.GetValueOrDefault(e.BasicProperties.CorrelationId);
        if (request is null) return false;

        var responseStatusCode =
            e.BasicProperties.Headers.GetIntHeader(Constants.HeaderStatusCode);

        switch (responseStatusCode)
        {
            case Constants.StatusCodes.Ok:
                request.SetCompleted(e);
                break;
            case Constants.StatusCodes.ServerError:
                request.SetException(
                    new RpcException(Encoding.UTF8.GetString(e.Body.Span)));
                break;
            default:
                request.SetException(
                    new RpcException($"Response status code {responseStatusCode} does not indicate success"));
                break;
        }

        if (request.IsFinishedState) _correlationIdToRequest.Remove(e.BasicProperties.CorrelationId);
        return true;
    }

    private bool HandleCancellationResponse(BasicDeliverEventArgs e)
    {
        var request = _correlationIdToRequest.GetValueOrDefault(e.BasicProperties.CorrelationId);
        if (request is null) return false;

        var responseStatusCode =
            e.BasicProperties.Headers.GetIntHeader(Constants.HeaderStatusCode);

        if (responseStatusCode == Constants.StatusCodes.Ok)
            request.SetCancelled();
        else
            request.SetCancelException(
                new RpcException($"Cancel response status code {responseStatusCode} does not indicate success"));

        if (request.IsFinishedState) _correlationIdToRequest.Remove(e.BasicProperties.CorrelationId);

        return true;
    }
}