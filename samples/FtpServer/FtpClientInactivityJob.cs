// <copyright file="FtpClientInactivityJob.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Quartz;

namespace FtpServer;

public class FtpClientInactivityJob : IJob
{
    private readonly IFtpClientManager _clientManager;
    private readonly TimeSpan _inactivityTimeSpan = TimeSpan.FromSeconds(10);

    public FtpClientInactivityJob(IFtpClientManager clientManager)
    {
        _clientManager = clientManager;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        foreach (var clientInfo in _clientManager.Clients)
        {
            var client = clientInfo.Client;
            // clientInfo.ConnectionContext.ConnectionId;
            var inactivity = await client.Control.GetInactivityAsync(context.CancellationToken);
            if (inactivity >= _inactivityTimeSpan)
            {
                clientInfo.ConnectionContext.Abort();
            }
        }
    }
}
