using System;
using System.Collections.Generic;
using System.Text;

namespace AttendenceBot.Model
{
    public class Log
    {
        public string DiscordName { get; set; }

        public ulong DiscordId { get; set; }

        public EventType EventType { get; set; }

        public DateTime Timestamp { get; set; }        
    }
}
