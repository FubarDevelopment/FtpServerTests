// <copyright file="IFtpClientFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Connections;

namespace FubarDev.FtpServer.Abstractions;

public interface IFtpClientFactory
{
    ValueTask<IFtpClient> CreateClientAsync(
        ConnectionContext connectionContext,
        CancellationToken cancellationToken = default);
}
