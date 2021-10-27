// <copyright file="FtpServerService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using System.Net;
using System.Threading.Channels;

using FubarDev.FtpServer.Abstractions;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;

namespace FtpServerRestart01;

public class FtpServerService : BackgroundService
{
    private readonly object _activeClientsLock = new();
    private readonly IFtpClientFactory _clientFactory;
    private readonly Channel<ConnectionContext> _connectionContextChannel;
    private readonly IConnectionListenerFactory _connectionListenerFactory;
    private readonly ILogger<FtpServerService> _logger;
    private readonly FtpServerOptions _serverOptions;
    private volatile ImmutableList<FtpClientInformation> _activeClients = ImmutableList<FtpClientInformation>.Empty;

    public FtpServerService(
        IOptions<FtpServerOptions> serverOptions,
        IConnectionListenerFactory connectionListenerFactory,
        IFtpClientFactory clientFactory,
        ILogger<FtpServerService> logger)
    {
        _connectionListenerFactory = connectionListenerFactory;
        _clientFactory = clientFactory;
        _logger = logger;
        _serverOptions = serverOptions.Value;
        _connectionContextChannel = Channel.CreateUnbounded<ConnectionContext>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var listenTasks = new List<Task>();
        await foreach (var endPoint in GetListenEndPointsAsync(_serverOptions, stoppingToken))
            try
            {
                var listener = await _connectionListenerFactory
                    .BindAsync(endPoint, stoppingToken);
                var listenTask = ListenConnectionAsync(
                    listener,
                    _connectionContextChannel.Writer,
                    stoppingToken);

                listenTasks.Add(listenTask);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error listening to {EndPoint}: {Message}",
                    endPoint,
                    exception.Message);
            }

        if (listenTasks.Count == 0) throw new InvalidOperationException("Service is not listening to any address");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var connectionContext = await _connectionContextChannel.Reader.ReadAsync(stoppingToken);
                _ = Task.Run(
                    () => ExecuteClientAsync(connectionContext, stoppingToken),
                    CancellationToken.None);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while reading connections: {ErrorMessage}", exception.Message);
        }

        await Task.WhenAll(listenTasks);
    }

    private static async Task ListenConnectionAsync(
        IConnectionListener connectionListener,
        ChannelWriter<ConnectionContext> writer,
        CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var connectionContext = await connectionListener.AcceptAsync(cancellationToken);
                if (connectionContext == null) break;

                await writer.WriteAsync(connectionContext, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        finally
        {
            await connectionListener.DisposeAsync();
        }
    }

    private static IAsyncEnumerable<IPEndPoint> GetListenEndPointsAsync(
        FtpServerOptions options,
        CancellationToken cancellationToken)
    {
        if (options.ListenEndPoints.Count == 0)
            return new[] {new IPEndPoint(IPAddress.IPv6Any, 21)}.ToAsyncEnumerable();

        return options.ListenEndPoints.ToAsyncEnumerable();
    }

    private async Task ExecuteClientAsync(
        ConnectionContext connectionContext,
        CancellationToken cancellationToken)
    {
        await using var registration = cancellationToken.Register(
            connectionContext.Abort);
        var client = await _clientFactory.CreateClientAsync(connectionContext, cancellationToken);
        var clientInfo = new FtpClientInformation(client, connectionContext);
        lock (_activeClientsLock)
        {
            _activeClients = _activeClients.Add(clientInfo);
        }

        _logger.LogTrace("FTP client added");
        await client.WaitForExitAsync(connectionContext.ConnectionClosed);

        lock (_activeClientsLock)
        {
            _activeClients = _activeClients.Remove(clientInfo);
            _logger.LogTrace("FTP client removed");
        }
    }
}
