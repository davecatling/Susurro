using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Susurro.Models;

namespace Susurro.ViewModels
{
    public class ChatVm : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<Message> Messages { get; } = [];

        private readonly Chat _chat;
        private string? _participants;

        public ChatVm(Chat chat)
        {
            _chat = chat;
            _chat.MessageReceived += MessageReceived;
            _chat.ParticipantAdded += ParticipantAdded;
        }

        private void ParticipantAdded(object sender, ParticipantAddedEventArgs e)
        {
            Participants = _chat.Participants;
        }

        public string? Participants
        {
            get { return _participants ?? "new chat"; }
            private set
            {
                _participants = value;
                OnPropertyChanged(nameof(Participants));
            }
        }

        private void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Messages.Add(e.Message);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return _chat.ToString();
        }
    }
}
