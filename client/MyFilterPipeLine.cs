using System.Buffers;
using SuperSocket.ProtoBase;

namespace client;

public sealed class MyFilterPipeLine() : FixedHeaderPipelineFilter<MyPackage>(2)
{
    protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);

        reader.TryReadBigEndian(out short length);

        return length;
    }
}