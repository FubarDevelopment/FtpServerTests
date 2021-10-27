// <copyright file="EmbeddedFtpClientFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.Abstractions;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer.Client.Embedded;

public class EmbeddedFtpClientFactory : IFtpClientFactory
{
    private readonly IServiceProvider _serviceProvider;

    public EmbeddedFtpClientFactory(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ValueTask<IFtpClient> CreateClientAsync(
        ConnectionContext connectionContext,
        CancellationToken cancellationToken = default)
    {
        var ftpClient = ActivatorUtilities.CreateInstance<EmbeddedFtpClient>(
            _serviceProvider,
            connectionContext);
        return ValueTask.FromResult<IFtpClient>(ftpClient);
    }
}
