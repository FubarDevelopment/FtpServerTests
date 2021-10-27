// <copyright file="Program.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Net;

using FtpServerRestart01;

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;

using var host = new HostBuilder()
    .ConfigureDefaults(args)
    .UseConsoleLifetime()
    .ConfigureServices(services =>
    {
        services
            .AddLogging()
            .AddOptions()
            .AddSingleton<IConnectionListenerFactory, SocketTransportFactory>()
            //.AddEmbeddedClient()
            .AddExternalClient()
            .AddSingleton<IHostedService, FtpServerService>();
        services
            .Configure<FtpServerOptions>(opt => { opt.ListenEndPoints.Add(new IPEndPoint(IPAddress.IPv6Any, 8021)); });
    })
    .Build();

await host.RunAsync();

Console.WriteLine("End!");
