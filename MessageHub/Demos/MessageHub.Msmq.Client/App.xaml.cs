using MessageHub.Hubs;
using MessageHub.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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

            AppHub = MsmqMessageHub.Create()
                                   .WithRemoteQueuePath(@"FormatName:DIRECT=OS:BDRISCOLL-PC2\private$\AppHub");

            ((RemoteMessageHub)AppHub).Connect().Wait();

            AppHub.Channel("Users").AddReceiver("Add", (userData) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    RemoteUser user = userData as RemoteUser;
                    if (user != null)
                    {
                        RemoteUser existing = Session.Current.AllUsers.FirstOrDefault(u => u.Id == user.Id);
                        if (existing != null)
                        {
                            App.Current.Dispatcher.Invoke(() => Session.Current.AllUsers.Remove(existing));
                        }

                        App.Current.Dispatcher.Invoke(() => Session.Current.AllUsers.Add(user));
                    }
                });
            }, channelReceiverId);

            App.AppHub.Channel("Users").AddReceiver("Remove", (userData) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    RemoteUser user = userData as RemoteUser;
                    if (user != null)
                    {
                        RemoteUser existing = Session.Current.AllUsers.FirstOrDefault(u => u.Id == user.Id);
                        if (existing != null)
                        {
                            App.Current.Dispatcher.Invoke(() => Session.Current.AllUsers.Remove(existing));
                        }
                    }
                });
            }, channelReceiverId);

            App.AppHub.Channel("Content").AddReceiver("Update", (contentData) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    string content = contentData as string;
                    if (content != null)
                    {
                        Session.Current.Content = content;
                    }
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
    }
}
