using AttendenceBot.Factory;
using AttendenceBot.Model;
using Discord.WebSocket;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AttendenceBot
{
    public class Program
    {
        private Container container;

        public static void Main(string[] args) => new Program().Run().GetAwaiter().GetResult();

        private List<Log> events = new List<Log>();

        private Configuration configuration;

        public bool isLogging;

        public async Task Run()
        {
            this.LoadContainerAsync();           

            configuration = container.GetInstance<Configuration>();

            var client = container.GetInstance<DiscordSocketClient>();

            await client.LoginAsync(Discord.TokenType.Bot, configuration.DiscordToken);
            await client.StartAsync();

            client.UserVoiceStateUpdated += ClientUserVoiceStateUpdatedAsync;
            
            //// TODO: Show list of servers                       
            var channel = client.GetChannel(configuration.ChannelId);                  
            
            if (channel != null)
            {                
                foreach (var user in channel.Users)
                {
                    events.Add(new Log()
                    {
                        DiscordId = user.Id,
                        DiscordName = user.Username,
                        EventType = EventType.Join,
                        Timestamp = DateTime.Now
                    });
                }                

                isLogging = true;
            }
            else
            {
                throw new Exception($"Could not find channel with ID of {configuration.ChannelId}");
            }
            
            Console.WriteLine("Hit enter to exit");
            Console.ReadLine();

            channel = client.GetChannel(configuration.ChannelId);
            foreach (var user in channel.Users)
            {
                events.Add(new Log()
                {
                    DiscordId = user.Id,
                    DiscordName = user.Username,
                    EventType = EventType.Disconnect,
                    Timestamp = DateTime.Now
                });
            }

            await ComputeStatisticsAsync();
        }

        private async Task ClientUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState pre, SocketVoiceState post)
        {
            if (!isLogging)
            {
                return;
            }

            //// Log User Joining
            if (pre.VoiceChannel?.Id != configuration.ChannelId && post.VoiceChannel?.Id == configuration.ChannelId)
            {
                events.Add(new Log()
                {
                    DiscordId = user.Id,
                    DiscordName = user.Username,
                    EventType = EventType.Join,
                    Timestamp = DateTime.Now
                });
                // Log user join event
            }

            //// Log User Leaving
            if (pre.VoiceChannel?.Id == configuration.ChannelId && post.VoiceChannel?.Id != configuration.ChannelId)
            {
                events.Add(new Log()
                {
                    DiscordId = user.Id,
                    DiscordName = user.Username,
                    EventType = EventType.Disconnect,
                    Timestamp = DateTime.Now
                });
            }

            await Task.Delay(1);
        }

        private async Task ComputeStatisticsAsync()
        {
            using (var stream = File.Create($"{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}"))
            using (var streamWriter = new StreamWriter(stream))
            {
                foreach (var user in events.GroupBy(x => x.DiscordId))
                {
                    var events = user.OrderBy(x => x.Timestamp).ToList();

                    if (events.Count % 2 != 0)
                    {
                        Console.WriteLine("wtf");
                    }

                    TimeSpan totalTime = new TimeSpan(0);
                    for (int x = 0; x < events.Count; x = x + 2)
                    {
                        var start = events[x];
                        var end = events[x + 1];

                        Debug.Assert(start.EventType == EventType.Join && end.EventType == EventType.Disconnect);

                        var elapsed = end.Timestamp - start.Timestamp;

                        if (totalTime.Ticks == 0)
                        {
                            totalTime = elapsed;
                        }
                        else
                        {
                            totalTime.Add(elapsed);
                        }
                    }
                    
                    Console.WriteLine($"User: {user.First().DiscordName} - Raid Time: {totalTime}");
                    await streamWriter.WriteLineAsync($"User: {user.First().DiscordName} - Raid Time: {totalTime}");
                }
            }
        }

        private void LoadContainerAsync()
        {
            this.container = new Container();

            //// Configuration services
            container.Register(ConfigurationFactory.Get, Lifestyle.Singleton);

            //// Client object
            container.Register(() => DiscordClientFactory.Get(ConfigurationFactory.Get()).Result, Lifestyle.Singleton);

            container.Verify();
        }
    }
}