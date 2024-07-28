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
    public class SusurroMain : INotifyPropertyChanged
    {
        private readonly IComms _http;
        private readonly Rsa _rsa;
        private SignalR _signalR;
        private string _username;
        private string _password;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public ObservableCollection<Chat> Chats { get; }

        public SusurroMain()
        {
            HostApplicationBuilder builder = new();
            builder.Configuration.Sources.Clear();
            builder.Configuration.AddJsonFile("appsettings.json", false);
            _http = new Http();
            builder.Configuration.GetSection("Http").Bind(_http);
            _rsa = new Rsa(_http);
            Chats = [new Chat(this) { ChatText = "Chat 1"}, 
                new Chat(this) { ChatText = "Chat 2"}];
            LoginAsync("dave", "P@ssw0rd4D@ve");
        }

        public async void LoginAsync (string name, string password)
        {
            var result = await _http!.Login(name, password);
            if (result.IsSuccessStatusCode)
            {
                Console.WriteLine($"User {name} logged in.");
                Username = name;
                _password = password;
                _signalR = new SignalR(name, password, _http);
                _signalR.ConnectAsync();
                _signalR.MsgIdReceived += SignalRmsgIdReceived;
            }
            else
            {
                using var streamReader = new StreamReader(result.Content.ReadAsStream());
                Console.WriteLine(streamReader.ReadToEnd());
            }
        }

        private void SignalRmsgIdReceived(object sender, MsgIdReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }    
}
