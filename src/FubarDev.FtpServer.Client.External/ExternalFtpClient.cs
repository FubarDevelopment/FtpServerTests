// <copyright file="ExternalFtpClient.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using CliWrap;

using FubarDev.FtpServer.Abstractions;

namespace FubarDev.FtpServer.Client.External;

internal class ExternalFtpClient : IFtpClient
{
    private readonly CommandTask<CommandResult> _executionTask;

    public ExternalFtpClient(
        CommandTask<CommandResult> executionTask)
    {
        _executionTask = executionTask;
    }

    public async ValueTask RunAsync(CancellationToken cancellationToken = default)
    {
        await _executionTask;
    }
}
