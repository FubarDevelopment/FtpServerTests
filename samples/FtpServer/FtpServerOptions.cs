// <copyright file="FtpServerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Net;

namespace FtpServerRestart01;

public class FtpServerOptions
{
    public List<IPEndPoint> ListenEndPoints { get; } = new();
}
