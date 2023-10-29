using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Text;
using Core;
using Microsoft.AspNetCore.Connections;

namespace aot;

public sealed class RpcConnectionHandler(ILogger<RpcConnectionHandler> logger) : ConnectionHandler
{
    private static readonly byte[] HelloWorld = Encoding.UTF8.GetBytes("Hello world");

    /// <summary>
    /// 收到Echo连接后
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    public override async Task OnConnectedAsync(ConnectionContext connection)
    {
        logger.LogInformation($"客户端连接：{connection.ConnectionId}-{connection.RemoteEndPoint}");

        var input = connection.Transport.Input;
        var output = connection.Transport.Output;

        WriteHelloWorld(output);

        await output.FlushAsync();

        try
        {
            while (connection.ConnectionClosed.IsCancellationRequested == false)
            {
                var result = await input.ReadAsync();
                if (result.IsCanceled)
                    break;

                if (TryReadEcho(result, out var echo, out var consumed))
                {
                    using (echo)
                    {
                        //var text = Encoding.UTF8.GetString(echo.Array, 0, echo.Length);
                        //logger.LogInformation($"Received from server: {text}");

                        output.Write(2);
                        output.WriteBigEndian((ushort)echo.Length);
                        output.Write(echo.Array.AsSpan(0, echo.Length));
                        await output.FlushAsync();
                    }

                    input.AdvanceTo(consumed);
                }
                else
                {
                    input.AdvanceTo(result.Buffer.Start, result.Buffer.End);
                }

                if (result.IsCompleted)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "未知异常");
        }
        finally
        {
            logger.LogInformation($"客户端断开连接：{connection.ConnectionId}-{connection.RemoteEndPoint}");
            await connection.DisposeAsync();
        }
    }

    private static void WriteHelloWorld(PipeWriter writer)
    {
        var headerBuffer = writer.GetSpan(2);
        writer.Advance(2);

        var length = writer.WriterBit(1); //key

        length += writer.WriterStringWithLength("Hello world");

        BinaryPrimitives.WriteInt16BigEndian(headerBuffer, (short)length);
    }

    private static bool TryReadEcho(ReadResult result,
        [MaybeNullWhen(false)] out IArrayOwner<byte> echo, out SequencePosition consumed)
    {
        var reader = new SequenceReader<byte>(result.Buffer);
        if (reader.TryReadBigEndian(out short length))
        {
            if (reader.Remaining >= length)
            {
                echo = ArrayPool<byte>.Shared.RentArrayOwner(length);
                reader.UnreadSequence.Slice(0, length).CopyTo(echo.Array);
                reader.Advance(length);

                consumed = reader.Position;
                return true;
            }
        }

        echo = null;
        consumed = result.Buffer.Start;
        return false;
    }
}