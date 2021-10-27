// <copyright file="ExternalConnectionOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Nerdbank.Streams;

namespace FubarDev.FtpServer.Client.External;

internal record ExternalConnectionOptions(
    MultiplexingStream.Channel ServerControlChannel);
