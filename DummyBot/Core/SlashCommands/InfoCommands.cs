using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace DummyBot.Core.SlashCommands
{
    [Group("info", "Information commands")]
    public class InfoCommands : InteractionModuleBase<SocketInteractionContext<SocketInteraction>>
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

        [SlashCommand("map", "display map information")]
        public async Task Map([Summary(description: "Name of the map"), Autocomplete(typeof(MapAutoCompleteHandler))] string MapName)
        {
            try
            {
                await DeferAsync();
                if (File.Exists($"{Config._MapPicturesFolder}/{MapName.Replace(" ", "_")}.jpg"))
                {
                    var embed = new EmbedBuilder()
                        .WithTitle($"Map: {MapName}")
                        .WithImageUrl($"attachment://{MapName.Replace(" ", "_")}.jpg")
                        .WithColor(Color.Orange);
                    await FollowupWithFileAsync($"{Config._MapPicturesFolder}/{MapName.Replace(" ", "_")}.jpg", embed: embed.Build());
                }
                else
                {
                    await FollowupAsync("That map does not exist! Make sure you have typed the map name correctly. Check **/info maps** for a full list of the available maps.\nThe map name searcher is CASE SENSITIVE");
                }
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

        [SlashCommand("servers","Shows all servers the bot is in")]
        [RequireOwner]
        public async Task Servers()
        {
            try
            {
                await DeferAsync();
                var servers = Context.Client.Guilds;
                var embed = new EmbedBuilder()
                    .WithTitle("Servers:")
                    .WithColor(Color.LightOrange);
                foreach(var server in servers)
                {
                    var user = await server.GetUsersAsync().FirstAsync();
                    var usernames = "";
                    int counter = 0;
                    foreach(string username in user.Select(x => x.Username))
                    {
                        usernames += username + "\n";
                        if(counter++ == 5)
                            break;
                    }
                    embed.AddField(server.Name, usernames);
                }
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

    public class MapAutoCompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var subject = autocompleteInteraction.Data.Current.Value as string; // what the user managed to type into the textbox so far
            var files = Directory.EnumerateFiles(Config._MapPicturesFolder, "*.jpg");
            IEnumerable<string> maps = files;
            List<AutocompleteResult> autocompleteResults = new List<AutocompleteResult>();
            int counter = 0;
            foreach (string map in maps)
            {
                if (Path.GetFileName(map).Replace("_", " ").Replace(".jpg", "").ToLower().Contains(subject.ToLower()))
                {
                    counter++;
                    autocompleteResults.Add(new AutocompleteResult
                    {
                        Name = Path.GetFileName(map).Replace("_", " ").Replace(".jpg", ""), // here's what will appear in the suggestions list
                        Value = Path.GetFileName(map).Replace("_", " ").Replace(".jpg", "") // here's what will actually go into the slashcommand argument on tapping the suggestion
                    });
                    if(counter >= 25)
                    {
                        break;
                    }
                }
            }
            return AutocompletionResult.FromSuccess(autocompleteResults);
        }
    }
}
