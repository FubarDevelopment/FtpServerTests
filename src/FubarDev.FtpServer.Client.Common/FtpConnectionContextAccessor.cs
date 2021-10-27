// <copyright file="FtpConnectionContextAccessor.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.Abstractions;

namespace FubarDev.FtpServer.Client.Common;

public class FtpConnectionContextAccessor : IFtpConnectionContextAccessor
{
    private static readonly AsyncLocal<FtpConnectionContextHolder> _contextCurrent = new();

    public FtpConnectionContext? Context
    {
        get => _contextCurrent.Value?.Context;
        set
        {
            var holder = _contextCurrent.Value;
            if (holder != null)
                // Clear current HttpContext trapped in the AsyncLocals, as its done.
                holder.Context = null;

            if (value != null)
                // Use an object indirection to hold the HttpContext in the AsyncLocal,
                // so it can be cleared in all ExecutionContexts when its cleared.
                _contextCurrent.Value = new FtpConnectionContextHolder {Context = value};
        }
    }

    private class FtpConnectionContextHolder
    {
        public FtpConnectionContext? Context;
    }
}
