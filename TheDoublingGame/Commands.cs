using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

// ReSharper disable UnusedMember.Global

namespace TheDoublingGame
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Commands : BaseCommandModule
    {
        [Command("setup"), Description("Use this command to set up the bot on your server, can only be executed once per server."), RequireUserPermissions(Permissions.ManageChannels)]
        public async Task Setup(CommandContext ctx)
        {
            var data = DataManager.GetData(ctx.Guild);
            if (data.IsSetup)
            {
                await ctx.RespondAsync(
                    "Oops, the bot is already set up! Please use the appropriate command to modify its settings.");
                return;
            }
            
            var interactivity = ctx.Client.GetInteractivity();

            var setupMessage = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Title = "The doubling game setup",
                Description =
                    "Would you like to setup the game in this channel. " +
                    "If not react with :x: or ignore this message, and run the command in the desired channel.",
                Color = DiscordColor.Blurple
            });

            await setupMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            await setupMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));

            var result = await interactivity.WaitForReactionAsync(setupMessage, ctx.Member);

            if (result.TimedOut)
            {
                await setupMessage.ModifyAsync(new DiscordEmbedBuilder
                {
                    Title = "The doubling game setup",
                    Description = "Timed out!",
                    Color = DiscordColor.Grayple
                }
                    .Build());
                return;
            }

            switch (result.Result.Emoji.GetDiscordName())
            {
                case ":white_check_mark:":
                    await setupMessage.DeleteReactionAsync(result.Result.Emoji, ctx.User);
                    await setupMessage.ModifyAsync(new DiscordEmbedBuilder
                        {
                            Title = "The doubling game setup",
                            Description = "Very nice! " +
                                          "Would you like to make a message with all the rules and pin it automatically ?",
                            Color = DiscordColor.Blurple
                        }
                        .Build());
                    break;
                
                case ":x:":
                    await setupMessage.ModifyAsync(new DiscordEmbedBuilder
                        {
                            Title = "The doubling game setup",
                            Description = "Setup aborted!",
                            Color = DiscordColor.Grayple
                        }
                        .Build());
                    return;
                
                default:
                    await setupMessage.ModifyAsync(new DiscordEmbedBuilder
                        {
                            Title = "The doubling game setup",
                            Description = "Invalid reaction! Please re-run the command.",
                            Color = DiscordColor.Grayple
                        }
                        .Build());
                    return;
            }
            
            result = await interactivity.WaitForReactionAsync(setupMessage, ctx.Member);

            if (result.TimedOut)
            {
                await setupMessage.ModifyAsync(new DiscordEmbedBuilder
                {
                    Title = "The doubling game setup",
                    Description = "Timed out!",
                    Color = DiscordColor.Grayple
                }
                    .Build());
                return;
            }

            switch (result.Result.Emoji.GetDiscordName())
            {
                case ":white_check_mark:":
                    await setupMessage.DeleteReactionAsync(result.Result.Emoji, ctx.User);
                    await setupMessage.ModifyAsync(new DiscordEmbedBuilder
                        {
                            Title = "The doubling game setup",
                            Description = "OKay! ",
                            Color = DiscordColor.Blurple
                        }
                        .Build());
                    var rules = await ctx.RespondAsync(new DiscordEmbedBuilder
                    {
                        Title = "The Doubling game rules",
                        Description = "Below you can find the rules to correctly play the doubling game.",
                        Color = DiscordColor.Blurple
                    }
                        .AddField("1 - You must answer with the next power of two.", "You must answer with last number multiplied by 2. For example, if the last number is 8, answer with 16.")
                        .AddField("2 - Do you answer twice in a row.", "if you answer twice in a row you will break the current streak and you will restart from 1 again.")
                        .AddField("3 - Wait for the green checkmark.", "The green checkmark has two purposes, one being to inform you that your answer is correct, the second being to notify you that you answer has been registered. If you answer without seeing the green checkmark on the last number, you'll probably break the chain."));
                    await rules.PinAsync();
                    break;
                
                case ":x:":
                    await setupMessage.ModifyAsync(new DiscordEmbedBuilder
                        {
                            Title = "The doubling game setup",
                            Description = "Alright",
                            Color = DiscordColor.Grayple
                        }
                        .Build());
                    break;
                
                default:
                    await setupMessage.ModifyAsync(new DiscordEmbedBuilder
                        {
                            Title = "The doubling game setup",
                            Description = "Invalid reaction! Please re-run the command.",
                            Color = DiscordColor.Grayple
                        }
                        .Build());
                    return;
            }
            
            await setupMessage.ModifyAsync(new DiscordEmbedBuilder
                {
                    Title = "The doubling game setup",
                    Description = "The setup is done, enjoy playing the doubling game! To talk without triggering the game put `!!` before you message.",
                    Color = DiscordColor.Blurple
                }
                .Build());
            await setupMessage.DeleteAllReactionsAsync();

            data.IsSetup = true;
            data.CountingChannelId = ctx.Channel.Id;
            data.LastNumber = 2;
            data.GameEnabled = true;
            DataManager.SaveData(data);

            await ctx.RespondAsync("**2**");
        }

        [Command("gameon"), Description("Enable the game."), RequireUserPermissions(Permissions.ManageChannels)]
        public async Task GameOn(CommandContext ctx)
        {
            var data = DataManager.GetData(ctx.Guild);
            data.GameEnabled = true;
            DataManager.SaveData(data);

            await ctx.RespondAsync(":white_check_mark: Game has been turned on!");
            await ctx.RespondAsync($"Last number is **{data.LastNumber}**");
        }
        
        [Command("gameoff"), Description("Disable the game."), RequireUserPermissions(Permissions.ManageChannels)]
        public async Task GameOff(CommandContext ctx)
        {
            var data = DataManager.GetData(ctx.Guild);
            data.GameEnabled = false;
            DataManager.SaveData(data);

            await ctx.RespondAsync(":x: Game has been turned off");
        }
        
        [Command("highscore"), Description("Shows the current highscore.")]
        public async Task HighScore(CommandContext ctx)
        {
            var data = DataManager.GetData(ctx.Guild);

            await ctx.RespondAsync($":trophy: The highest score is **{data.HighScore}**.");
        }

        [Command("changechannel"), Description("Allows you to change the channel the bot is working in."), RequireUserPermissions(Permissions.ManageChannels)]
        public async Task ChangeChannel(CommandContext ctx)
        {
            var data = DataManager.GetData(ctx.Guild);
            if (!data.IsSetup)
            {
                await ctx.RespondAsync("The bot has not been set up, please run `tdg!setup` first.");
                return;
            }
            data.IsSetup = false;
            DataManager.SaveData(data);

            await ctx.CommandsNext.RegisteredCommands["setup"].ExecuteAsync(ctx);
        }
        
        [Command("lastnumber"), Description("In case you lost the count, run this command to show the last number.")]
        public async Task LastNumber(CommandContext ctx)
        {
            var data = DataManager.GetData(ctx.Guild);

            await ctx.RespondAsync($":bulb: The last number is **{data.LastNumber}**.");
        }
    }
}