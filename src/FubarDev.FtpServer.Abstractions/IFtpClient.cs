// <copyright file="IFtpClient.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Abstractions;

public interface IFtpClient
{
    ValueTask RunAsync(CancellationToken cancellationToken = default);
}
