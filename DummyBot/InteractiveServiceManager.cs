using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace DummyBot
{
    public class InteractiveServiceManager
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly InteractionService Interactions;
        private readonly DiscordSocketClient Client;

        public InteractiveServiceManager(IServiceProvider Services)
        {
            ServiceProvider = Services;
            Interactions = ServiceProvider.GetRequiredService<InteractionService>();
            Client = ServiceProvider.GetRequiredService<DiscordSocketClient>();
        }

        public async Task Initialize()
        {
            try
            {
                await Interactions.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);

                foreach (ModuleInfo module in Interactions.Modules)
                {
                    Console.WriteLine($"\u001b[97m[{DateTime.Now}]: [\u001b[93mMODULES\u001b[97m] => {module.Name} \u001b[92mInitialized");
                    Interactions.Log += InteractionServiceLog;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\u001b[97m[{DateTime.Now}]: [\u001b[31mERROR\u001b[97m] => An error occured in InteractiveServiceManager.cs \nError Info:\n{ex}");
            }
        }

        private Task InteractionServiceLog(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }
    }
}
