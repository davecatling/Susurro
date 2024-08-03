using Microsoft.AspNetCore.SignalR.Client;
using SusurroHttp;

namespace SusurroSignalR
{
    public class SignalR(string name, string password, IComms http)
    {
        private HubConnection? _connection;
        private readonly IComms _http = http;
        private readonly string _name = name;
        private readonly string _password = password;

        public delegate void MsgIdReceivedEventHandler(object sender, MsgIdReceivedEventArgs e);
        public event MsgIdReceivedEventHandler? MsgIdReceived;        

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
                await _http.PutConIdAsync(Connection.ConnectionId!);
                Connection.On<string>("newMessage", (message) =>
                {
                    MsgIdReceived?.Invoke(this, new MsgIdReceivedEventArgs(message));
                });
            }
        }
    }

    public class MsgIdReceivedEventArgs(string id) : EventArgs
    {
        public string Id { get; } = id;
    }
}
