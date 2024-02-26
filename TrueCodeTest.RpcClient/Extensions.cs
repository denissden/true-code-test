using System.Text;

namespace TrueCodeTest.RpcClient;

public static class Extensions
{
    public static string? GetStringHeader(this IDictionary<string, object>? dictionary, string header)
    {
        return dictionary?.TryGetValue(header, out var value) == true ? Encoding.UTF8.GetString((byte[])value) : null;
    }

    public static int? GetIntHeader(this IDictionary<string, object>? dictionary, string header)
    {
        return dictionary?.TryGetValue(header, out var value) == true ? Convert.ToInt32(value) : null;
    }
}