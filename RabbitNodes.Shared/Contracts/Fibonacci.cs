namespace RabbitNodes.Shared.Contracts;

public class Fibonacci
{
    public const string Topic = "fibonacci.getNumber";

    public class Request
    {
        public int F { get; set; }

        public byte[] Serialize()
        {
            var bytes = new byte[8];
            BitConverter.GetBytes(F).CopyTo(bytes, 0);
            return bytes;
        }

        public static Request Parse(ReadOnlyMemory<byte> memory)
        {
            var bytes = memory.ToArray();
            var f = BitConverter.ToInt32(bytes, 0);
            return new Request { F = f };
        }
    }

    public class Response
    {
        public int N { get; set; }

        public byte[] Serialize()
        {
            var bytes = new byte[4];
            BitConverter.GetBytes(N).CopyTo(bytes, 0);
            return bytes;
        }

        public static Response Parse(ReadOnlyMemory<byte> memory)
        {
            var bytes = memory.ToArray();
            var n = BitConverter.ToInt32(bytes, 0);
            return new Response { N = n };
        }
    }
}