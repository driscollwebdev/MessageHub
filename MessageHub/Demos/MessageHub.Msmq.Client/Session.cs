namespace MessageHub.Msmq.Client
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    internal class Session : INotifyPropertyChanged
    {
        private static Lazy<Session> _current = new Lazy<Session>(() => new Session());

        public event PropertyChangedEventHandler PropertyChanged;

        public static Session Current
        {
            get
            {
                return _current.Value;
            }
        }

        private Session()
        {
            AllUsers.Add(new RemoteUser { Id = User.Id, Username = User.Username });
        }

        private Guid _receiverId = Guid.NewGuid();

        private string _currentContent = string.Empty;

        public string Content
        {
            get
            {
                return _currentContent;
            }
            set
            {
                if (_currentContent != value)
                {
                    _currentContent = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        private LocalUser _user = new LocalUser();

        public LocalUser User
        {
            get
            {
                return _user;
            }

            set
            {
                if (_user != value)
                {
                    _user = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<RemoteUser> AllUsers { get; set; } = new ObservableCollection<RemoteUser>();

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
