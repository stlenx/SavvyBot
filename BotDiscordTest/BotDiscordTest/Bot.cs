using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using SavvyBot.Commands;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.Codec;
using DiscordClientExtensions = DSharpPlus.VoiceNext.DiscordClientExtensions;

namespace SavvyBot
{
    public class Bot
    {
        private DiscordClient Client { get; set; }
        private VoiceNextClient Voice { get; set; }
        private CommandsNextModule Commands { get; set; }
        public InteractivityModule Interactivity { get; private set; }
        
        public async Task RunAsync()
        {

            
            var config = new DiscordConfiguration
            {
                Token = "NzM2NzE4MDI2OTkyMjU1MDM3.Xxy4IQ.d2aPgJlEr-xBcVNjzAG9UG-koK8",
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };

            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2)
            });
            
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefix = "!",
                EnableDms = false,
                EnableMentionPrefix = true,
                EnableDefaultHelp = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<AslCommands>();
            Commands.RegisterCommands<TTS>();
            Commands.RegisterCommands<BslCommands>();
            Commands.RegisterCommands<Hentai>();
            
            var vcfg = new VoiceNextConfiguration
            {
                VoiceApplication = VoiceApplication.Music
            };

            this.Voice = this.Client.UseVoiceNext(vcfg);

            await Client.ConnectAsync();
            
            await Task.Delay(-1);
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            Console.WriteLine("Bot is online poggers");
            return Task.CompletedTask;
        }
    }
}