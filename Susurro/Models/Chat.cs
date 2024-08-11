using SusurroDtos;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Susurro.Models
{
    public class Chat(SusurroMain susurroMain)
    {
        private readonly SusurroMain _susurroMain = susurroMain;
        private readonly List<string> _participants = [];

        public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
        public delegate void ParticipantAddedEventHandler(object sender, ParticipantAddedEventArgs e);
        public event MessageReceivedEventHandler? MessageReceived;
        public event ParticipantAddedEventHandler? ParticipantAdded;

        public List<Message> Messages { get; private set; } = [];

        public string? Participants
        {
            get
            {
                var result = string.Empty;
                if (_participants.Count != 0)
                {
                    _participants.Sort();
                    foreach (var item in _participants)
                        result += $"{item} ";
                    return result[..^1];
                }
                else
                    return null;
            }
        }

        public void AddMessage(Message message)
        {
            Messages.Add(message);
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        }

        public void AddParticipants(string participants)
        {
            var newParticipants = participants.Trim().Split(' ');
            if ((_participants.Count + newParticipants.Length) > 6)
                throw new InvalidOperationException("Maximum chat participants is six");
            else
            {
                var i = 0;
                while ((_participants.Count < 6) && (i < newParticipants.Length)) 
                {
                    _participants.Add(newParticipants[i]);
                    ParticipantAdded?.Invoke(this, 
                        new ParticipantAddedEventArgs(newParticipants[i]));
                    i++;
                }
            }
        }
        
        public async Task SendMessageAsync(string plainText)
        {
            if (Participants == null)
                throw new InvalidOperationException("No chat participants");
            await _susurroMain.SendMessageAsync(Participants, plainText);
            AddMessage(new Message(_susurroMain.Username!, plainText, DateTime.Now));
        }
    }

    public class MessageReceivedEventArgs(Message message) : EventArgs
    {
        public Message Message { get; } = message;
    }

    public class ParticipantAddedEventArgs(string participant) : EventArgs
    {
        public string Participant { get; } = participant;
    }
}
