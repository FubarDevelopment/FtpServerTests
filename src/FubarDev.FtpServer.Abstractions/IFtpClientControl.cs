// <copyright file="IFtpClientControl.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Abstractions;

public interface IFtpClientControl
{
    Task PingAsync(CancellationToken cancellationToken = default);
    Task<DateTimeOffset> GetLastActivityAsync(CancellationToken cancellationToken = default);
    Task<TimeSpan> GetInactivityAsync(CancellationToken cancellationToken = default);
}
