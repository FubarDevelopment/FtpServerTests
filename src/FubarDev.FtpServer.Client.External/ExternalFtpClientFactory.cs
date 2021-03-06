// <copyright file="ExternalFtpClientFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Reflection;

using CliWrap;
using CliWrap.Builders;

using FubarDev.FtpServer.Abstractions;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Client.External;

public class ExternalFtpClientFactory : IFtpClientFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExternalFtpClientFactory> _logger;
    private readonly Assembly _clientAssembly;
    private readonly string _clientExecutable;
    private readonly bool _startedWithDotnetTool;

    public ExternalFtpClientFactory(
        IServiceProvider serviceProvider,
        ILogger<ExternalFtpClientFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        var startExecutable = Path.GetFileName(Environment.ProcessPath)
                              ?? throw new InvalidOperationException();
        _startedWithDotnetTool = startExecutable.ToLowerInvariant() switch
        {
            null => throw new InvalidOperationException(),
            "dotnet" => true,
            "dotnet.exe" => true,
            _ => false
        };

        _clientAssembly = typeof(ExternalFtpClient).Assembly;
        _clientExecutable = startExecutable.ToLowerInvariant() switch
        {
            null => throw new InvalidOperationException(),
            "dotnet" => Environment.ProcessPath ?? throw new InvalidOperationException(),
            "dotnet.exe" => Environment.ProcessPath ?? throw new InvalidOperationException(),
            _ => ProbeExecutableFor(_clientAssembly) ?? throw new InvalidOperationException()
        };
    }

    public async ValueTask<IFtpClient> CreateClientAsync(
        ConnectionContext connectionContext,
        CancellationToken cancellationToken = default)
    {
        var data = new ExternalFtpClientData(connectionContext, this, _logger);
        await data.InitializeAsync(cancellationToken);
        var ftpClient = ActivatorUtilities.CreateInstance<ExternalFtpClient>(_serviceProvider, data);
        return ftpClient;
    }

    internal Command CreateCommand(Action<ArgumentsBuilder>? configure = null)
    {
        return Cli.Wrap(_clientExecutable)
            .WithArguments(ab =>
            {
                if (_startedWithDotnetTool)
                {
                    ab.Add(_clientAssembly.Location);
                    ab.Add("--");
                }
                
                configure?.Invoke(ab);
            })
            .WithValidation(CommandResultValidation.None);
    }

    private static string? ProbeExecutableFor(Assembly assembly)
    {
        var directory = Path.GetDirectoryName(assembly.Location)
                        ?? throw new InvalidOperationException();
        var fileName = Path.GetFileName(assembly.Location);
        var exeFileName = OperatingSystem.IsWindows()
            ? Path.ChangeExtension(fileName, "exe")
            : Path.GetFileNameWithoutExtension(fileName);
        var exePath = Path.Combine(directory, exeFileName);
        return File.Exists(exePath)
            ? exePath
            : null;
    }
}
