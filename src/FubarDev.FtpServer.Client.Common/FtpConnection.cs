// <copyright file="FtpConnection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Text;

using FubarDev.FtpServer.Abstractions;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Client.Common;

public class FtpConnection
{
    private readonly IFtpConnectionContextAccessor _connectionContextAccessor;
    private readonly ILogger<FtpConnection> _logger;
    private DataHolder<DateTimeOffset> _lastActivity = new(DateTimeOffset.Now);

    public FtpConnection(
        IFtpConnectionContextAccessor connectionContextAccessor,
        ILogger<FtpConnection> logger)
    {
        _connectionContextAccessor = connectionContextAccessor;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken stoppingToken = default)
    {
        var connectionContext = _connectionContextAccessor.Context ?? throw new InvalidOperationException();
        var transport = connectionContext.ControlConnection;
        try
        {
            _logger.LogInformation("Sending status message");
            var result = Encoding.UTF8.GetBytes("220 Ready.\r\n");
            await transport.Output.WriteAsync(result, stoppingToken);
            await transport.Output.FlushAsync(stoppingToken);
            _logger.LogInformation("Status message sent");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Wait for read");
                var readResult = await transport.Input.ReadAsync(stoppingToken);
                if (readResult.IsCanceled || readResult.IsCompleted)
                {
                    _logger.LogInformation("Read cancelled");
                    break;
                }

                Interlocked.Exchange(ref _lastActivity, new(DateTimeOffset.Now));

                var text = Encoding.UTF8.GetString(readResult.Buffer);

                _logger.LogDebug(
                    "Received {NumBytes} bytes ({Text})",
                    readResult.Buffer.Length,
                    text);

                // Drop
                transport.Input.AdvanceTo(readResult.Buffer.End);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Client connection closed: {ErrorMessage}", exception.Message);
        }
        finally
        {
            _logger.LogInformation("FTP client handler stopped");
            await transport.Output.CompleteAsync();
        }
    }

    public void Ping()
    {
        _logger.LogDebug("Ping received");
    }

    public DateTimeOffset GetLastActivity()
    {
        return _lastActivity.Data;
    }

    public TimeSpan GetInactivity()
    {
        return DateTimeOffset.UtcNow - _lastActivity.Data.ToUniversalTime();
    }

    private record DataHolder<T>(T Data);
}
