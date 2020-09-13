using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using BotDiscordTest.Modules;

namespace BotDiscordTest
{
    public class CommandHandler
    {
        private static SocketMessage _message;
        
        private readonly DiscordSocketClient _client;

        private readonly CommandService _service;
        
        public CommandHandler(DiscordSocketClient client)
        {
            _client = client;
            
            _service = new CommandService();

            _service.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), 
                services: null);

            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) return;

            _message = s;

            var context = new SocketCommandContext(_client, msg);
            
            var argPos = 0;

            var user1 = context.User;

            if (msg.HasCharPrefix('!', ref argPos))
            {
                var unused = await _service.ExecuteAsync(context, argPos, null);
            } 
            else if (Program.StoredUserResponseDict.ContainsKey(user1.ToString()))
            {
                Program.ReturnLink[] signResponses = Program.StoredUserResponseDict[user1.ToString()];
                Program.ReturnLink chosenLink = signResponses[int.Parse(msg.ToString()) - 1];
                var media = await Test.ExtractMedia(chosenLink.Link);

                await context.Channel.SendMessageAsync("https://www.signingsavvy.com/" + media);

                Program.StoredUserResponseDict.Remove(user1.ToString());
            }
        }
    }
}
