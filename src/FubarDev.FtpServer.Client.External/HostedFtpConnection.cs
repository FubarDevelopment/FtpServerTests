// <copyright file="HostedFtpConnection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.Client.Common;

using Microsoft.Extensions.Hosting;

using Nerdbank.Streams;

using StreamJsonRpc;

namespace FubarDev.FtpServer.Client.External;

internal class HostedFtpConnection : BackgroundService
{
    private readonly FtpConnection _connection;
    private readonly ExternalConnectionOptions _connectionOptions;

    public HostedFtpConnection(
        FtpConnection connection,
        ExternalConnectionOptions connectionOptions)
    {
        _connection = connection;
        _connectionOptions = connectionOptions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var serverControlPipe = new DuplexPipe(
            _connectionOptions.ServerControlChannel.Input,
            _connectionOptions.ServerControlChannel.Output);
        /*
        var handler = new LengthHeaderMessageHandler(
            serverControlPipe,
            new MessagePackFormatter());
            */
        IJsonRpcMessageHandler handler =
            new HeaderDelimitedMessageHandler(serverControlPipe, new JsonMessageFormatter());

        using var rpc = new JsonRpc(handler, _connection);
        await _connection.RunAsync(stoppingToken);
    }
}
