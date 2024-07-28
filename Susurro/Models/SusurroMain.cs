using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SusurroHttp;
using SusurroRsa;
using SusurroSignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;

namespace Susurro.Models
{
    public class SusurroMain
    {
        private readonly Http _http;
        private readonly Rsa _rsa;
        private SignalR? _signalR;
        private string? _username;
        private string? _password;

        public string? Username { get { return _username; } }

        public List<Chat> Chats { get; }
        public event EventHandler? LoginSuccess;

        public SusurroMain()
        {
            HostApplicationBuilder builder = new();
            builder.Configuration.Sources.Clear();
            builder.Configuration.AddJsonFile("appsettings.json", false);
            _http = new Http();
            builder.Configuration.GetSection("Http").Bind(_http);
            _rsa = new Rsa(_http);
            Chats = [new Chat(), new Chat()];  
        }

        public async Task LoginAsync (string name, string password)
        {
            var result = await _http!.Login(name, password);
            if (result.IsSuccessStatusCode)
            {
                _username = name;
                _password = password;
                _signalR = new SignalR(name, password, _http);
                _signalR.ConnectAsync();
                _signalR.MsgIdReceived += SignalRmsgIdReceived;
                LoginSuccess?.Invoke(this, new EventArgs());
                Chats[0].AddParticipant("tom");
                Chats[0].AddParticipant("jerry");
                Chats[0].AddMessage(new Message("tom", "Hi from Tom", DateTime.Now));
            }
            else
            {
                using var streamReader = new StreamReader(result.Content.ReadAsStream());
                throw new Exception(streamReader.ReadToEnd());
            }
        }

        private void SignalRmsgIdReceived(object sender, MsgIdReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }    
}
