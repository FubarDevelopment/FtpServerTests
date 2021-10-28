// <copyright file="ExternalFtpClientData.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO.Pipelines;

using CliWrap;

using FubarDev.FtpServer.Abstractions;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

using Nerdbank.Streams;

using StreamJsonRpc;

namespace FubarDev.FtpServer.Client.External;

internal class ExternalFtpClientData : IAsyncDisposable
{
    private readonly ExternalFtpClientFactory _clientFactory;
    private readonly ILogger _logger;
    private Stream? _clientTransportStream;
    private Stream? _serverTransportStream;
    private MultiplexingStream? _multiplexer;
    private IFtpClientControl? _clientControl;
    private CommandTask<CommandResult>? _commandTask;
    private MultiplexingStream.Channel? _clientTransportChannel;
    private MultiplexingStream.Channel? _clientControlChannel;

    public ExternalFtpClientData(
        ConnectionContext connectionContext,
        ExternalFtpClientFactory clientFactory,
        ILogger logger)
    {
        ConnectionContext = connectionContext;
        _clientFactory = clientFactory;
        _logger = logger;
    }

    public ConnectionContext ConnectionContext { get; }

    public Stream ServerTransportStream => _serverTransportStream ?? throw new InvalidOperationException();
    
    public IFtpClientControl ClientControl => _clientControl ?? throw new InvalidOperationException();
    
    public CommandTask<CommandResult> CommandTask => _commandTask ?? throw new InvalidOperationException();

    public MultiplexingStream.Channel ClientTransportChannel =>
        _clientTransportChannel ?? throw new InvalidOperationException();

    public async ValueTask InitializeAsync(CancellationToken cancellationToken)
    {
        var clientInputPipe = new Pipe();
        var clientOutputPipe = new Pipe();

        var clientTransportPipe = new DuplexPipe(clientInputPipe.Reader, clientOutputPipe.Writer);
        _clientTransportStream = clientTransportPipe.AsStream();
        _commandTask = _clientFactory.CreateCommand()
            // .WithCredentials(cb => cb.SetUserName("fubar-coder"))
            .WithStandardOutputPipe(PipeTarget.ToStream(_clientTransportStream))
            .WithStandardInputPipe(PipeSource.FromStream(_clientTransportStream))
            .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
            .ExecuteAsync(CancellationToken.None);

        var serverTransportPipe = new DuplexPipe(clientOutputPipe.Reader, clientInputPipe.Writer);
        _serverTransportStream = serverTransportPipe.AsStream();
        _multiplexer = await MultiplexingStream.CreateAsync(
            _serverTransportStream,
            new MultiplexingStream.Options()
            {
                ProtocolMajorVersion = 3,
            },
            cancellationToken);

        _clientTransportChannel = await _multiplexer.OfferChannelAsync(
            "ftpserver.transport", cancellationToken);
        _clientControlChannel = await _multiplexer.OfferChannelAsync(
            "ftpserver.control", cancellationToken);

        var clientControlPipe = new DuplexPipe(
            _clientControlChannel.Input,
            _clientControlChannel.Output);
        // _clientControlStream = clientControlChannel.AsStream();

        // IJsonRpcMessageHandler handler =
        //     new LengthHeaderMessageHandler(clientControlPipe, new MessagePackFormatter());
        IJsonRpcMessageHandler handler =
            new HeaderDelimitedMessageHandler(clientControlPipe, new JsonMessageFormatter());
        var rpc = new JsonRpc(handler);
        _clientControl = rpc.Attach<IFtpClientControl>();
        rpc.StartListening();
    }

    public async ValueTask DisposeAsync()
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        (_clientControl as IDisposable)?.Dispose();

        if (_multiplexer != null)
        {
            await _multiplexer.DisposeAsync();
        }

        if (_serverTransportStream != null)
        {
            await _serverTransportStream.DisposeAsync();
        }

        if (_commandTask != null)
        {
            await _commandTask;
        }

        if (_clientTransportStream != null)
        {
            await _clientTransportStream.DisposeAsync();
        }
    }
}
