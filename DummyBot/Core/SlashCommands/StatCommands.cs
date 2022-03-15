using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Fergun.Interactive;

namespace DummyBot.Core.SlashCommands
{
    [Group("stats", "Shows WB stats")]
    public class StatCommands : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
    {
        private readonly HttpClient client = new HttpClient();
        private Utils utils = new Utils();
        private Database.Database db = new Database.Database();
        public InteractiveService Interactive { get; set; }

        [SlashCommand("searchplayer", "Searches for a warbokers player by name")]
        public async Task SearchPlayer([Summary(description:"The player name")] string player)
        {
            try
            {
                await DeferAsync();
                var cooldown = utils.Cooldown(Context.User, "SearchPlayer", 60 * 60 * 1);
                if (!cooldown.CooledDown)
                {
                    await FollowupAsync($"You are on cooldown for this command. Try again in {cooldown.Seconds} seconds");
                    return;
                }
                var embed = new EmbedBuilder()
                    .WithTitle($"Top 5 players matching search: **{player}**")
                    .WithColor(Color.Orange);
                var builder = new ComponentBuilder();
                player = player.ToLower();
                string data = utils.GetRequest($"https://stats.warbrokers.io/players/search?term={player}");
                data = $"{{ \"players\": {data} }}";
                JObject jsonData = JObject.Parse(data);
                dynamic Data = JsonConvert.DeserializeObject(data);
                int counter = 0;

                foreach (JArray value in Data["players"])
                {
                    counter++;
                    if(counter<=5)
                    {
                        embed.AddField($"{counter}: {value[0]}", $"ID: {value[1]}");
                        builder.WithButton(counter.ToString(),$"PlayerSearch:{value[1]}");
                    }
                    else
                    {
                        break;
                    }
                }
                await FollowupAsync(embed: embed.Build(), components: builder.Build());
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

        [SlashCommand("me", "Shows your stats")]
        public async Task MyStats()
        {
            try
            {
                await DeferAsync();
                var hasStats = await db.HasProfileAsync(Context.User.Id);
                if(!hasStats)
                {
                    await FollowupAsync("You do not have your stats account linked. Please link an account using your WarBrokers ID by doing the following command\n**/stats link <WB ID>** Without the <>");
                    return;
                }
                var userData = await db.GetUserByIdAsync(Context.User.Id);
                var data = await utils.GetSavedEmbedWBStats(Context.User.Id);
                var builder = new ComponentBuilder()
                    .WithButton("View Full Stats", style: ButtonStyle.Link, url: $"https://stats.warbrokers.io/players/i/{userData.WBID}");
                if (utils.CheckCooldown(Context.User, "Sync").CooledDown)
                {
                    builder.WithButton("Sync Stats", $"SyncStats:{Context.User.Id}", style: ButtonStyle.Success);
                }
                await FollowupAsync(embed: data.Build(), components: builder.Build());
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                var embed = new EmbedBuilder()
                    .WithTitle("An error has occured")
                    .WithDescription(ex.Message)
                    .WithColor(Color.DarkRed);
                await FollowupAsync(embed: embed.Build());
            }
        }

        [SlashCommand("link", "Links your stat account")]
        public async Task LinkStats(string WarBrokersID)
        {
            try
            {
                await DeferAsync();
                var cooldown = utils.CheckCooldown(Context.User, "Sync");
                if (!cooldown.CooledDown)
                {
                    await FollowupAsync($"You are on cooldown for this command. Try again in {cooldown.Seconds} seconds");
                    return;
                }
                var data = utils.WBStatsData(WarBrokersID, Context.User.Id);
                if (string.IsNullOrWhiteSpace(data.WBName))
                {
                    await FollowupAsync("This is not a valid WB stats ID");
                    return;
                }
                var builder = new ComponentBuilder()
                    .WithButton("Confirm", "ConfirmWBLink", ButtonStyle.Success, new Emoji("✅"))
                    .WithButton("Deny", "DenyWBLink", ButtonStyle.Danger, new Emoji("❎"));
                var embed = new EmbedBuilder()
                    .WithTitle($"PlayerName: { data.WBName }")
                    .WithDescription("Is this your WarBrokers Ingame Name?")
                    .WithColor(Color.LightOrange);
                var msg = await FollowupAsync(embed: embed.Build(), components: builder.Build());
                var choice = await Interactive.NextMessageComponentAsync(x => x.Message.Id == msg.Id && x.User.Id == Context.User.Id, timeout: TimeSpan.FromSeconds(10));
                if(choice.IsTimeout)
                {
                    await msg.ModifyAsync(x => { x.Content = "Timed out. Canceled linking..."; x.Components = new ComponentBuilder().Build(); });
                    return;
                }
                bool hasProfile = await db.HasProfileAsync(Context.User.Id);
                if(hasProfile)
                {
                    await db.UpdateUserAsync(data);
                }
                else
                {
                    await db.CreateUserAsync(data);
                }
                utils.Cooldown(Context.User, "Sync", 60 * 60 * 6);
                await msg.ModifyAsync(x => { x.Content = "You have linked your account. You can now use **/stats me**"; x.Components = new ComponentBuilder().Build(); });
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

        [SlashCommand("user", "Displays another discord users WB stats if linked")]
        public async Task UserStats(SocketUser user)
        {
            try
            {
                await DeferAsync();
                var hasStats = await db.HasProfileAsync(user.Id);
                if (!hasStats)
                {
                    await FollowupAsync("This user does not have their stats account linked.");
                    return;
                }
                var userData = await db.GetUserByIdAsync(user.Id);
                var data = await utils.GetSavedEmbedWBStats(user.Id);
                var builder = new ComponentBuilder()
                    .WithButton("View Full Stats", style: ButtonStyle.Link, url: $"https://stats.warbrokers.io/players/i/{userData.WBID}");
                await FollowupAsync(embed: data.Build(), components: builder.Build());
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

        [SlashCommand("squad", "Displays squad stats")]
        public async Task SquadStats(string SquadName)
        {
            try
            {

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
    }
}
