using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitNodes.Shared.Contracts;
using TrueCodeTest.RpcClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connFactory = new ConnectionFactory
{
    Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")),
    DispatchConsumersAsync = false,
    ConsumerDispatchConcurrency = Environment.ProcessorCount
};
builder.Services.AddScoped<Hub>(_ => Hub.Connect(connFactory));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/fibonacci", async ([FromQuery] int f, [FromQuery] int timeout, [FromServices] Hub hub) =>
    {
        var stopwatch = Stopwatch.StartNew();

        var nodelet = await hub!.NodeletProvider.GetNodelet([Fibonacci.Topic]);

        var request = new Fibonacci.Request { F = f };

        var execution = nodelet.Execute(request.Serialize(), Fibonacci.Topic);
        _ = Task.Delay(timeout).ContinueWith(async task => { await execution.CancelAsync(); });

        var result = await execution.GetOutputAsync();
        var response = AddNumbers.Response.Parse(result);

        stopwatch.Stop();

        return new FibonacciResponse(f, response.N, stopwatch.ElapsedMilliseconds);
    })
    .WithName("Fibonacci")
    .WithOpenApi();

app.Run();

internal record FibonacciResponse(int F, int N, long TotalMilliseconds);