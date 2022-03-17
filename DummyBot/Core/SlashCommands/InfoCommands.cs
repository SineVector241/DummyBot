using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace DummyBot.Core.SlashCommands
{
    [Group("info", "Information commands")]
    public class InfoCommands : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
    {
        private Utils utils = new Utils();

        [SlashCommand("squads", "Displays all currently existing squads in War Brokers")]
        [RequireContext(ContextType.Guild)]
        public async Task Squads()
        {
            try
            {
                await DeferAsync();
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(utils.GetRequest($"https://stats.warbrokers.io"));
                var squads = document.DocumentNode.SelectNodes("//a[@class='squadLink']");
                string list = "";
                foreach (var squadname in squads)
                {
                    list += $"\n{squadname.InnerText}";
                }
                var embed = new EmbedBuilder()
                    .WithTitle("Available Squads")
                    .WithColor(Color.Green)
                    .WithDescription(list);
                await FollowupAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                var embed = new EmbedBuilder()
                    .WithTitle("An error has occured")
                    .WithDescription(ex.Message)
                    .WithColor(Color.DarkRed);
                await FollowupAsync(embed: embed.Build());
            }
        }

        [SlashCommand("ping", "Latency of the bot")]
        [RequireContext(ContextType.Guild)]
        public async Task Ping()
        {
            try
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Title = "Pong!",
                    Description = $"Bots ping is: {Context.Client.Latency} ms",
                    Color = Context.Client.Latency <= 100 ? Color.Green : Color.Red,
                    Footer = new EmbedFooterBuilder().WithText(Context.User.Username).WithIconUrl(Context.User.GetAvatarUrl())
                };

                await RespondAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                var embed = new EmbedBuilder()
                    .WithTitle("An error has occured")
                    .WithDescription($"Error Message: {ex.Message}")
                    .WithColor(Color.DarkRed);
                await RespondAsync(embed: embed.Build());
            }
        }

        [SlashCommand("cooldowns", "Check your cooldowns")]
        public async Task Cooldowns()
        {
            try
            {
                await DeferAsync();
                int c1 = utils.CheckCooldown(Context.User, "SearchPlayer").Seconds;
                int c2 = utils.CheckCooldown(Context.User, "Sync").Seconds;
                var embed = new EmbedBuilder()
                    .WithTitle($"{Context.User.Username}'s cooldowns")
                    .AddField("Search Player", $"{ (c1 <= 0 ? "Ready" : c1 + " Seconds")}")
                    .AddField("Sync Stats", $"{(c2 <= 0 ? "Ready" : c2 + " Seconds")}")
                    .WithColor(Color.Orange);
                await FollowupAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                var embed = new EmbedBuilder()
                    .WithTitle("An error has occured")
                    .WithDescription($"Error Message: {ex.Message}")
                    .WithColor(Color.DarkRed);
                await FollowupAsync(embed: embed.Build());
            }
        }
    }
}
