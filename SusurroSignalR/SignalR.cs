using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Net.Http;
using System.Runtime.InteropServices;

namespace SusurroSignalR
{
    public class SignalR
    {

        public string? SignalRurl { get; set; }

        private HubConnection? _connection;

        private HubConnection Connection
        {
            get
            {
                _connection ??= new HubConnectionBuilder()
                        .WithUrl(SignalRurl!)
                        .WithAutomaticReconnect()
                        .Build();
                return _connection;
            }
        }

        public async void ConnectAsync()
        {
            //Connection.On<string, string>("message", (user, message) =>
            await Connection.StartAsync();
        }
    }
}
