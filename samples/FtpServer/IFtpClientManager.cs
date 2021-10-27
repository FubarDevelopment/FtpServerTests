// <copyright file="IClientManager.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Connections;

namespace FtpServer;

public interface IFtpClientManager
{
    IEnumerable<FtpClientInformation> Clients { get; }

    ValueTask StartAsync(
        ConnectionContext connectionContext,
        CancellationToken stoppingToken = default);
}
