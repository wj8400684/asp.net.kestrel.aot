using Microsoft.AspNetCore.Connections;

namespace aot;

public sealed class RpcConnectionHandler(ILogger<RpcConnectionHandler> logger) : ConnectionHandler
{
    public override async Task OnConnectedAsync(ConnectionContext connection)
    {
        logger.LogInformation($"客户端连接：{connection.ConnectionId}-{connection.RemoteEndPoint}");

        try
        {
            await HandleRequestsAsync(connection);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,"未知异常");
        }
        finally
        {
            logger.LogInformation($"客户端断开连接：{connection.ConnectionId}-{connection.RemoteEndPoint}");
            await connection.DisposeAsync();
        }
    }

    private static async Task HandleRequestsAsync(ConnectionContext context)
    {
        var reader = context.Transport.Input;
        var writer = context.Transport.Output;

        while (context.ConnectionClosed.IsCancellationRequested == false)
        {
            var result = await reader.ReadAsync();
            if (result.IsCanceled)
                break;

            if (result.IsCompleted)
                break;
        }
    }
}