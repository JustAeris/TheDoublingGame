using System.IO;
using System.Text;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace TheDoublingGame
{
    public static class DataManager
    {
        static DataManager()
        {
            Directory = Configuration.DataDir;

            if (!System.IO.Directory.Exists(Directory))
                System.IO.Directory.CreateDirectory(Directory);
        }
        private static string Directory { get; }

        private static GuildConfiguration GetData(ulong guildId)
        {
            var path = $"{Directory}/{guildId}.json";

            if (!File.Exists(path))
                return null;

            var content = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<GuildConfiguration>(content);
        }

        public static GuildConfiguration GetData(DiscordGuild guild) => GetData(guild.Id);
        
        public static void SaveData(GuildConfiguration guildConfig)
        {
            var userFile = $"{Directory}/{guildConfig.GuildId}.json";

            var json = JsonConvert.SerializeObject(guildConfig, Formatting.Indented);

            using var f =
                new FileStream(userFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)
                    {Position = 0};

            f.SetLength(0);

            f.Write(Encoding.UTF8.GetBytes(json));
        }

        private static void DeleteData(ulong guildId) => File.Delete($"{Directory}/{guildId}.json");
        public static void DeleteData(DiscordGuild guild) => DeleteData(guild.Id);
    }
}