// <copyright file="HostedFtpConnection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.Client.Common;

using Microsoft.Extensions.Hosting;

namespace FubarDev.FtpServer.Client.External;

public class HostedFtpConnection : BackgroundService
{
    private readonly FtpConnection _connection;

    public HostedFtpConnection(FtpConnection connection)
    {
        _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _connection.RunAsync(stoppingToken);
    }
}
