using RabbitMQ.Client;
using RabbitNodes.Shared.Contracts;
using TrueCodeTest.RpcClient;

var connFactory = new ConnectionFactory
{
    Uri = new Uri(Environment.GetEnvironmentVariable("RABBITMQ_URI") ?? "amqp://guest:guest@localhost:5672"),
    DispatchConsumersAsync = true,
    ConsumerDispatchConcurrency = Environment.ProcessorCount,
};
var hub = Hub.Connect(connectionFactory: connFactory);

// Add two numbers using Math service


// Get N fibonacci number


var channel = hub.DefaultConnection.CreateModel();
channel.QueueDeclare("q_fibonacci", durable: true, exclusive: false, autoDelete: false);
channel.Dispose();

hub.DefaultNode.HandleRpc("q_fibonacci", Fibonacci.Topic, async (body, token) =>
{
    var request = Fibonacci.Request.Parse(body);
    
    var result = await GetFibonacci(request.F, token);
    var response = new Fibonacci.Response
    {
        N = result
    };
    return response.Serialize();
});

Console.Read();