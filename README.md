# Test for true.code

## Links

- [Task 1 - Read messages from a stream](TrueCodeTest.StreamReader)
- [Task 2 - Query users with tags](TrueCodeTest.UserManager)
- [Task 3 - RabbitMQ RPC client with cancellation support](TrueCodeTest.RpcClient)

## Task

1. Write an implementation that allows reading consecutive messages (data packets) from an input byte stream (Stream).
   Each subsequent message is separated from the previous one by a configurable one-byte delimiter.

2. Write an implementation that allows:
    - to select a user (with their tags) by their Id and Domain
    - to select all users (with their tags) of one Domain, using pagination
    - to select all users by the value of a tag (those who have the given tag) and Domain

3. Implement an RPC client based on RabbitMQ to invoke IRemoteMethod on a remote node. Each remote method has some input
   and output (input & output), and its execution can be remotely interrupted. Use RabbitMQ.Client library. The
   implementation should be built using docker compose. 
      - run `docker compose up -d --build`
      - query [Http file](RabbitNodes.FibonacciApi/RabbitNodes.FibonacciApi.http)

The application must:

- be implemented based on the console (not ASP.NET Core)
- use .NET 5 or higher
- use EF API
- use the standard Dependency Injection container (IServiceProvider)

## Original task (in Russian)

### Задача

1. Написать реализацию, которая позволяет считывать из входного байтового потока (Stream) последовательные сообщения (
   пакеты данных).
   Каждое следующее сообщение отделено от предыдущего конфигурируемым однобайтовым разделителем.

2. Написать реализацию, который позволяет
   выбрать пользователя (с его тегами) по его Id и Domain
   выбрать всех пользователей (с их тегами) одного Domain, используя pagination
   выбрать всех пользователей по значению тега (которые имеют данный тег) и Domain

### Приложение должно

- быть реализовано на базе консоли (не ASP.NET Core)
- использовать .NET 5 или выше
- использовать EF API
- использовать стандартный Dependency Injection контейнер (IServiceProvider)

```c#
public class User 
{
  public Guid UserId { get; set; }
  
  [Required]
  public string Name { get; set; } = default!;
  
  [Required]
  public string Domain { get; set; } = default!;
  public List<TagToUser>? Tags { get; set; }
}

public class TagToUser 
{
  public Guid EntityId { get; set;}
  public Guid UserId { get; set; }
  public Guid TagId { get; set; }
  
  
  public User? User { get; set; }
  public List<Tag>? Tags { get; set; }
}

public class Tag
{
  public Guid TagId { get; set; }
  
  [Required]
  public string Value { get; set; } = default!;
  
  [Required]
  public string Domain { get; set; } = default!;
  
  
  public List<Users>? Users { get; set; }
}
```

### Требования к присылаемым решениям

Выполненное тестовое задание должно быть собрано в архив и включать в себя:
Все исходные файлы вместе с проектными файлами.
Текстовый файл readme.txt с инструкцией по настройке и конфигурированию приложения (если необходимо).

### Задание 3

Реализовать RPC клиент на базе RabbitMQ для вызова `IRemoteMethod` на удаленном узле.
Каждый удаленный метод имеет некоторый вход и выход (`input` & `output`), а также его исполнение может быть прервано
удаленно.
Использовать библиотеку `RabbitMQ.Client`.
Реализация должна быть построена на базе `docker compose`.

```c#
public interface IRemoteMethod {
ValueTask<byte[]> ExecuteAsync(ReadOnlyMemory<byte> input, CancellationToken cancellationToken);
}
```

Пример

```c#
string nodeId = ... // get a target node ID
IRemoteNode node = await nodeProvider.GetNodeAsync(nodeId, cancellationToken);
byte[] input = JsonSerializer.SerializeToUtf8Bytes(new MyInput("hi there!"));
IRemoteMethodHandler remoteMethod = await node.ExecuteAsync<MyMethod>(input, cancellationToken);
await Task.Delay(1_000, cancellationToken); if (!remoteMethod.IsRunning)
{
await remoteMethod.CancelAsync(cancellationToken); }

 
 // process the output
record MyInput(string Message); 
```