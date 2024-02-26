using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TrueCodeTest.RpcClient.Config;
using TrueCodeTest.RpcClient.Contracts;
using TrueCodeTest.RpcClient.Interfaces;

namespace TrueCodeTest.RpcClient.Impl.Server;

public class RpcNodelet : ISubscriber
{
    private readonly IModel _channel;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _correlationIdToCancellation = new();
    private readonly NodeConfig _nodeConfig;

    private readonly Dictionary<string, Func<ReadOnlyMemory<byte>, CancellationToken, Task<byte[]>>>
        _topicToHandler = new();

    private IBasicConsumer _discoveryConsumer;
    private string _discoveryQueue;
    private IBasicConsumer _rpcCancelConsumer;
    private string _rpcCancelQueue;

    private IBasicConsumer _rpcConsumer;
    private string _rpcQueue;

    public RpcNodelet(IModel channel, NodeConfig nodeConfig)
    {
        _channel = channel;
        _nodeConfig = nodeConfig;
        Id = Guid.NewGuid();
    }

    public Guid Id { get; init; }
    public bool IsInitialized { get; private set; }
    public IEnumerable<string> SubscribedTopics => _topicToHandler.Keys.ToArray();

    public void HandleRpc(string sharedQueue, string topic,
        Func<ReadOnlyMemory<byte>, CancellationToken, Task<byte[]>> handler)
    {
        _topicToHandler[topic] = handler;
        _channel.QueueBind(sharedQueue, _nodeConfig.RpcExchange, topic);
        _channel.BasicConsume(sharedQueue, false, _rpcConsumer);
    }

    public static RpcNodelet Create(IModel channel, NodeConfig nodeConfig)
    {
        var nodelet = new RpcNodelet(channel, nodeConfig);
        nodelet.Init();
        return nodelet;
    }

    private void Init()
    {
        // https://www.rabbitmq.com/docs/consumer-prefetch
        // applied separately to each new consumer on the channel
        // should have no problems with cancel queue
        _channel.BasicQos(0, _nodeConfig.NodeletPrefetchCount, false);
        InitDiscovery();
        InitRpc();
        InitRpcCancel();
        IsInitialized = true;
    }

    private Dictionary<string, object> GetDefaultHeaders(string? topic)
    {
        var headers = new Dictionary<string, object>
        {
            [Constants.HeaderNodeName] = _nodeConfig.NodeName,
            [Constants.HeaderNodelet] = Id.ToString(),
            [Constants.HeaderDiscoveryQueue] = _discoveryQueue,
            [Constants.HeaderRpcQueue] = _rpcQueue,
            [Constants.HeaderCancelQueue] = _rpcCancelQueue
        };

        if (topic is not null) headers.Add(Constants.HeaderTopic, topic);

        return headers;
    }

    private IBasicProperties GetDefaultProperties(IModel channel, BasicDeliverEventArgs e, string? topic)
    {
        var props = channel.CreateBasicProperties();
        props.CorrelationId = e.BasicProperties.CorrelationId;
        props.Headers = GetDefaultHeaders(topic);
        return props;
    }

    #region discovery internal

    private void InitDiscovery()
    {
        // Should have different consumer for async connection mode,
        // otherwise it will error
        if (_nodeConfig.IsAsync)
        {
            var asyncConsumer = new AsyncEventingBasicConsumer(_channel);
            asyncConsumer.Received += (sender, @event) =>
                Task.Factory.StartNew(() => OnDiscoveryReceived(sender, @event));
            _discoveryConsumer = asyncConsumer;
        }
        else
        {
            var basicConsumer = new EventingBasicConsumer(_channel);
            basicConsumer.Received += OnDiscoveryReceived;
            _discoveryConsumer = basicConsumer;
        }

        // default arguments are the ones we need
        // "", durable: false, autoDelete: true, exclusive: true
        var queueDeclareOk = _channel.QueueDeclare();
        _discoveryQueue = queueDeclareOk.QueueName;
        // topic is redundant due to exchange being fanout 
        _channel.QueueBind(_discoveryQueue, _nodeConfig.NodeDiscoveryExchange, _nodeConfig.NodeDiscoveryRequestTopic);
        // autoAck: true
        // if the node doesn't respond it means something is broken
        _channel.BasicConsume(_discoveryQueue, true, _discoveryConsumer);
    }

    private void OnDiscoveryReceived(object? sender, BasicDeliverEventArgs e)
    {
        var consumer = sender as IBasicConsumer;
        Debug.Assert(consumer is not null);

        // we only process node discovery
        if (e.RoutingKey != _nodeConfig.NodeDiscoveryRequestTopic &&
            e.BasicProperties.Headers.GetStringHeader(Constants.HeaderTopic) != _nodeConfig.NodeDiscoveryRequestTopic)
            return;

        // only respond if nodelet consumes one of requested topics
        var request = NodeDiscovery.Request.Parse(e.Body);
        if (!SubscribedTopics.Any(request.Topics.Contains))
            return;

        var response = new NodeDiscovery.Response
        {
            Topics = SubscribedTopics.ToArray()
        };
        var properties = GetDefaultProperties(consumer.Model, e, _nodeConfig.NodeDiscoveryResponseTopic);
        consumer.Model.BasicPublish("", e.BasicProperties.ReplyTo ?? string.Empty, body: response.Serialize(),
            basicProperties: properties);
    }

    #endregion

    #region rpc internal

