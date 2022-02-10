using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace DummyBot.Core.Interactions
{
    public class ButtonTesting : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
    {
        [ComponentInteraction("ButtonTest")]
        public async Task ButtonTest()
        {
            await RespondAsync("I responded!", ephemeral: true);
        }
    }
}
