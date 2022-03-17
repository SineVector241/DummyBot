using Discord;
using Discord.WebSocket;
using Discord.Interactions;

namespace DummyBot.Core.SlashCommands
{
    public class MiscellaneousCommands : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
    {
        [SlashCommand("invite", "Invite the bot to your server")]
        public async Task Invite()
        {
            try
            {
                await DeferAsync();
                var embed = new EmbedBuilder()
                    .WithTitle("Here is the invite link")
                    .WithDescription("[Invite Me](https://discord.com/api/oauth2/authorize?client_id=828203639671488512&permissions=8&scope=bot%20applications.commands)");
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
