// <copyright file="ServiceCollectionExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.Abstractions;
using FubarDev.FtpServer.Client.Common;
using FubarDev.FtpServer.Client.Embedded;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmbeddedClient(
        this IServiceCollection services)
    {
        return services
            .AddSingleton<IFtpClientFactory, EmbeddedFtpClientFactory>()
            .AddSingleton<IFtpConnectionContextAccessor, FtpConnectionContextAccessor>();
    }
}
