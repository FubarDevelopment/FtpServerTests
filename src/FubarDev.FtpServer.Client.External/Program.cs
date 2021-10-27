// <copyright file="Program.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.Abstractions;
using FubarDev.FtpServer.Client.Common;
using FubarDev.FtpServer.Client.External;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using Nerdbank.Streams;

var inputStream = Console.OpenStandardInput();
var outputStream = Console.OpenStandardOutput();

await using var transportStream = FullDuplexStream.Splice(inputStream, outputStream);
await using var multiplexer = await MultiplexingStream.CreateAsync(
    transportStream,
    new MultiplexingStream.Options()
    {
        ProtocolMajorVersion = 3,
    });

var serverTransportChannel = await multiplexer.AcceptChannelAsync("ftpserver.transport");
var serverControlChannel = await multiplexer.AcceptChannelAsync("ftpserver.control");

var cts = new CancellationTokenSource();
var pipeReader = serverTransportChannel.Input;
var pipeWriter = serverTransportChannel.Output
    .OnCompleted((_, _) => cts.Cancel());
var duplexPipe = new DuplexPipe(pipeReader, pipeWriter);

var host = new HostBuilder()
    .ConfigureDefaults(args)
    .UseConsoleLifetime()
    .ConfigureServices(services =>
    {
        services.Configure<ConsoleLoggerOptions>(
            opt => opt.LogToStandardErrorThreshold = LogLevel.Trace);
        services.AddSingleton<IFtpConnectionContextAccessor, FtpConnectionContextAccessor>();
        services.AddSingleton<FtpConnection>();
        services.AddSingleton(new ExternalConnectionOptions(serverControlChannel));
        services.AddSingleton<IHostedService, HostedFtpConnection>();
    })
    .Build();

var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("FubarDev.FtpServer.Client.External.Program");

// Set FTP connection context
var connectionContext = new FtpConnectionContext(duplexPipe);
var contextAccessor = host.Services.GetRequiredService<IFtpConnectionContextAccessor>();
contextAccessor.Context = connectionContext;

logger.LogTrace("Client host starting");
await host.RunAsync(cts.Token);
logger.LogTrace("Client host stopped");

return 0;
