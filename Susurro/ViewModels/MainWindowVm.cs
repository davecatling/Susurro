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
    public class MainWindowVm : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        
        public string? Username
        {
            get { return _userName; }
            private set { _userName = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public int SelectedChatIndex
        {
            get { return _selectedChatIndex; }
            set 
            { 
                _selectedChatIndex = value;
                for (int i = 0; i < ChatVms.Count; i++)
                    ChatVms[i].Selected = (_selectedChatIndex == i);
                OnPropertyChanged(nameof(SelectedChatIndex));
            }
        }

        public ObservableCollection<ChatVm> ChatVms
        {
            get { return _chatVms; }
        }

        private readonly ObservableCollection<ChatVm> _chatVms;
        private string? _userName;
        private int _selectedChatIndex;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly SusurroMain _susurroMain;

        public MainWindowVm() 
        {
            _susurroMain = new SusurroMain();
            _chatVms = [];
            foreach (var chat in _susurroMain.Chats)
            {                
                ChatVms.Add(new ChatVm(chat));
            }
            SelectedChatIndex = 0;
            OnPropertyChanged(nameof(ChatVms));
            _susurroMain.LoginSuccess += LoginSuccess;
            _susurroMain.ChatAdded += ChatAdded;
            LoginAsync("dave", "P@ssw0rd4D@ve");
        }

        private void ChatAdded(object sender, ChatAddedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                ChatVms.Add(new ChatVm(e.Chat));
            }));
        }

        private async void LoginAsync(string username, string password)
        {
            await _susurroMain.LoginAsync(username, password);
        }

        private void LoginSuccess(object? sender, EventArgs e)
        {
            Username = _susurroMain?.Username;
        }
    }
}
