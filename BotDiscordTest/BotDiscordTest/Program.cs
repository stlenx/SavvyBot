using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Xml;
using TokenType = Discord.TokenType;

namespace BotDiscordTest
{
    public class Program
    {
        public struct ReturnLink
        {
            public string AsInContext;
            public string Link;
        }
        
        public static Dictionary<string, ReturnLink[]> StoredUserResponseDict = new Dictionary<string, ReturnLink[]>();

        private static void Main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;

        private async Task StartAsync()
        {
            _client = new DiscordSocketClient();

            var token = await File.ReadAllTextAsync(System.AppDomain.CurrentDomain.BaseDirectory + "token.txt");

            ApiHelper.InitializeClient();

            await _client.LoginAsync(TokenType.Bot, token);
            
            await _client.StartAsync();

            var commandHandler = new CommandHandler(_client);

            await Task.Delay(-1);
        }
    }
}
