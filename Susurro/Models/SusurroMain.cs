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
using SusurroDtos;

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
        public delegate void ChatAddedEventHandler(object sender, ChatAddedEventArgs e);
        public event ChatAddedEventHandler? ChatAdded;
        public event EventHandler? LoginSuccess;

        public SusurroMain()
        {
            HostApplicationBuilder builder = new();
            builder.Configuration.Sources.Clear();
            builder.Configuration.AddJsonFile("appsettings.json", false);
            _http = new Http();
            builder.Configuration.GetSection("Http").Bind(_http);
            _rsa = new Rsa(_http);
            Chats = [];
            AddNewChat();
        }

        private void AddNewChat()
        {
            var newChat = new Chat(this);
            newChat.ParticipantAdded += ChatParticipantAdded;
            Chats.Add(newChat);
            ChatAdded?.Invoke(this, new ChatAddedEventArgs(newChat));
        }

        private void ChatParticipantAdded(object sender, ParticipantAddedEventArgs e)
        {
            if (!Chats.Any(c => c.Participants == null))
                AddNewChat();
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
            }
            else
            {
                using var streamReader = new StreamReader(result.Content.ReadAsStream());
                throw new Exception(streamReader.ReadToEnd());
            }
        }

        public async Task SendMessageAsync(string participants, string plainText)
        {
            if (_username == null || _password == null)
                throw new InvalidOperationException("Username or password not set");
            var messages = new List<MessageDto>();
            var participantsArr = participants.Split(' ');
            for (var i = 0; i < participantsArr.Length; i++)
            {
                var to = participantsArr[i];
                byte[]? cypherText = null;
                byte[]? signature = null;
                cypherText = await _rsa.EncryptAsync(plainText!, to);
                signature = Rsa.Sign(plainText!, _username, _password);
                messages.Add(new MessageDto()
                {
                    From = _username,
                    To = to,
                    Text = cypherText!,
                    Signature = signature!
                });
            }
            messages.ForEach(m => m.AllTo = participants);
            var result = await _http!.SendMsgAsync(messages);
            if (!result.IsSuccessStatusCode)
                throw new Exception(await result.Content.ReadAsStringAsync());
        }

        private async void SignalRmsgIdReceived(object sender, MsgIdReceivedEventArgs e)
        {
            if (_username == null || _password == null) return;
            var message = await GetMessageAsync(e.Id);
            AddMessageToChat(message);
        }

        private async Task<Message> GetMessageAsync(string id)
        {
            var messageDto = await _http.GetMsgAsync(id);
            var plainText = Rsa.Decrypt(messageDto.Text, _username!, _password!);
            return new Message(messageDto, plainText);
        }

        private void AddMessageToChat(Message message)
        {
            var requiredParticipants = new List<string>(message.AllTo!);
            requiredParticipants.Remove(_username!);
            requiredParticipants.Add(message.From);
            var chatFound = false;
            foreach (var chat in Chats.Where(c => c.Participants != null))
            {                
                var participants = chat.Participants!.Split(' ').ToList();
                if (participants.Count == requiredParticipants.Count)
                {
                    if (participants.All(p => requiredParticipants.Contains(p)))
                    {
                        chat.AddMessage(message);
                        chatFound = true;
                    }
                }
            }
            if (!chatFound)
            {
                if (!Chats.Any(c => c.Participants == null))
                    AddNewChat();
                var chat = Chats.FirstOrDefault(c => c.Participants == null);
                if (chat != null)
                {
                    requiredParticipants.ForEach(p => chat.AddParticipant(p));
                    chat.AddMessage(message);
                }
            }
        }
    }

    public class ChatAddedEventArgs(Chat chat) : EventArgs
    {
        public Chat Chat { get; } = chat;
    }
}
