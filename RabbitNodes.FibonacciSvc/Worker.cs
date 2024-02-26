using RabbitMQ.Client;
using RabbitNodes.Shared.Contracts;
using TrueCodeTest.RpcClient;

namespace RabbitNodes.FibonacciSvc;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private Hub _hub;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Staring worker");
        var connFactory = new ConnectionFactory
        {
            Uri = new Uri(_configuration.GetConnectionString("RabbitMQ")),
            DispatchConsumersAsync = false,
            ConsumerDispatchConcurrency = Environment.ProcessorCount
        };
        _hub = Hub.Connect(connFactory);

        var channel = _hub.DefaultConnection.CreateModel();
        channel.QueueDeclare("q_fibonacci", true, false, false);
        channel.Dispose();

        _hub.DefaultNode.HandleRpc("q_fibonacci", Fibonacci.Topic, HandleFibonacci);
    }

    private async Task<byte[]> HandleFibonacci(ReadOnlyMemory<byte> body, CancellationToken token)
    {
        var request = Fibonacci.Request.Parse(body);

        var result = await GetFibonacci(request.F, token);
        var response = new Fibonacci.Response
        {
            N = result
        };
        return response.Serialize();
    }

    private async Task<int> GetFibonacci(int n, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (n <= 1)
        {
            return n;
        }

        var a = await GetFibonacci(n - 1, cancellationToken);
        var b = await GetFibonacci(n - 2, cancellationToken);
        return await Add(a, b, cancellationToken);
    }

    private async Task<int> Add(int a, int b, CancellationToken cancellationToken)
    {
        var nodelet = await _hub!.NodeletProvider.GetNodelet([AddNumbers.Topic], cancellationToken: cancellationToken);

        Console.WriteLine(
            $"""Discovered nodelet - id: {nodelet.Id}; topics: {string.Join(" ,", nodelet.SupportedTopics)}""");
        var request = new AddNumbers.Request { A = a, B = b };

        var execution = nodelet.Execute(request.Serialize(), AddNumbers.Topic, cancellationToken);

        var result = await execution.GetOutputAsync(cancellationToken);
        var response = AddNumbers.Response.Parse(result);
        return response.N;
    }
}