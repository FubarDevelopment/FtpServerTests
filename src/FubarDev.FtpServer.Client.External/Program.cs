// <copyright file="Program.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO.Pipelines;

using FubarDev.FtpServer.Abstractions;
using FubarDev.FtpServer.Client.Common;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using Nerdbank.Streams;

var inputStream = Console.OpenStandardInput();
var outputStream = Console.OpenStandardOutput();

var cts = new CancellationTokenSource();
var pipeReader = PipeReader.Create(inputStream);
var pipeWriter = PipeWriter.Create(outputStream)
    .OnCompleted((_, _) => cts.Cancel());
var duplexPipe = new DuplexPipe(pipeReader, pipeWriter);

using var host = new HostBuilder()
    .ConfigureDefaults(args)
    .UseConsoleLifetime()
    .ConfigureServices(services =>
    {
        services.Configure<ConsoleLoggerOptions>(
            opt => opt.LogToStandardErrorThreshold = LogLevel.Trace);
        services.AddSingleton<IFtpConnectionContextAccessor, FtpConnectionContextAccessor>();
        services.AddSingleton<IHostedService, FtpConnection>();
    })
    .Build();

var contextAccessor = host.Services.GetRequiredService<IFtpConnectionContextAccessor>();
var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("FubarDev.FtpServer.Client.External.Program");
var connectionContext = new FtpConnectionContext(duplexPipe);
contextAccessor.Context = connectionContext;

logger.LogTrace("Client host starting");
await host.RunAsync(cts.Token);
logger.LogTrace("Client host stopped");

return 0;
