using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace DummyBot.Core.SlashCommands
{
    [Group("apply", "Application Commands")]
    public class Applications : InteractionModuleBase<SocketInteractionContext<SocketInteraction>>
    {
        Database.Database db = new Database.Database();
        Utils utils = new Utils();
        [SlashCommand("steam", "Apply for steam privelages")]
        public async Task SteamApply()
        {
            try
            {
                await DeferAsync();
                if(!utils.CheckCooldown(Context.User, "ApplySteam").CooledDown)
                {
                    await FollowupAsync("You are on cooldown for this command!");
                    return;
                }
                if(!await db.HasProfileAsync(Context.User.Id))
                {
                    await FollowupAsync("You have not linked your warbrokers stats to your discord account! Please link it by running */stats link* command. Be sure to also be in the official WarBrokers discord and verify that you have steam there so I can verify that you do have steam version of WB");
                    return;
                }
                var wbStats = await db.GetUserByIdAsync(Context.User.Id);
                if(wbStats.IsSteam)
                {
                    await FollowupAsync("You already have steam privelages enabled!");
                    return;
                }
                var AppChannel = Context.Client.GetGuild(Config.BotConfiguration.Server).GetTextChannel(Config.BotConfiguration.ApplicationChannel);
                var embed = new EmbedBuilder()
                    .WithTitle($"New steam application from: {Context.User.Username}")
                    .WithDescription($"Discord ID: {Context.User.Id}\nAccount Creation Date: {Context.User.CreatedAt}\nWarBrokers ID: {wbStats.ID}\nWarBrokers IGN: {wbStats.WBName}")
                    .WithColor(Color.LightOrange);
                var builder = new ComponentBuilder()
                    .WithButton("Accept", $"AcceptSteamApp:{Context.User.Id}", ButtonStyle.Success)
                    .WithButton("Deny", $"DenySteamApp:{Context.User.Id}", ButtonStyle.Danger);
                await AppChannel.SendMessageAsync(embed: embed.Build(), components: builder.Build());
                utils.Cooldown(Context.User, "ApplySteam", 60 * 60 * 24);
                await FollowupAsync("Successfully Sent Application", ephemeral: true);
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

        [SlashCommand("squad", "Apply for your server to be verified as a squad server")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SquadApply([Summary(description:"Default timezone of your squad"),Autocomplete(typeof(AutoCompleteTimezoneHandler))] string timezone)
        {
            try
            {
                if (await db.HasSquadServerAsync(Context.Guild.Id))
                {
                    await RespondAsync("This server is already verified as a squad server!");
                    return;
                }
                if (!utils.CheckCooldown(Context.User,"ApplySquad").CooledDown)
                {
                    await RespondAsync("You are on cooldown for this command!");
                    return;
                }
                try
                {
                    var ptimezone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                }
                catch
                {
                    var embed = new EmbedBuilder()
                        .WithTitle($"{timezone} is not a valid timezone")
                        .WithColor(Color.Red);
                    await RespondAsync(embed: embed.Build());
                }
                await RespondWithModalAsync<SquadApplicationModal>($"SquadServerApplication:{Context.Guild.Id},{timezone.Replace(" ","_")}");
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
    }

    public class ModalInteractions : InteractionModuleBase<SocketInteractionContext<SocketInteraction>>
    {
        Utils utils = new Utils();
        [ModalInteraction("SquadServerApplication:*,*")]
        public async Task SquadServerApplicationSubmit(string guildID, string timezone, SquadApplicationModal modal)
        {
            try
            {
                var AppChannel = Context.Client.GetGuild(Config.BotConfiguration.Server).GetTextChannel(Config.BotConfiguration.ApplicationChannel);
                var embed = new EmbedBuilder()
                    .WithTitle("New Squad Application")
                    .AddField("Squad Name", modal.Name)
                    .AddField("TimeZone", timezone.Replace("_"," "))
                    .AddField("Invite Link", modal.InviteLink)
                    .AddField("Other Info", modal.InfoOptional)
                    .AddField("Submitted By", Context.User.Username)
                    .WithColor(Color.LightOrange);
                var builder = new ComponentBuilder()
                    .WithButton("Accept", $"AcceptSquadApp:{guildID},{timezone},{Context.User.Id}", ButtonStyle.Success)
                    .WithButton("Deny", $"DenySquadApp:{Context.User.Id}", ButtonStyle.Danger);
                await AppChannel.SendMessageAsync(embed: embed.Build(), components: builder.Build());
                utils.Cooldown(Context.User, "ApplySquad", 60 * 60);
                await RespondAsync("Successfully sent application.");
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
    }

    public class SquadApplicationModal : IModal
    {
        public string Title => "Squad Server Application";

        [InputLabel("What is your squad tag?")]
        [ModalTextInput("SquadName", placeholder: "Squad Name")]
        public string Name { get; set; }

        [InputLabel("Input an invite link here to the squad server")]
        [ModalTextInput("InviteLink", placeholder: "Invite Link")]
        public string InviteLink { get; set; }

        [InputLabel("Any other information? (Optional)")]
        [ModalTextInput("InfoOptional", TextInputStyle.Paragraph, initValue: "No Other Information")]
        public string InfoOptional { get; set; }
    }

    public class AutoCompleteTimezoneHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var subject = autocompleteInteraction.Data.Current.Value as string; // what the user managed to type into the textbox so far
            List<AutocompleteResult> timezones = new List<AutocompleteResult>();
            int counter = 0;
            foreach (TimeZoneInfo z in TimeZoneInfo.GetSystemTimeZones())
            {
                if (counter >= 25)
                    break;
                if(z.ToString().ToLower().Contains(subject.ToLower()))
                {
                    timezones.Add(new AutocompleteResult { Name = z.DisplayName, Value = z.StandardName });
                    counter++;
                }
            }
            return AutocompletionResult.FromSuccess(timezones);
        }
    }
}
