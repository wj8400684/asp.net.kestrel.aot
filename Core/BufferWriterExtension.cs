using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace Core;

public static class BufferWriterExtension
{
    public static int WriterBit(this IBufferWriter<byte> writer, byte value)
    {
        const int v1 = sizeof(byte);

        var buffer = writer.GetSpan(v1);

        MemoryMarshal.Write(buffer, value);
        writer.Advance(v1);

        return v1;
    }

    public static int WriterShort(this IBufferWriter<byte> writer, short value)
    {
        const int v1 = sizeof(short);

        var buffer = writer.GetSpan(v1);

        BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
        writer.Advance(v1);

        return v1;
    }

    public static int WriterInt(this IBufferWriter<byte> writer, int value)
    {
        const int v1 = sizeof(int);

        var buffer = writer.GetSpan(v1);

        BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
        writer.Advance(v1);

        return v1;
    }

    public static int WriterLong(this IBufferWriter<byte> writer, long value)
    {
        const int v1 = sizeof(long);

        var buffer = writer.GetSpan(v1);

        BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
        writer.Advance(v1);

        return v1;
    }

    public static int WriterStringWithLength(this IBufferWriter<byte> writer, string value)
    {
        const int v1 = sizeof(ushort);

        var buffer = writer.GetSpan(v1);

        writer.Advance(v1);

        var length = (byte)writer.Write(value, Encoding.UTF8);

        BinaryPrimitives.WriteInt16BigEndian(buffer, length);

        return v1 + length;
    }
    
    public static int Write(this IBufferWriter<byte> writer, ReadOnlySpan<char> text, Encoding encoding)
    {
        var encoder = encoding.GetEncoder();
        var completed = false;
        var totalBytes = 0;

        var minSpanSizeHint = encoding.GetMaxByteCount(1);

        while (!completed)
        {
            var span = writer.GetSpan(minSpanSizeHint);

            encoder.Convert(text, span, false, out int charsUsed, out int bytesUsed, out completed);
                
            if (charsUsed > 0)
                text = text.Slice(charsUsed);

            totalBytes += bytesUsed;
            writer.Advance(bytesUsed);
        }

        return totalBytes;
    }
}