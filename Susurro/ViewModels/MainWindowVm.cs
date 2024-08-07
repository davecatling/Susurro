using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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

        private string? _loginName;
        private string? _createName;
        private string? _loginPassword;
        private string? _createPassword1;
        private string? _createPassword2;
        public bool PasswordsLocked { get; set; }
        public string? LoginName
        {
            get { return _loginName; }
            set
            {
                _loginName = value;
                OnPropertyChanged(nameof(LoginName));
            }
        }

        public string? CreateName
        {
            get { return _createName; }
            set
            {
                _createName = value;
                OnPropertyChanged(nameof(CreateName));
            }
        }

        public string? LoginPassword
        {
            get { return _loginPassword; }
            set
            {
                if (!PasswordsLocked)
                {
                    _loginPassword = value;
                    OnPropertyChanged(nameof(LoginPassword));
                }
            }
        }

        public string? CreatePassword1
        {
            get { return _createPassword1; }
            set
            {
                if (!PasswordsLocked)
                {
                    _createPassword1 = value;
                    OnPropertyChanged(nameof(CreatePassword1));
                }
            }
        }
        public string? CreatePassword2
        {
            get { return _createPassword2; }
            set
            {
                if (!PasswordsLocked)
                {
                    _createPassword2 = value;
                    OnPropertyChanged(nameof(CreatePassword2));
                }
            }
        }

        private ObservableCollection<ChatVm>? _chatVms;
        private ICommand? _loginCommand;
        private ICommand? _logoutCommand;
        private ICommand? _createUserCommand;
        private string? _userName;
        private int _selectedChatIndex;

        public ObservableCollection<ChatVm>? ChatVms
        {
            get { return _chatVms; }
            set { _chatVms = value; }
        }

        public ICommand LoginCommand
        {
            get
            {
                _loginCommand ??= new RelayCommand(async (exec) => await LoginAsync());
                return _loginCommand;
            }
        }

        public ICommand LogoutCommand
        {
            get
            {
                _logoutCommand ??= new RelayCommand(async (exec) => await LogoutAsync());
                return _logoutCommand;
            }
        }

        public ICommand CreateUserCommand
        {
            get
            {
                _createUserCommand ??= new RelayCommand(async (exec) => await CreateUserAsync());
                return _createUserCommand;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly SusurroMain _susurroMain;

        public MainWindowVm() 
        {
            _susurroMain = new SusurroMain();
            ChatVms = [];
            OnPropertyChanged(nameof(ChatVms));
            _susurroMain.LoginSuccess += LoginSuccess;
            _susurroMain.LogoutSuccess += LogoutSuccess;
            _susurroMain.ChatAdded += ChatAdded;
        }

        private void ChatAdded(object sender, ChatAddedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                ChatVms.Add(new ChatVm(e.Chat));
            }));
        }

        private void LoginSuccess(object? sender, EventArgs e)
        {
            Username = _susurroMain!.Username;
            ClearLoginValues();
            _chatVms.Clear();
            foreach (var chat in _susurroMain!.Chats)
            {
                ChatVms.Add(new ChatVm(chat));
            }
            SelectedChatIndex = 0;
        }

        private void LogoutSuccess(object? sender, EventArgs e)
        {
            Username = null;
            ClearLoginValues();
            while (ChatVms.Count > 0)
                ChatVms.Remove(ChatVms.First());
        }

        private void ClearLoginValues()
        {
            LoginName = null;
            LoginPassword = null;
            CreateName = null;
            CreatePassword1 = null;
            CreatePassword2 = null;
        }

        private async Task LoginAsync()
        {
            try
            {
                if (String.IsNullOrEmpty(LoginName) || String.IsNullOrEmpty(LoginPassword)) return;
                await _susurroMain.LoginAsync(LoginName!, LoginPassword!);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }
        }

        private async Task CreateUserAsync()
        {
            try
            {
                if (CreateName == null || CreatePassword1 == null) return;
                if (CreatePassword1 != CreatePassword2)
                {
                    DisplayError("The provided passwords do not match.");
                    return;
                }
                await _susurroMain.CreateUserAsync(CreateName, CreatePassword1);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }
        }

        private async Task LogoutAsync()
        {
            await _susurroMain.LogoutAsync();            
        }

        private static void DisplayError(string message)
        {
            MessageBox.Show(message, "Susurro", MessageBoxButton.OK);
        }
    }
}
