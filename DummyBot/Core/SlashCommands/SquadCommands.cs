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
                            .WithButton("Accept", $"AcceptWar:{data2.ID},{data.Tag},{Time.ToString().Replace(" ","_")}", ButtonStyle.Success)
                            .WithButton("Deny", $"DenyWar:{data2.ID}");
                        await FollowupAsync(embed: embed.Build(), components: builder.Build());
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
}
