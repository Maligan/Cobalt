using System.Collections.Generic;
using System.IO;
using NetcodeIO.NET;
using ProtoBuf;

public static class NetcodeUtils
{
    private static byte[] Serialize<T>(T value, out int length)
    {
        var stream = new MemoryStream();
        Serializer.Serialize(stream, value);
        length = (int)stream.Position;
        return stream.GetBuffer();
    }

    public static void Send<T>(this Client client, T data)
    {
        var bytes = Serialize(data, out var length);
        client.Send(bytes, length);
    }

    public static void Send<T>(this RemoteClient client, T data)
    {
        var bytes = Serialize(data, out var length);
        client.SendPayload(bytes, length);
    }

    public static void Send<T>(this IEnumerable<RemoteClient> clients, T data)
    {
        var bytes = Serialize(data, out var length);
        foreach (var client in clients)
            client.SendPayload(bytes, length);
    }

    public static T Read<T>(byte[] bytes, int length)
    {
        var stream = new MemoryStream(bytes, 0, length);
        return Serializer.Deserialize<T>(stream);
    }
}