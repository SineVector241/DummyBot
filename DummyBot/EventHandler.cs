using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using Discord;

namespace DummyBot
{
    public class EventHandler
    {
        private DiscordSocketClient Client;
        private readonly InteractionService Interactions;
        private readonly IServiceProvider ServiceProvider;

        public EventHandler(IServiceProvider Services)
        {
            ServiceProvider = Services;
            Interactions = Services.GetRequiredService<InteractionService>();
            Client = Services.GetRequiredService<DiscordSocketClient>();
        }

        public Task Initialize()
        {
            Client.Ready += Ready;
            Client.InteractionCreated += InteractionCreated;
            return Task.CompletedTask;
        }

        private async Task InteractionCreated(SocketInteraction interaction)
        {
            if (interaction is SocketSlashCommand)
            {
                var ctx = new SocketInteractionContext<SocketSlashCommand>(Client, (SocketSlashCommand)interaction);
                await Interactions.ExecuteCommandAsync(ctx, ServiceProvider);
            }
            else if (interaction is SocketMessageComponent)
            {
                var ctx = new SocketInteractionContext<SocketMessageComponent>(Client, (SocketMessageComponent)interaction);
                await Interactions.ExecuteCommandAsync(ctx, ServiceProvider);
            }
            else if (interaction is SocketAutocompleteInteraction)
            {
                var ctx = new SocketInteractionContext(Client, (SocketAutocompleteInteraction)interaction);
                await Interactions.ExecuteCommandAsync(ctx, ServiceProvider);
            }
        }

        private async Task Ready()
        {
            try
            {
                Console.WriteLine($"\u001b[97m[{DateTime.Now}]: [\u001b[92mREADY\u001b[97m] => {Client.CurrentUser.Username} is ready!");
                await Client.SetGameAsync("/help");
                await Client.SetStatusAsync(UserStatus.Online);
#if DEBUG
                await Interactions.RegisterCommandsToGuildAsync(797094263568465970);
#else
                await Interactions.RegisterCommandsGloballyAsync();
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\u001b[97m[{DateTime.Now}]: [\u001b[31mERROR\u001b[97m] => An error occured in EventHandler.cs \nError Info:\n{ex}");
            }
        }
    }
}
