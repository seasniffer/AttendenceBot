using AttendenceBot.Model;
using Newtonsoft.Json;
using System.IO;

namespace AttendenceBot.Factory
{
    public static class ConfigurationFactory
    {
        private const string ConfigurationFile = "secrets.json";

        public static Configuration Get()
        {
            if (!File.Exists(ConfigurationFile))
            {
                throw new FileNotFoundException($"Could not find the {ConfigurationFile} file");
            }

            var file = File.ReadAllText(ConfigurationFile);
            var userStrings = JsonConvert.DeserializeObject<Configuration>(file);

            return userStrings;
        }
    }
}