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
            else if (Program.UserId == user1.ToString())
            {
                var media = await Test.ExtractMedia(Program.Links[int.Parse(msg.ToString())-1]);
                
                await context.Channel.SendMessageAsync("https://www.signingsavvy.com/" + media);
                
                Program.UserId = "SomeRAndo#6969lol";
            }
        }
    }
}
