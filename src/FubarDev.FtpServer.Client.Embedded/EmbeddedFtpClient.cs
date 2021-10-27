// <copyright file="EmbeddedFtpClient.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.Abstractions;
using FubarDev.FtpServer.Client.Common;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Nerdbank.Streams;

namespace FubarDev.FtpServer.Client.Embedded;

internal class EmbeddedFtpClient : IFtpClient
{
    private readonly ConnectionContext _connectionContext;
    private readonly IFtpConnectionContextAccessor _connectionContextAccessor;
    private readonly ILogger<EmbeddedFtpClient> _logger;
    private readonly FtpConnection _connection;

    public EmbeddedFtpClient(
        ConnectionContext connectionContext,
        IFtpConnectionContextAccessor connectionContextAccessor,
        IServiceProvider serviceProvider,
        ILogger<EmbeddedFtpClient> logger)
    {
        _connectionContext = connectionContext;
        _connectionContextAccessor = connectionContextAccessor;
        _logger = logger;
        _connection = ActivatorUtilities.CreateInstance<FtpConnection>(serviceProvider);
        Control = new EmbeddedFtpClientControl(_connection);
    }

    public IFtpClientControl Control { get; }

    public async ValueTask RunAsync(CancellationToken cancellationToken = default)
    {
        var transport = new DuplexPipe(
            _connectionContext.Transport.Input,
            _connectionContext.Transport.Output);
        _connectionContextAccessor.Context = new FtpConnectionContext(transport);

        using var stoppingTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken);
        try
        {
            using var loggerScope = _logger.BeginScope(
                new Dictionary<string, object?>()
                {
                    ["ConnectionId"] = _connectionContext.ConnectionId,
                    ["LocalEndPoint"] = _connectionContext.LocalEndPoint,
                    ["RemoteEndPoint"] = _connectionContext.RemoteEndPoint,
                });
            _logger.LogTrace("FTP connection starting");
            await _connection.RunAsync(stoppingTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "FTP connection aborted with error {ErrorMessage}", exception.Message);
        }

        _logger.LogTrace("FTP connection stopped");
    }
}
