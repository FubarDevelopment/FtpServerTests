// <copyright file="LoggingBuilderExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Client.External;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder RemoveLogger<T>(
        this ILoggingBuilder loggingBuilder)
        where T : class, ILoggerProvider
    {
        for (var i = loggingBuilder.Services.Count - 1; i >= 0; --i)
        {
            var descriptor = loggingBuilder.Services[i];
            if (descriptor.ServiceType == typeof(ILoggerProvider)
                && descriptor.ImplementationType == typeof(T))
                loggingBuilder.Services.RemoveAt(i);
        }

        return loggingBuilder;
    }
}
