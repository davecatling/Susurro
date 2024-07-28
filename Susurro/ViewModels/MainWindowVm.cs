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

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SusurroMain SusurroMain { get; }

        public MainWindowVm() 
        {
            SusurroMain = new SusurroMain();
            OnPropertyChanged(nameof(SusurroMain));
        }
            
    }
}
