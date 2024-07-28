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

        public ObservableCollection<ChatVm> ChatVms
        {
            get { return _chatVms; }
        }

        private readonly ObservableCollection<ChatVm> _chatVms;
        private string? _userName;

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
            OnPropertyChanged(nameof(ChatVms));
            _susurroMain.LoginSuccess += LoginSuccess;
            LoginAsync("dave", "P@ssw0rd4D@ve");
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
