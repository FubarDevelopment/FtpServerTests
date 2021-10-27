// <copyright file="FtpClientInformation.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.Abstractions;

using Microsoft.AspNetCore.Connections;

namespace FtpServerRestart01;

public record FtpClientInformation(
    IFtpClient Client,
    ConnectionContext ConnectionContext);
