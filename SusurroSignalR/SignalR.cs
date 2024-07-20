using Microsoft.AspNetCore.SignalR.Client;
using SusurroHttp;
using System;
using System.Net.Http;
using System.Runtime.InteropServices;

namespace SusurroSignalR
{
    public class SignalR(string name, string password, IComms http)
    {
        private HubConnection? _connection;
        private readonly IComms _http = http;
        private string _name = name;
        private string _password = password;

        private HubConnection Connection
        {
            get
            {
                _connection ??= new HubConnectionBuilder()
                        .WithUrl(_http.BaseUrl!)
                        .WithAutomaticReconnect()
                        .Build();
                return _connection;
            }
        }

        public async void ConnectAsync()
        {
            await Connection.StartAsync();
            if (Connection.State == HubConnectionState.Connected)
            {
                await _http.PutConIdAsync(_name, _password, Connection.ConnectionId!);
                Connection.On<string>("newMessage", (message) =>
                {
                    Console.WriteLine(message);
                });
            }
        }
    }
}
