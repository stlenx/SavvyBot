using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using HtmlAgilityPack;
using Random = System.Random;
using Newtonsoft.Json.Linq;
using ScrapySharp.Extensions;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using static System.Console;
using DSharpPlus.VoiceNext;

namespace SavvyBot.Commands
{
    public class BslCommands
    {
        private static HtmlWeb _web = new HtmlWeb();
        private static HtmlDocument doc;
        
        [Command("bsl")]
        public async Task Irlasl(CommandContext Context,string sign)
        {
            await IrlSign(Context, sign);
        }

        private static async Task IrlSign(CommandContext Context, string sign)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Bsl",
                Description = "Sign not found :confused:",
                Color = DiscordColor.Azure
            };
            var link = ExtractMedia("https://www.signbsl.com/sign/" + sign.ToLower());
            if (link == "")
            {
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                await Context.Channel.SendMessageAsync(link);
            }
        }
        
        private static string ExtractMedia(string url)
        {
            doc = _web.Load(url);
            
            if (doc.DocumentNode.SelectSingleNode("//div[@itemprop='video']") == null) return "";
            var search = doc.DocumentNode.CssSelect(".col-md-12");
            var node = search.CssSelect("source").First();
            return node.GetAttributeValue("src");
        }
    }
}