using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Susurro.Models
{
    public class Chat (SusurroMain susurroMain) : INotifyPropertyChanged
    {
        private SusurroMain _susurroMain = susurroMain;
        private string? _chatText;
        private List<string> _participants = [];

        public event PropertyChangedEventHandler? PropertyChanged;

        public override string ToString()
        {
            return Participants;
        }

        public string Participants
        {
            get
            {
                var result = string.Empty;
                if (_participants.Any())
                {
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

        public bool AddParticipant(string participant)
        {
            if (_participants.Count >= 6)
                return false;
            _participants.Add(participant);
            OnPropertyChanged(nameof(Participants));
            return true;
        }

        private void OnPropertyChanged(string? property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }               

        public string? ChatText
        {
            get
            {
                return _chatText;
            }
            set
            {
                _chatText = value;
                OnPropertyChanged(nameof(ChatText));
            }
        }        
    }
}
