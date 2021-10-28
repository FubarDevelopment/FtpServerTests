// <copyright file="IFtpClientControl.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Abstractions;

public interface IFtpClientControl
{
    Task Ping(CancellationToken cancellationToken = default);
    Task<DateTimeOffset> GetLastActivity(CancellationToken cancellationToken = default);
    Task<TimeSpan> GetInactivity(CancellationToken cancellationToken = default);
}
