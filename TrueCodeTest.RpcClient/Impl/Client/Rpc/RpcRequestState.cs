using RabbitMQ.Client.Events;

namespace TrueCodeTest.RpcClient.Impl.Client.Rpc;

public class RpcRequestState
{
    public string CorrelationId { get; set; }
    public string RequestTopic { get; set; }

    public TaskCompletionSource<(BasicDeliverEventArgs, byte[])> CompletionSource { get; set; } = new();
    public TaskCompletionSource? CancelCompletionSource { get; set; }

    public RequestLifetime RequestLifetime { get; private set; } = RequestLifetime.NotStarted;
    public RequestLifetime CancelRequestLifetime { get; private set; } = RequestLifetime.NotStarted;

    public bool IsFinishedState => (RequestLifetime, CancelRequestLifetime) switch
    {
        (RequestLifetime.Finished, RequestLifetime.NotStarted) => true,
        (RequestLifetime.Errored, RequestLifetime.NotStarted) => true,
        (RequestLifetime.Finished, RequestLifetime.Errored) => true,
        (RequestLifetime.Errored, RequestLifetime.Errored) => true,
        // is successfully cancelled, then remote call is not running and we can safely remove the task 
        (_, RequestLifetime.Finished) => true,
        _ => false
    };


    public void StartRequest()
    {
        RequestLifetime = RequestLifetime.Started;
    }

    public void SetCompleted(BasicDeliverEventArgs e)
    {
        RequestLifetime = RequestLifetime.Finished;
        CompletionSource.TrySetResult((e, e.Body.ToArray()));
    }

    public void SetException(Exception exception)
    {
        RequestLifetime = RequestLifetime.Errored;
        CompletionSource.SetException(exception);
    }

    public void StartCancelRequest()
    {
        CancelCompletionSource = new TaskCompletionSource();
        CancelRequestLifetime = RequestLifetime.Started;
    }

    public void SetCancelled()
    {
        CancelCompletionSource?.SetResult();
        CancelRequestLifetime = RequestLifetime.Finished;
        CompletionSource.TrySetCanceled();
    }

    public void SetCancelException(Exception exception)
    {
        CancelCompletionSource?.SetException(exception);
        CancelRequestLifetime = RequestLifetime.Errored;
    }
}