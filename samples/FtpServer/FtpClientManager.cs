// <copyright file="FtpClientManager.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using FubarDev.FtpServer.Abstractions;

using Microsoft.AspNetCore.Connections;

namespace FtpServer;

public class FtpClientManager : IFtpClientManager
{
    private readonly object _activeClientsLock = new();
    private readonly IFtpClientFactory _clientFactory;
    private readonly ILogger<FtpClientManager> _logger;
    private volatile ImmutableList<FtpClientInformation> _activeClients = ImmutableList<FtpClientInformation>.Empty;

    public FtpClientManager(
        IFtpClientFactory clientFactory,
        ILogger<FtpClientManager> logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
    }

    public IEnumerable<FtpClientInformation> Clients
    {
        get
        {
            lock (_activeClientsLock)
            {
                return _activeClients;
            }
        }
    }

    public ValueTask StartAsync(ConnectionContext connectionContext, CancellationToken stoppingToken = default)
    {
        Run(connectionContext, stoppingToken);
        return default;
    }

    [SuppressMessage(
        "Usage",
        "VSTHRD100",
        Justification = "It's what we want here, because we're not using the returning task anyway")]
    private async void Run(ConnectionContext connectionContext, CancellationToken stoppingToken = default)
    {
        await using var registration = stoppingToken.Register(
            connectionContext.Abort);
        try
        {
            var client = await _clientFactory.CreateClientAsync(connectionContext, stoppingToken);
            var clientInfo = new FtpClientInformation(client, connectionContext);
            lock (_activeClientsLock)
            {
                _activeClients = _activeClients.Add(clientInfo);
            }

            _logger.LogTrace("FTP client added");
            try
            {
                await client.RunAsync(connectionContext.ConnectionClosed);
            }
            finally
            {
                lock (_activeClientsLock)
                {
                    _activeClients = _activeClients.Remove(clientInfo);
                    _logger.LogTrace("FTP client removed");
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error during FTP client handling: {ErrorMessage}", exception.Message);
        }
        finally
        {
            await connectionContext.DisposeAsync();
        }
    }
}
