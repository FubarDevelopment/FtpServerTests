// <copyright file="EmbeddedFtpClient.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.Abstractions;
using FubarDev.FtpServer.Client.Common;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Client.Embedded;

internal class EmbeddedFtpClient : IFtpClient, IDisposable
{
    private readonly FtpConnection _connection;
    private readonly CancellationTokenSource _connectionTokenSource;
    private readonly ILogger<EmbeddedFtpClient> _logger;

    public EmbeddedFtpClient(
        FtpConnection connection,
        CancellationTokenSource connectionTokenSource,
        ILogger<EmbeddedFtpClient> logger)
    {
        _connection = connection;
        _connectionTokenSource = connectionTokenSource;
        _logger = logger;
    }

    public async Task WaitForExitAsync(CancellationToken cancellationToken = default)
    {
        using var stoppingTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _connectionTokenSource.Token);
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Delay aborted with error {ErrorMessage}", exception.Message);
        }

        _logger.LogTrace("Stopping FTP connection");
        await _connection.StopAsync(cancellationToken);
    }

    public void Dispose()
    {
        _connectionTokenSource.Dispose();
    }
}
