using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Susurro.Models
{
    public class Chat()
    {
        private readonly List<string> _participants = [];

        public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
        public delegate void ParticipantAddedEventHandler(object sender, ParticipantAddedEventArgs e);
        public event MessageReceivedEventHandler? MessageReceived;
        public event ParticipantAddedEventHandler? ParticipantAdded;

        public override string ToString()
        {
            return Participants;
        }

        public List<Message>? Messages { get; private set; }

        public string Participants
        {
            get
            {
                var result = string.Empty;
                if (_participants.Count != 0)
                {
                    _participants.Sort();
                    foreach (var item in _participants)
                    {
                        result += $"{item} ";
                    }
                    return result.Trim();
                }
                else
                    return "New chat";
            }
        }

        public void AddMessage(Message message)
        {
            Messages ??= [];
            Messages.Add(message);
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        }

        public void AddParticipant(string participant)
        {
            if (_participants.Count <= 6)
            {
                _participants.Add(participant);
                ParticipantAdded?.Invoke(this, new ParticipantAddedEventArgs(participant));
            }
            else
                throw new InvalidOperationException("Maximum chat participants reached");
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
