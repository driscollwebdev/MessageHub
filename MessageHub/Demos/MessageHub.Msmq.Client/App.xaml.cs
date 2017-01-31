using MessageHub.Hubs;
using MessageHub.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using StackExchange.Redis;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MessageHub.Msmq.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IMessageHub AppHub { get; private set; }

        private static Guid channelReceiverId = Guid.NewGuid();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            PopulateCurrentSessionData();

            AppHub = MsmqMessageHub.Create()
                                   .WithRemoteQueuePath(@"FormatName:DIRECT=OS:BDRISCOLL-PC2\private$\AppHub");

            ((RemoteMessageHub)AppHub).Connect().Wait();

            AppHub.Channel("Users").AddReceiver("Add", (userData) =>
            {
                return Task.Factory.StartNew(async () =>
                {
                    RemoteUser user = userData as RemoteUser;
                    if (user != null)
                    {
                        RemoteUser existing = Session.Current.AllUsers.FirstOrDefault(u => u.Id == user.Id);
                        if (existing != null)
                        {
                            await App.Current.Dispatcher.InvokeAsync(() => Session.Current.AllUsers.Remove(existing));
                        }

                        await App.Current.Dispatcher.InvokeAsync(() => Session.Current.AllUsers.Add(user));
                    }
                });
            }, channelReceiverId);

            App.AppHub.Channel("Users").AddReceiver("Remove", (userData) =>
            {
                return Task.Factory.StartNew(async () =>
                {
                    RemoteUser user = userData as RemoteUser;
                    if (user != null)
                    {
                        RemoteUser existing = Session.Current.AllUsers.FirstOrDefault(u => u.Id == user.Id);
                        if (existing != null)
                        {
                            await App.Current.Dispatcher.InvokeAsync(() => Session.Current.AllUsers.Remove(existing));
                        }
                    }
                });
            }, channelReceiverId);

            App.AppHub.Channel("Content").AddReceiver("Update", (contentData) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    string content = contentData as string;
                    Session.Current.Content = content ?? string.Empty;
                });
            }, channelReceiverId);

            Session.Current.PropertyChanged += Session_PropertyChanged;
            Session.Current.User.PropertyChanged += User_PropertyChanged;
        }

        private void User_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            LocalUser source = (LocalUser)sender;

            if (e.PropertyName == "Username")
            {
                App.AppHub.Channel("Users").Send("Add", new RemoteUser { Id = source.Id, Username = source.Username });
            }
        }

        private void Session_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Session source = (Session)sender;

            if (e.PropertyName == "Content")
            {
                AppHub.Channel("Content").Send("Update", source.Content);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            AppHub.Channel("Users").Send("Remove", new RemoteUser { Id = Session.Current.User.Id, Username = Session.Current.User.Username });
            AppHub.Channel("Users").RemoveReceiver("Add", channelReceiverId);
            AppHub.Channel("Users").RemoveReceiver("Remove", channelReceiverId);
            AppHub.Channel("Content").RemoveReceiver("Update", channelReceiverId);

            ((RemoteMessageHub)AppHub).Disconnect();
        }

        private void PopulateCurrentSessionData()
        {
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost"))
            {
                IDatabase db = redis.GetDatabase();

                ObservableCollection<RemoteUser> remoteUsers = new ObservableCollection<RemoteUser>(Session.Current.AllUsers.AsEnumerable());

                RedisValue[] serializedUserData = db.SetMembers("AllUsers");
                foreach (string serializedUser in serializedUserData)
                {
                    RemoteUser user = JsonConvert.DeserializeObject<RemoteUser>(serializedUser);
                    RemoteUser existing = remoteUsers.FirstOrDefault(u => u.Id == user.Id);

                    if (existing != null)
                    {
                        remoteUsers.Remove(existing);
                    }

                    remoteUsers.Add(user);
                }

                App.Current.Dispatcher.Invoke(() => { Session.Current.AllUsers = remoteUsers; });

                string content = db.StringGet("Content");

                Session.Current.Content = content;

            }
        }
    }
}
