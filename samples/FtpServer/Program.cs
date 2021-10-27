// <copyright file="Program.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Net;

using FtpServer;

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;

using Quartz;

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
            .AddSingleton<IHostedService, FtpServerService>()
            .AddSingleton<IFtpClientManager, FtpClientManager>()
            .AddQuartz(cfg =>
            {
                cfg.UseMicrosoftDependencyInjectionJobFactory();
                cfg.AddJob<FtpClientInactivityJob>(
                    jc => jc.WithIdentity("client-cleanup", "ftp-server"));
                cfg.AddTrigger(tc =>
                {
                    tc.WithSimpleSchedule(b =>
                            b.WithInterval(TimeSpan.FromSeconds(1))
                                .RepeatForever())
                        .ForJob("client-cleanup", "ftp-server")
                        .WithIdentity("client-cleanup-trigger", "ftp-server");
                });
            })
            .AddQuartzServer();
        services
            .Configure<FtpServerOptions>(opt => { opt.ListenEndPoints.Add(new IPEndPoint(IPAddress.IPv6Any, 8021)); });
    })
    .Build();

await host.RunAsync();

Console.WriteLine("End!");
