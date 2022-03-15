using Discord.Interactions;
using Discord.WebSocket;
using Discord;

namespace DummyBot.Core.Interactions
{
    public class StatInteractions : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
    {
        private Utils utils = new Utils();
        private Database.Database db = new Database.Database();

        [ComponentInteraction("PlayerSearch:*")]
        public async Task SearchPlayer(string playerID)
        {
            try
            {
                await DeferAsync();
                await Context.Interaction.Message.ModifyAsync(x => { x.Embed = new EmbedBuilder().WithTitle("Loading Data...").Build(); x.Components = new ComponentBuilder().Build(); });
                var embed = utils.GetEmbedWBStats(playerID);
                var builder = new ComponentBuilder()
                    .WithButton("View Full Stats", style: ButtonStyle.Link, url: $"https://stats.warbrokers.io/players/i/{playerID}");
                await Context.Interaction.Message.ModifyAsync(x => { x.Embed = embed.Build(); x.Components = builder.Build(); });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [ComponentInteraction("SyncStats:*")]
        public async Task SyncStats(string userID)
        {
            try
            {
                await DeferAsync();
                if (Context.User.Id.ToString() != userID)
                {
                    await FollowupAsync($"This is not your stats page!!", ephemeral: true);
                    return;
                }
                var cooldown = utils.Cooldown(Context.User, "Sync", 60 * 60 * 6);
                if (!cooldown.CooledDown)
                {
                    await FollowupAsync($"You are on cooldown for this! Try again in {cooldown.Seconds} seconds", ephemeral: true);
                    return;
                }
                await Context.Interaction.Message.ModifyAsync(x => { x.Embed = new EmbedBuilder().WithTitle("Syncing Stats").Build(); x.Components = null; });
                var userData = await db.GetUserByIdAsync(Context.User.Id);
                var data = utils.WBStatsData(userData.WBID, Context.User.Id);
                if(userData.IsSteam)
                {
                    data.IsSteam = true;
                }
                await db.UpdateUserAsync(data);
                var dataEmbed = await utils.GetSavedEmbedWBStats(Context.User.Id);
                var builder = new ComponentBuilder()
                    .WithButton("View Full Stats", style: ButtonStyle.Link, url: $"https://stats.warbrokers.io/players/i/{userData.WBID}");
                await Context.Interaction.Message.ModifyAsync(x => { x.Embed = dataEmbed.Build(); x.Components = builder.Build(); });
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
