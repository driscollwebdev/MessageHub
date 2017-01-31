namespace MessageHub.Msmq
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    public class LocalUser : INotifyPropertyChanged
    {
        public Guid Id { get; } = Guid.NewGuid();

        private string _username;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Username
        {
            get
            {
                return _username ?? "Guest";
            }
            set
            {
                if (_username != value)
                {
                    _username = value ?? "Guest";
                    OnPropertyChanged();
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }

    [Serializable]
    public class RemoteUser
    {
        public Guid Id { get; set; }

        public string Username { get; set; }
    }
}
