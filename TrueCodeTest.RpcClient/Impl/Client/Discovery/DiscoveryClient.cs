using System.Collections.Concurrent;
using System.Diagnostics;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TrueCodeTest.RpcClient.Config;
using TrueCodeTest.RpcClient.Contracts;

namespace TrueCodeTest.RpcClient.Impl.Client.Discovery;

public class DiscoveryClient
{
    private readonly IModel _channel;
    private readonly ClientConfig _clientConfig;

    private readonly ConcurrentDictionary<string, List<DiscoveredNodelet>>
        _multipleNodeResponses = new(); // TODO: discover multiple nodes

    private readonly ConcurrentDictionary<string, TaskCompletionSource<DiscoveredNodelet>> _singleNodeResponses = new();

    private IBasicConsumer _replyToConsumer;
    private string _replyToQueue;

    private DiscoveryClient(IModel channel, ClientConfig clientConfig)
    {
        _channel = channel;
        _clientConfig = clientConfig;
        Id = Guid.NewGuid();
    }

    public Guid Id { get; init; }

    public static DiscoveryClient Create(IModel channel, ClientConfig clientConfig)
    {
        var client = new DiscoveryClient(channel, clientConfig);
        client.Init();
        return client;
    }

    public async Task<DiscoveredNodelet> DiscoverOneNodelet(string[] topics, TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        timeout = timeout ?? TimeSpan.FromSeconds(1);

        var timeoutTask = Task.Delay(timeout.Value, cancellationToken);

        var correlationId = Guid.NewGuid().ToString();

        var tcs = new TaskCompletionSource<DiscoveredNodelet>();
        cancellationToken.Register(() =>
        {
            _singleNodeResponses.TryRemove(correlationId, out _);
            tcs.TrySetCanceled(cancellationToken);
        }, false);
        _singleNodeResponses.TryAdd(correlationId, tcs);

        SendDiscovery(topics, correlationId);

        cancellationToken.ThrowIfCancellationRequested();
        var firstCompletedTask = await Task.WhenAny(new[] { timeoutTask, tcs.Task }).ConfigureAwait(false);
        if (firstCompletedTask == timeoutTask)
        {
            tcs.TrySetCanceled(cancellationToken);
            throw new TimeoutException();
        }

        return tcs.Task.Result;
    }

    private void SendDiscovery(string[] topics, string correlationId)
    {
        var props = _channel.CreateBasicProperties();
        props.CorrelationId = correlationId;
        props.Headers = GetHeaders();
        props.ReplyTo = _replyToQueue;
        var request = new NodeDiscovery.Request { Topics = topics };
        _channel.BasicPublish(_clientConfig.NodeDiscoveryExchange, _clientConfig.NodeDiscoveryRequestTopic, props,
            request.Serialize());
    }

    private Dictionary<string, object> GetHeaders()
    {
        return new Dictionary<string, object>
        {
            [Constants.HeaderTopic] = _clientConfig.NodeDiscoveryRequestTopic,
            [Constants.HeaderClientName] = _clientConfig.ClientName,
            [Constants.HeaderClientId] = Id.ToString()
        };
    }

    private void Init()
    {
        InitDiscoveryCallbacks();
    }

    private void InitDiscoveryCallbacks()
    {
        if (_clientConfig.IsAsync)
        {
            var asyncConsumer = new AsyncEventingBasicConsumer(_channel);
            asyncConsumer.Received += (sender, @event) =>
                Task.Factory.StartNew(() => OnNodeletResponseReceived(sender, @event));
            _replyToConsumer = asyncConsumer;
        }
        else
        {
            var basicConsumer = new EventingBasicConsumer(_channel);
            basicConsumer.Received += OnNodeletResponseReceived;
            _replyToConsumer = basicConsumer;
        }

        var rpcQueueDeclareOk = _channel.QueueDeclare();
        _replyToQueue = rpcQueueDeclareOk.QueueName;
        _channel.BasicConsume(_replyToQueue, true, _replyToConsumer);
    }

    private void OnNodeletResponseReceived(object? sender, BasicDeliverEventArgs e)
    {
        var consumer = sender as IBasicConsumer;
        Debug.Assert(consumer is not null);

        if (_singleNodeResponses.Remove(e.BasicProperties.CorrelationId, out var tcs))
            tcs.TrySetResult(DiscoveredNodelet.FromEvent(e));

        if (_multipleNodeResponses.TryGetValue(e.BasicProperties.CorrelationId, out var list))
            list.Add(DiscoveredNodelet.FromEvent(e));
    }
}