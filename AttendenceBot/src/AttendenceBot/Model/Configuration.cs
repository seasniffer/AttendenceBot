using System;
using System.Collections.Generic;
using System.Text;

namespace AttendenceBot.Model
{
    public class Configuration
    {
        public ulong ServerId { get; set; }

        public string DiscordToken { get; set; }

        public ulong ChannelId { get; set; }
    }
}
