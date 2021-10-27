// <copyright file="FtpConnection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Text;

using FubarDev.FtpServer.Abstractions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Client.Common;

public class FtpConnection : BackgroundService
{
    private readonly IFtpConnectionContextAccessor _connectionContextAccessor;
    private readonly ILogger<FtpConnection> _logger;

    public FtpConnection(
        IFtpConnectionContextAccessor connectionContextAccessor,
        ILogger<FtpConnection> logger)
    {
        _connectionContextAccessor = connectionContextAccessor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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
}
