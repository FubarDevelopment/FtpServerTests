// <copyright file="IFtpConnectionContextAccessor.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Abstractions;

public interface IFtpConnectionContextAccessor
{
    FtpConnectionContext? Context { get; set; }
}
