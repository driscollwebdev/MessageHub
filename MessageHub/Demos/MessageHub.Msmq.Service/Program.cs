using MessageHub.Msmq;
using MessageHub.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace MessageHub.Msmq.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            using (MsmqMessageHubService service = new MsmqMessageHubService(".\\private$\\AppHub", new AppConnectedClientRepository<MsmqConnectedClient>()))
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost"))
            {
                MsmqMessageHub stateHub = MsmqMessageHub.Create().WithRemoteQueuePath(".\\private$\\AppHub");
                stateHub.Connect().Wait();

                Guid stateHubChannelId = Guid.NewGuid();

                stateHub.Channel("Users").AddReceiver("Add", (userData) =>
                {
                    return Task.Factory.StartNew(async () =>
                    {
                        RemoteUser user = userData as RemoteUser;
                        if (user != null)
                        {
                            string serializedUser = JsonConvert.SerializeObject(user);
                            IDatabase db = redis.GetDatabase();
                            if (await db.SetContainsAsync("AllUsers", serializedUser))
                            {
                                await db.SetRemoveAsync("AllUsers", serializedUser);
                            }

                            await db.SetAddAsync("AllUsers", serializedUser);
                        }
                    });
                }, stateHubChannelId);

                stateHub.Channel("Users").AddReceiver("Remove", (userData) =>
                {
                    return Task.Factory.StartNew(async () =>
                    {
                        RemoteUser user = userData as RemoteUser;
                        if (user != null)
                        {
                            string serializedUser = JsonConvert.SerializeObject(user);
                            IDatabase db = redis.GetDatabase();
                            if (await db.SetContainsAsync("AllUsers", serializedUser))
                            {
                                await db.SetRemoveAsync("AllUsers", serializedUser);
                            }
                        }
                    });
                }, stateHubChannelId);

                stateHub.Channel("Content").AddReceiver("Update", (contentData) =>
                {
                    return Task.Factory.StartNew(async () =>
                    {
                        string content = contentData as string;
                        IDatabase db = redis.GetDatabase();
                        await db.StringSetAsync("Content", content);
                    });
                }, stateHubChannelId);

                Console.ReadKey();
                redis.GetDatabase().KeyDelete("AllUsers");
                redis.GetDatabase().KeyDelete("Content");

                stateHub.Channel("Users").RemoveReceiver("Add", stateHubChannelId);
                stateHub.Channel("Users").RemoveReceiver("Remove", stateHubChannelId);
                stateHub.Channel("Content").RemoveReceiver("Update", stateHubChannelId);
                stateHub.Disconnect();
            }
        }
    }
}
