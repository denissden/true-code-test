namespace RabbitNodes.Shared.Contracts;

public class AddNumbers
{
    public const string Topic = "math.add";

    public class Request
    {
        public int A { get; set; }
        public int B { get; set; }

        public byte[] Serialize()
        {
            var bytes = new byte[8];
            BitConverter.GetBytes(A).CopyTo(bytes, 0);
            BitConverter.GetBytes(B).CopyTo(bytes, 4);
            return bytes;
        }

        public static Request Parse(ReadOnlyMemory<byte> memory)
        {
            var bytes = memory.ToArray();
            var a = BitConverter.ToInt32(bytes, 0);
            var b = BitConverter.ToInt32(bytes, 4);
            return new Request { A = a, B = b };
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