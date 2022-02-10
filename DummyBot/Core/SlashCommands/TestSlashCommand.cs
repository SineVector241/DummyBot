using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DummyBot.Core.SlashCommands
{
    public class TestSlashCommand : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
    {
        private readonly HttpClient client = new HttpClient();

        [SlashCommand("buttontest","Test button command",runMode: RunMode.Async)]
        public async Task ButtonTest()
        {
            var builder = new ComponentBuilder()
                .WithButton("Button!","ButtonTest");
            await RespondAsync("Heres a button", components: builder.Build());
        }

        [SlashCommand("repeat","Repeats what you say",runMode:RunMode.Async)]
        public async Task Repeat([Summary("input", "Text you want the bot to repeat")] string input)
        {
            await RespondAsync(input);
        }

        [SlashCommand("searchplayer", "Searches for a warbokers player by name")]
        public async Task SearchPlayer([Summary(description:"The player name")] string player)
        {
            try
            {
                player = player.ToLower();
                WebRequest request = WebRequest.Create($"https://stats.warbrokers.io/players/search?term={player}");
                request.Method = "GET";
                using WebResponse webResponse = request.GetResponse();
                using Stream webStream = webResponse.GetResponseStream();

                using StreamReader reader = new StreamReader(webStream);
                string data = reader.ReadToEnd();
                data = data + "}";
                data = "{ \"players\": " + data;
                JObject jsonData = JObject.Parse(data);
                dynamic Data = JsonConvert.DeserializeObject(data);
                string result = null;

                foreach (JArray value in Data["players"])
                {
                    result += $"Name: {value[0]} ID: {value[1]}\n";
                }
                await RespondAsync("PlayerList\n"+result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await RespondAsync("An error occured");
            }
        }
    }
}
