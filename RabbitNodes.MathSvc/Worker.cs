using RabbitMQ.Client;
using RabbitNodes.Shared.Contracts;
using TrueCodeTest.RpcClient;

namespace RabbitNodes.MathSvc;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

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
        var hub = Hub.Connect(connFactory);

        var channel = hub.DefaultConnection.CreateModel();
        channel.QueueDeclare("q_math", true, false, false);
        channel.Dispose();

        hub.DefaultNode.HandleRpc("q_math", AddNumbers.Topic, HandleAdd);
    }

    private Task<byte[]> HandleAdd(ReadOnlyMemory<byte> body, CancellationToken token)
    {
        var request = AddNumbers.Request.Parse(body);


        var result = request.A + request.B;

        Console.WriteLine($"{request.A} + {request.B} = {result}");

        var response = new AddNumbers.Response
        {
            N = result
        };
        return Task.FromResult(response.Serialize());
    }
}