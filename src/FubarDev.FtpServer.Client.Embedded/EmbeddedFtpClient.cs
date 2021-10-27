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
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmbeddedFtpClient> _logger;

    public EmbeddedFtpClient(
        ConnectionContext connectionContext,
        IFtpConnectionContextAccessor connectionContextAccessor,
        IServiceProvider serviceProvider,
        ILogger<EmbeddedFtpClient> logger)
    {
        _connectionContext = connectionContext;
        _connectionContextAccessor = connectionContextAccessor;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async ValueTask RunAsync(CancellationToken cancellationToken = default)
    {
        var cts = new CancellationTokenSource();
        var transport = new DuplexPipe(
            _connectionContext.Transport.Input,
            _connectionContext.Transport.Output
                .OnCompleted((_, _) => cts.Cancel()));
        _connectionContextAccessor.Context = new FtpConnectionContext(transport);
        var connection = ActivatorUtilities.CreateInstance<FtpConnection>(_serviceProvider);

        using var stoppingTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _connectionContext.ConnectionClosed);
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
            await connection.RunAsync(stoppingTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Delay aborted with error {ErrorMessage}", exception.Message);
        }

        _logger.LogTrace("FTP connection stopped");
    }
}
