using System;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;

namespace DummyBot.Core.SlashCommands
{
    [Group("squad", "Squad commands")]
    public class SquadCommands : InteractionModuleBase<SocketInteractionContext<SocketInteraction>>
    {
        Database.Database db = new Database.Database();
        [SlashCommand("setnotifychannel", "Sets the notification channel to recieve incoming war requests")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetNotifyChannel(SocketTextChannel channel)
        {
            try
            {
                await DeferAsync();
                if (!await db.HasSquadServerAsync(Context.Guild.Id))
                {
                    await FollowupAsync("This server is not registered as a squad server!");
                    return;
                }
                var data = await db.GetSquadServerByIdAsync(Context.Guild.Id);
                data.NotifyChannel = channel.Id;
                await db.UpdateSquadServerAsync(data);
                await FollowupAsync($"Successfully set <#{channel.Id}> as notifications channel");
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

        [SlashCommand("settimezone", "Sets the notification channel to recieve incoming war requests")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetTimeZone([Summary(description: "Default timezone of your squad"), Autocomplete(typeof(AutoCompleteTimezoneHandler))] string timezone)
        {
            try
            {
                await DeferAsync();
                if (!await db.HasSquadServerAsync(Context.Guild.Id))
                {
                    await FollowupAsync("This server is not registered as a squad server!");
                    return;
                }
                var data = await db.GetSquadServerByIdAsync(Context.Guild.Id);
                data.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                await db.UpdateSquadServerAsync(data);
                await FollowupAsync($"Successfully set {timezone} as the squads timezone");
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

        [SlashCommand("setwarstatus", "Sets the notification channel to recieve incoming war requests")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetWarStatus([Choice("Available For War", "available"), Choice("Unavailable for war", "unavailable")] string war)
        {
            try
            {
                await DeferAsync();
                if (!await db.HasSquadServerAsync(Context.Guild.Id))
                {
                    await FollowupAsync("This server is not registered as a squad server!");
                    return;
                }
                var data = await db.GetSquadServerByIdAsync(Context.Guild.Id);
                data.WarStatus = war == "available"? true: false;
                await db.UpdateSquadServerAsync(data);
                await FollowupAsync($"Successfully updated warstatus to {war}");
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

        [SlashCommand("declarewar", "Sets the notification channel to recieve incoming war requests")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DeclareWar([Summary(description: "Squad tag")] string squadtag, [Summary(name:"DateTime",description:"Bases time region off of your squad's default timezone. Format: dd/mm/yyyy hh:mm AM/PM")] string time, string serverregion)
        {
            try
            {
                await DeferAsync();
                if (!await db.HasSquadServerAsync(Context.Guild.Id))
                {
                    await FollowupAsync("This server is not registered as a squad server!");
                    return;
                }
                if(!await db.HasSquadServerByTagAsync(squadtag))
                {
                    await FollowupAsync("Either this squad does not exist or has not been verified as a squad server");
                    return;
                }
                try
                {
                    DateTime ParsedTime = DateTime.Parse(time);
                    var data = await db.GetSquadServerByTagAsync(squadtag);
                    var data2 = await db.GetSquadServerByIdAsync(Context.Guild.Id);
                    var Time = TimeZoneInfo.ConvertTimeToUtc(ParsedTime, data2.TimeZone);
                    try
                    {
                        var channel = Context.Client.GetGuild(data.ID).GetTextChannel(data.NotifyChannel);
                        var embed = new EmbedBuilder()
                            .WithTitle($"New war request from squad: {data2.Tag}")
                            .AddField("Time", TimeZoneInfo.ConvertTimeFromUtc(Time, data.TimeZone))
                            .AddField("Server Region", serverregion)
                            .WithColor(Color.Red);
                        var builder = new ComponentBuilder()
                            .WithButton("Accept", $"AcceptWar:{data2.ID},{Time.ToString().Replace(" ","_")},{serverregion}", ButtonStyle.Success)
                            .WithButton("Deny", $"DenyWar:{data2.ID}");
                        await channel.SendMessageAsync(embed: embed.Build(), components: builder.Build());
                        await FollowupAsync("Successfully sent request");
                    }
                    catch
                    {
                        await FollowupAsync("I could not send the request to the squad. Please notify the squad that their notify channel is either not setup properly or that I do not have permissions to notify");
                        return;
                    }
                }
                catch
                {
                    await FollowupAsync("Not a valid time format");
                    return;
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
    }

    public class SquadInteractions : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
    {
        Database.Database db = new Database.Database();
        [ComponentInteraction("AcceptWar:*,*,*")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AcceptSquadWar(string guildID, string Time, string ServerRegion)
        {
            try
            {
                await DeferAsync();
                var rGuild = Context.Client.GetGuild((ulong)Convert.ToInt64(guildID));
                var rGuildData = await db.GetSquadServerByIdAsync((ulong)Convert.ToInt64(guildID));
                var sGuildData = await db.GetSquadServerByIdAsync(Context.Guild.Id);
                var channel = Context.Client.GetGuild(rGuildData.ID).GetTextChannel(rGuildData.NotifyChannel);
                DateTime ParsedTime = DateTime.Parse(Time.Replace("_"," "));
                var embed = new EmbedBuilder()
                    .WithTitle($"Squad war from **{sGuildData.Tag}** has been accepted!")
                    .AddField("Time", TimeZoneInfo.ConvertTimeFromUtc(ParsedTime, rGuildData.TimeZone))
                    .AddField("Server Region", ServerRegion)
                    .WithColor(Color.Green);
                var embed2 = EmbedBuilderExtensions.ToEmbedBuilder(Context.Interaction.Message.Embeds.First())
                    .WithTitle($"Accepted squad war: {rGuildData.Tag}")
                    .WithColor(Color.Green);
                await channel.SendMessageAsync(embed: embed.Build());
                await FollowupAsync("Accepted Squad War", ephemeral: true);
                await ModifyOriginalResponseAsync(x => { x.Embed = embed2.Build(); x.Components = new ComponentBuilder().Build(); });
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

        [ComponentInteraction("DenyWar:*")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DenySquadWar(string guildID)
        {
            try
            {
                await DeferAsync();
                var rGuild = Context.Client.GetGuild((ulong)Convert.ToInt64(guildID));
                var rGuildData = await db.GetSquadServerByIdAsync((ulong)Convert.ToInt64(guildID));
                var sGuildData = await db.GetSquadServerByIdAsync(Context.Guild.Id);
                var channel = Context.Client.GetGuild(rGuildData.ID).GetTextChannel(rGuildData.NotifyChannel);
                var embed = new EmbedBuilder()
                    .WithTitle($"Squad war request from **{sGuildData.Tag}** has been denied!")
                    .WithColor(Color.Red);
                var embed2 = EmbedBuilderExtensions.ToEmbedBuilder(Context.Interaction.Message.Embeds.First())
                    .WithTitle($"Denied squad war: {rGuildData.Tag}")
                    .WithColor(Color.Red);
                await channel.SendMessageAsync(embed: embed.Build());
                await FollowupAsync("Denied Squad War", ephemeral: true);
                await ModifyOriginalResponseAsync(x => { x.Embed = embed2.Build(); x.Components = new ComponentBuilder().Build(); });
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
