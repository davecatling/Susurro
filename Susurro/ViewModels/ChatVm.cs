using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Susurro.Models;

namespace Susurro.ViewModels
{
    public class ChatVm : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<Message> Messages { get; } = [];

        private readonly Chat _chat;
        private string? _participants;
        private string? _newParticipant;
        private string? _plainText;
        private ICommand? _addParticipantCommand;
        private ICommand? _sendMessageCommand;

        public ChatVm(Chat chat)
        {
            _chat = chat;
            _chat.MessageReceived += MessageReceived;
            _chat.ParticipantAdded += ParticipantAdded;
        }

        public string? NewParticipant
        {
            get => _newParticipant;
            set
            {
                _newParticipant = value;
                OnPropertyChanged(nameof(NewParticipant));
            }
        }

        public string? PlainText
        {
            get => _plainText;
            set
            {
                _plainText = value;
                OnPropertyChanged(nameof(PlainText));
            }
        }

        public ICommand AddParticipantCommand
        {
            get
            {
                _addParticipantCommand ??= new RelayCommand((exec) => AddParticipant());
                return _addParticipantCommand;
            }
        }

        public ICommand SendMessageCommand
        {
            get
            {
                _sendMessageCommand ??= new RelayCommand((exec) => SendMessageAsync());
                return _sendMessageCommand;
            }
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
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                Messages.Add(e.Message);
            }));
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddParticipant()
        {
            if (NewParticipant != null)
            {
                _chat.AddParticipant(NewParticipant);
                NewParticipant = null;
            }
        }

        private async void SendMessageAsync()
        {
            if (PlainText != null)
            {
                await _chat.SendMessageAsync(PlainText);
                PlainText = null;
            }
        }
    }
}