    private void InitRpc()
    {
        if (_nodeConfig.IsAsync)
        {
            var asyncConsumer = new AsyncEventingBasicConsumer(_channel);
            asyncConsumer.Received += OnRpcCommandReceived;
            _rpcConsumer = asyncConsumer;
        }
        else
        {
            var basicConsumer = new EventingBasicConsumer(_channel);
            basicConsumer.Received += (sender, @event) => OnRpcCommandReceived(sender, @event).GetAwaiter().GetResult();
            _rpcConsumer = basicConsumer;
        }

        var rpcQueueDeclareOk = _channel.QueueDeclare();
        _rpcQueue = rpcQueueDeclareOk.QueueName;
        _channel.BasicConsume(_rpcQueue, false, _rpcConsumer);
    }

    private void InitRpcCancel()
    {
        if (_nodeConfig.IsAsync)
        {
            var asyncConsumer = new AsyncEventingBasicConsumer(_channel);
            asyncConsumer.Received += (sender, @event) =>
                Task.Factory.StartNew(() => OnRpcCancelReceived(sender, @event));
            _rpcCancelConsumer = asyncConsumer;
        }
        else
        {
            var basicConsumer = new EventingBasicConsumer(_channel);
            basicConsumer.Received += OnRpcCancelReceived;
            _rpcCancelConsumer = basicConsumer;
        }

        var cancelQueueDeclareOk = _channel.QueueDeclare();
        _rpcCancelQueue = cancelQueueDeclareOk.QueueName;
        _channel.BasicConsume(_rpcCancelQueue, false, _rpcCancelConsumer);
    }

    private void OnRpcCancelReceived(object? sender, BasicDeliverEventArgs e)
    {
        var consumer = sender as IBasicConsumer;
        Debug.Assert(consumer is not null);

        // should not happen!
        if (e.BasicProperties.Headers.GetStringHeader(Constants.HeaderRpcMessageType) !=
            Constants.RpcMessageTypes.Cancel)
        {
            consumer.Model.BasicReject(e.DeliveryTag, false);
            return;
        }

        int statusCode;
        if (_correlationIdToCancellation.TryGetValue(e.BasicProperties.CorrelationId, out var cts))
        {
            if (!cts.IsCancellationRequested)
                try
                {
                    cts.Cancel();

                    // cancelled successfully
                    statusCode = Constants.StatusCodes.Ok;
                }
                catch (OperationCanceledException canceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    // error while cancelling, result unknown
                    statusCode = Constants.StatusCodes.ServerError;
                }
            else
                // trying to cancel already cancelled task
                statusCode = Constants.StatusCodes.Conflict;
        }
        else
        {
            // no task with suck correlation_id
            statusCode = Constants.StatusCodes.NotFound;
        }

        var properties = GetDefaultProperties(consumer.Model, e, null);
        properties.Headers.Add(Constants.HeaderRpcMessageType, Constants.RpcMessageTypes.CancelResponse);
        properties.Headers.Add(Constants.HeaderStatusCode, statusCode);
        consumer.Model.BasicPublish("", e.BasicProperties.ReplyTo, basicProperties: properties);

        consumer.Model.BasicAck(e.DeliveryTag, false);
    }

    private async Task OnRpcCommandReceived(object? sender, BasicDeliverEventArgs e)
    {
        var consumer = sender as IBasicConsumer;
        Debug.Assert(consumer is not null);

        var topic = e.RoutingKey;
        var headerTopic = e.BasicProperties.Headers.GetStringHeader(Constants.HeaderTopic) ?? string.Empty;
        Func<ReadOnlyMemory<byte>, CancellationToken, Task<byte[]>> handler;

        int statusCode;
        byte[] body = [];
        if (_topicToHandler.TryGetValue(topic, out handler) ||
            _topicToHandler.TryGetValue(headerTopic, out handler))
        {
            using var cts = new CancellationTokenSource();
            _correlationIdToCancellation.TryAdd(e.BasicProperties.CorrelationId, cts);

            if (e.BasicProperties.Headers.GetIntHeader(Constants.HeaderTimeout) is { } timeout and > 0)
                cts.CancelAfter(timeout);

            try
            {
                var task = handler(e.Body, cts.Token);
                body = await task.ConfigureAwait(false);

                statusCode = Constants.StatusCodes.Ok;
            }
            catch (OperationCanceledException canceledException)
            {
                Console.WriteLine("Cancelled");
                throw;
            }
            catch (Exception exception)
            {
                body = Encoding.UTF8.GetBytes(exception.Message + "\n\n" + exception.StackTrace);
                statusCode = Constants.StatusCodes.ServerError;
            }
            finally
            {
                _correlationIdToCancellation.TryRemove(e.BasicProperties.CorrelationId, out _);
            }
        }
        else
        {
            statusCode = Constants.StatusCodes.NotFound;
        }

        var properties = GetDefaultProperties(consumer.Model, e, headerTopic);
        properties.Headers.Add(Constants.HeaderRpcMessageType, Constants.RpcMessageTypes.Response);
        properties.Headers.Add(Constants.HeaderStatusCode, statusCode);
        consumer.Model.BasicPublish("", e.BasicProperties.ReplyTo, properties, body);

        if (statusCode == Constants.StatusCodes.ServerError)
            consumer.Model.BasicNack(e.DeliveryTag, false, true);
        else
            consumer.Model.BasicAck(e.DeliveryTag, false);
    }

    #endregion
}