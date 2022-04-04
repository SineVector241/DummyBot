using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace DummyBot.Core.Interactions
{
    public class ApplicationInteractions : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
    {
        Database.Database db = new Database.Database();

        [ComponentInteraction("AcceptSteamApp:*")]
        public async Task AcceptSteamApplication(string userId)
        {
            try
            {
                await DeferAsync();
                var user = await Context.Client.GetUserAsync((ulong)Convert.ToInt64(userId));
                var wbStats = await db.GetUserByIdAsync(user.Id);
                wbStats.IsSteam = true;
                await db.UpdateUserAsync(wbStats);
                try
                {
                    await user.SendMessageAsync("Congrats! Your steam application has been accepted! You can now view your full stats");
                }
                catch
                { }
                await FollowupAsync("Accepted User", ephemeral: true);
                await DeleteOriginalResponseAsync();
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

        [ComponentInteraction("DenySteamApp:*")]
        public async Task DenySteamApplication(string userId)
        {
            try
            {
                await DeferAsync();
                var user = await Context.Client.GetUserAsync((ulong)Convert.ToInt64(userId));
                try
                {
                    await user.SendMessageAsync("Your steam application has been denied. This could be due that you have not verified that you have steam on the official WarBrokers discord. Please retry again in 24 hours");
                }
                catch
                {}
                await FollowupAsync("Successfully denied application", ephemeral: true);
                await DeleteOriginalResponseAsync();
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

        [ComponentInteraction("AcceptSquadApp:*,*,*")]
        public async Task AcceptSquadApplication(string guildID, string timezone, string userID)
        {
            try
            {
                await DeferAsync();
                var user = await Context.Client.GetUserAsync((ulong)Convert.ToInt64(userID));
                await db.CreateSquadServerAsync(new Database.SquadServer { ID = (ulong)Convert.ToInt64(guildID), NotifyChannel = 0, TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone.Replace("_", " ")), WarStatus = true, Tag = Context.Interaction.Message.Embeds.First().Fields.First().Value});
                try
                {
                    await user.SendMessageAsync("Congrats! Your squad application has been accepted! You now have access to squad features!");
                }
                catch
                { }
                await FollowupAsync("Accepted Squad", ephemeral: true);
                await DeleteOriginalResponseAsync();
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

        [ComponentInteraction("DenySquadApp:*")]
        public async Task DenySquadApplication(string userID)
        {
            try
            {
                await DeferAsync();
                var user = await Context.Client.GetUserAsync((ulong)Convert.ToInt64(userID));
                try
                {
                    await user.SendMessageAsync("Your squad application has been denied. Please retry again in 1 hour");
                }
                catch
                { }
                await FollowupAsync("Successfully denied application", ephemeral: true);
                await DeleteOriginalResponseAsync();
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
