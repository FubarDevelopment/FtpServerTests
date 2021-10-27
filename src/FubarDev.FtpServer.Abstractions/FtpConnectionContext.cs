// <copyright file="FtpConnectionContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO.Pipelines;

namespace FubarDev.FtpServer.Abstractions;

public class FtpConnectionContext
{
    public FtpConnectionContext(IDuplexPipe controlConnection)
    {
        ControlConnection = controlConnection;
    }

    public IDuplexPipe ControlConnection { get; }
}
