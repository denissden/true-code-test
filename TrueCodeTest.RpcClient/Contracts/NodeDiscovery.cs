using System.Text;

namespace TrueCodeTest.RpcClient.Contracts;

public class NodeDiscovery
{
    public class Request
    {
        /// <summary>
        ///     Query accepted topics
        /// </summary>
        public string[] Topics { get; set; } = [];

        public static Request Parse(ReadOnlyMemory<byte> body)
        {
            var text = Encoding.UTF8.GetString(body.Span);
            var lines = text.Split('\n');
            return new Request { Topics = lines };
        }

        public byte[] Serialize()
        {
            var topics = string.Join('\n', Topics);
            return Encoding.UTF8.GetBytes(topics);
        }
    }

    public class Response
    {
        /// <summary>
        ///     Topics node accepts
        /// </summary>
        public string[] Topics { get; set; } = [];

        public static Response Parse(ReadOnlyMemory<byte> body)
        {
            var text = Encoding.UTF8.GetString(body.Span);
            var lines = text.Split('\n');
            return new Response { Topics = lines };
        }

        public byte[] Serialize()
        {
            var topics = string.Join('\n', Topics);
            return Encoding.UTF8.GetBytes(topics);
        }
    }
}