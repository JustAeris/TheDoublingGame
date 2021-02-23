using System;
using System.Numerics;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

namespace TheDoublingGame
{
    internal static class Program
    {
        private static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            var discord = new DiscordClient(new DiscordConfiguration
            {
                Token = Configuration.UseProduction ? Configuration.ProductionToken : Configuration.DevelopmentToken,
                TokenType = TokenType.Bot
            });

            discord.MessageCreated += (sender, e) =>
            {
                _ = Task.Run(async () =>
                {
                    var data = DataManager.GetData(e.Guild);
                    
                    if (!data.IsSetup || 
                        data.CountingChannelId != e.Channel.Id ||
                        e.Message.Author.IsBot ||
                        e.Message.Content.StartsWith("!!") ||
                        e.Message.Content.StartsWith("tdg!") ||
                        !data.GameEnabled) 
                        return;

                    var isANumber = BigInteger.TryParse(e.Message.Content.GetRawNumber(), out var num);

                    if (data.LastPlayerId != 0 && data.LastPlayerId == e.Author.Id)
                    {
                        await e.Message.CreateReactionAsync(DiscordEmoji.FromName(discord, ":x:"));

                        await e.Channel.SendMessageAsync("**2**");

                        data.LastNumber = 2;
                        data.LastPlayerId = 0;
                        DataManager.SaveData(data);
                        return;
                    }
                    if (num == data.LastNumber * 2 && isANumber)
                    {
                        try
                        {
                            await e.Message.CreateReactionAsync(DiscordEmoji.FromName(discord, ":white_check_mark:"));
                            
                            data.LastNumber = num == 0 ? 2 : num;

                            if (data.LastNumber > data.HighScore) data.HighScore = data.LastNumber;

                            data.LastPlayerId = e.Author.Id;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                    else
                    {
                        await e.Message.CreateReactionAsync(DiscordEmoji.FromName(discord, ":x:"));

                        await e.Channel.SendMessageAsync("**2**");

                        data.LastNumber = 2;
                    }
                    DataManager.SaveData(data);
                });
                return Task.CompletedTask;
            };

            discord.GuildCreated += (_, args) =>
            {
                DataManager.SaveData(new GuildConfiguration{ IsSetup = false, GuildId = args.Guild.Id });
                return Task.CompletedTask;
            };

            discord.GuildDeleted += (_, args) =>
            {
                DataManager.DeleteData(args.Guild);
                return Task.CompletedTask;
            };

            discord.ClientErrored += (_, e) =>
            {
                Console.WriteLine(e.Exception.Message);
                return Task.CompletedTask;
            };
            
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] {"tdg!"},
                IgnoreExtraArguments = true
            });
            
            discord.UseInteractivity(new InteractivityConfiguration
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });
            
            commands.RegisterCommands<Commands>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }

    internal static class MyExtensions
    {
        public static string GetRawNumber(this string s)
        {
            return s.Replace(",", "")
                .Replace(".", "")
                .Replace(" ", "")
                .Replace("_", "")
                .Replace("*", "")
                .Replace("_", "")
                .Replace("~", "")
                .Replace("`", "")
                .Replace(">", "")
                .Replace("|", "");
        }
    }
}