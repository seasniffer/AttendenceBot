using AttendenceBot.Factory;
using Discord.WebSocket;
using SimpleInjector;
using System;
using System.Threading.Tasks;

namespace AttendenceBot
{
    public class Program
    {
        private Container container;

        public static void Main(string[] args) => new Program().Run().GetAwaiter().GetResult();

        public async Task Run()
        {
            this.LoadContainerAsync();

            var client = container.GetInstance<DiscordSocketClient>();

            client.UserVoiceStateUpdated += ClientUserVoiceStateUpdated;

            while (true)
            {
                //// TODO: Show list of servers

                //// TODO: Show list of voice channels

                //// TODO: Start logging until user specifies to stop logging
            }
        }

        private Task ClientUserVoiceStateUpdated(SocketUser user, SocketVoiceState pre, SocketVoiceState post)
        {
            throw new NotImplementedException();
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