using System.Buffers;
using System.Buffers.Binary;
using Core;
using SuperSocket.ProtoBase;

namespace client;

public sealed class MyPackageEncoder : IPackageEncoder<MyPackage>
{
    public int Encode(IBufferWriter<byte> writer, MyPackage pack)
    {
        var buffer = writer.GetSpan(MyPackage.HeadLength);
        writer.Advance(MyPackage.HeadLength);

        var length = writer.WriterBit(pack.Key);

        if (string.IsNullOrWhiteSpace(pack.Content))
            length += writer.WriterStringWithLength(pack.Content!);

        BinaryPrimitives.WriteInt16BigEndian(buffer, (short)length);

        return length + MyPackage.HeadLength;
    }
}