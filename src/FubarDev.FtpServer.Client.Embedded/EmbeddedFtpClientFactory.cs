// <copyright file="EmbeddedFtpClientFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.Abstractions;
using FubarDev.FtpServer.Client.Common;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Nerdbank.Streams;

namespace FubarDev.FtpServer.Client.Embedded;

public class EmbeddedFtpClientFactory : IFtpClientFactory
{
    private readonly IFtpConnectionContextAccessor _connectionContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmbeddedFtpClientFactory> _logger;

    public EmbeddedFtpClientFactory(
        IFtpConnectionContextAccessor connectionContextAccessor,
        IServiceProvider serviceProvider,
        ILogger<EmbeddedFtpClientFactory> logger)
    {
        _connectionContextAccessor = connectionContextAccessor;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async ValueTask<IFtpClient> CreateClientAsync(
        ConnectionContext connectionContext,
        CancellationToken cancellationToken = default)
    {
        var cts = new CancellationTokenSource();
        var transport = new DuplexPipe(
            connectionContext.Transport.Input,
            connectionContext.Transport.Output
                .OnCompleted((_, _) => cts.Cancel()));
        _connectionContextAccessor.Context = new FtpConnectionContext(transport);
        var connection = ActivatorUtilities.CreateInstance<FtpConnection>(_serviceProvider);
        
        await connection.StartAsync(cancellationToken);
        if (connection.ExecuteTask.IsCompleted)
        {
            _logger.LogWarning("FTP connection stopped too early");
            await connection.ExecuteTask;
        }

        var ftpClient = ActivatorUtilities.CreateInstance<EmbeddedFtpClient>(
            _serviceProvider,
            connection,
            cts);
        return ftpClient;
    }
}
