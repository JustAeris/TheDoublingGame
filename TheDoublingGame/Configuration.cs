using System.IO;
using Newtonsoft.Json.Linq;

namespace TheDoublingGame
{
    public static class Configuration
    {
        public static string DevelopmentToken { get; }
        public static string ProductionToken { get; }
        public static bool UseProduction { get; }
        public static string DataDir { get; }
        
        static Configuration()
        {
            if (!File.Exists("config.json"))
                return;

            var content = JObject.Parse(File.ReadAllText("config.json"));

            DevelopmentToken = (string) content["Discord"]?["Development"];
            ProductionToken = (string) content["Discord"]?["Production"];
            UseProduction = (bool) content["Discord"]?["UseProduction"];

            DataDir = (string) content[nameof(DataDir)];
        }
    }
}