// <copyright file="EmbeddedFtpClientControl.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.Abstractions;
using FubarDev.FtpServer.Client.Common;

namespace FubarDev.FtpServer.Client.Embedded;

internal class EmbeddedFtpClientControl : IFtpClientControl
{
    private readonly FtpConnection _connection;

    public EmbeddedFtpClientControl(FtpConnection connection)
    {
        _connection = connection;
    }

    public Task PingAsync(CancellationToken cancellationToken = default)
    {
        _connection.Ping();
        return Task.CompletedTask;
    }

    public Task<DateTimeOffset> GetLastActivityAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_connection.GetLastActivity());
    }

    public Task<TimeSpan> GetInactivityAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_connection.GetInactivity());
    }
}
