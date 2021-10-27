// <copyright file="ExternalFtpClient.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.Abstractions;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Client.External;

internal class ExternalFtpClient : IFtpClient
{
    private readonly ExternalFtpClientData _data;
    private readonly ILogger<ExternalFtpClient> _logger;

    public ExternalFtpClient(
        ExternalFtpClientData data,
        ILogger<ExternalFtpClient> logger)
    {
        _data = data;
        _logger = logger;
    }

    public IFtpClientControl Control => _data.ClientControl;

    public async ValueTask RunAsync(CancellationToken cancellationToken = default)
    {
        var connectionContext = _data.ConnectionContext;
        var serverTransportStream = _data.ServerTransportStream;
        var cliTask = _data.CommandTask;
        var clientTransportChannel = _data.ClientTransportChannel;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var delayTask = Task.Delay(Timeout.Infinite, cancellationToken);
                var inputCopyTask = connectionContext.Transport.Input.CopyToAsync(
                    clientTransportChannel.Output, cancellationToken);
                var outputCopyTask = clientTransportChannel.Input.CopyToAsync(
                    connectionContext.Transport.Output, cancellationToken);
                var tasksToWaitFor = new List<Task>()
                {
                    inputCopyTask,
                    outputCopyTask,
                    delayTask,
                    cliTask,
                };

                var finishedTask = await Task.WhenAny(tasksToWaitFor);
                await serverTransportStream.DisposeAsync();
                tasksToWaitFor.Remove(finishedTask);
                tasksToWaitFor.Remove(delayTask);

                await Task.WhenAll(tasksToWaitFor);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "FTP connection aborted with error {ErrorMessage}", exception.Message);
        }
        finally
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            await _data.DisposeAsync();
        }
    }
}
