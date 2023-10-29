using System.Buffers;
using SuperSocket.ProtoBase;

namespace client;

public sealed class MyPackageDecoder : IPackageDecoder<MyPackage>
{
    public MyPackage Decode(ref ReadOnlySequence<byte> buffer, object context)
    {
        var reader = new SequenceReader<byte>(buffer);
        reader.Advance(MyPackage.HeadLength);

        reader.TryRead(out var key);

        var package = new MyPackage { Key = key };

        if (reader.TryReadBigEndian(out ushort contentLength))
            package.Content = reader.ReadString(contentLength);

        return package;
    }
}