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
                var cooldown = utils.Cooldown(Context.User, "Sync", 60 * 60 * 1);
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

        [ComponentInteraction("Members1:*:*")]
        public async Task ViewMembers1(string userID, string SquadName)
        {
            try
            {
                await DeferAsync();
                if(userID != Context.User.Id.ToString())
                {
                    await FollowupAsync("This is not your page to interact with!", ephemeral: true);
                    return;
                }
                var embed = new EmbedBuilder()
                    .WithTitle($"Viewing Squad Stats: {SquadName}")
                    .WithColor(Color.Green);
                var SquadData = await db.GetSquadByNameAsync(SquadName);
                int counter = 0;
                foreach(var member in SquadData.Members)
                {
                    counter++;
                    string level = member.Value["Level"].ToString();
                    string XP = member.Value["XP"].ToString();
                    string ClassicWins = member.Value["ClassicWins"].ToString();
                    embed.AddField(member.Key, $"```css\nLevel: {level}\nExperience: {XP}\nClassic Wins: {ClassicWins}```");
                    if (counter >= 8)
                        break;
                }
                await ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
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

        [ComponentInteraction("Members2:*:*")]
        public async Task ViewMembers2(string userID, string SquadName)
        {
            try
            {
                await DeferAsync();
                if (userID != Context.User.Id.ToString())
                {
                    await FollowupAsync("This is not your page to interact with!", ephemeral: true);
                    return;
                }
                var embed = new EmbedBuilder()
                    .WithTitle($"Viewing Squad Stats: {SquadName}")
                    .WithColor(Color.Green);
                var SquadData = await db.GetSquadByNameAsync(SquadName);
                int counter = 0;
                foreach (var member in SquadData.Members)
                {
                    counter++;
                    if (counter >= 9)
                    {
                        string level = member.Value["Level"].ToString();
                        string XP = member.Value["XP"].ToString();
                        string ClassicWins = member.Value["ClassicWins"].ToString();
                        embed.AddField(member.Key, $"```css\nLevel: {level}\nExperience: {XP}\nClassic Wins: {ClassicWins}```");
                    }
                    if (counter >= 16)
                        break;
                }
                await ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
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

        [ComponentInteraction("Members3:*:*")]
        public async Task ViewMembers3(string userID, string SquadName)
        {
            try
            {
                await DeferAsync();
                if (userID != Context.User.Id.ToString())
                {
                    await FollowupAsync("This is not your page to interact with!", ephemeral: true);
                    return;
                }
                var embed = new EmbedBuilder()
                    .WithTitle($"Viewing Squad Stats: {SquadName}")
                    .WithColor(Color.Green);
                var SquadData = await db.GetSquadByNameAsync(SquadName);
                int counter = 0;
                foreach (var member in SquadData.Members)
                {
                    counter++;
                    if (counter >= 17)
                    {
                        string level = member.Value["Level"].ToString();
                        string XP = member.Value["XP"].ToString();
                        string ClassicWins = member.Value["ClassicWins"].ToString();
                        embed.AddField(member.Key, $"```css\nLevel: {level}\nExperience: {XP}\nClassic Wins: {ClassicWins}```");
                    }
                    if (counter >= 24)
                        break;
                }
                await ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
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

        [ComponentInteraction("Members4:*:*")]
        public async Task ViewMembers4(string userID, string SquadName)
        {
            try
            {
                await DeferAsync();
                if (userID != Context.User.Id.ToString())
                {
                    await FollowupAsync("This is not your page to interact with!", ephemeral: true);
                    return;
                }
                var embed = new EmbedBuilder()
                    .WithTitle($"Viewing Squad Stats: {SquadName}")
                    .WithColor(Color.Green);
                var SquadData = await db.GetSquadByNameAsync(SquadName);
                int counter = 0;
                foreach (var member in SquadData.Members)
                {
                    counter++;
                    if (counter >= 25)
                    {
                        string level = member.Value["Level"].ToString();
                        string XP = member.Value["XP"].ToString();
                        string ClassicWins = member.Value["ClassicWins"].ToString();
                        embed.AddField(member.Key, $"```css\nLevel: {level}\nExperience: {XP}\nClassic Wins: {ClassicWins}```");
                    }
                    if (counter >= 32)
                        break;
                }
                await ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
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

        [ComponentInteraction("SquadHome:*:*")]
        public async Task SquadHomePage(string userID, string SquadName)
        {
            try
            {
                await DeferAsync();
                if (userID != Context.User.Id.ToString())
                {
                    await FollowupAsync("This is not your page to interact with!", ephemeral: true);
                    return;
                }
                var squadData = await db.GetSquadByNameAsync(SquadName);
                var embed = new EmbedBuilder()
                    .WithTitle($"Viewing Squad Stats: {SquadName}")
                    .AddField("Overall Squad Wins", $"```css\nDeath Match: {squadData.DeathMatch}\nBattle Royale: {squadData.BattleRoyale}\nMissile Launch: {squadData.MissileLaunch}\nPackage Drop: {squadData.PackageDrop}\nVehicle Escort: {squadData.VehicleEscort}\nZombie Battle Royale: {squadData.ZombieBR}\nCapture Point: {squadData.CapturePoint}```")
                    .WithColor(Color.Green);
                await ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
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
