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
using DSharpPlus.Interactivity;
using static System.Console;
using DSharpPlus.VoiceNext;

namespace SavvyBot.Commands
{
    public class Hentai
    {
        [Command("hentaiTags")]
        public async Task HentaiWithTags(CommandContext Context,params string[] tags)
        {
            var r = new Random();
            var result = await NHentaiSharp.Core.SearchClient.SearchWithTagsAsync(tags);
            var page = r.Next(0, result.numPages) + 1;
            result = await NHentaiSharp.Core.SearchClient.SearchWithTagsAsync(tags, page);
            var doujinshi = result.elements[r.Next(0, result.elements.Length)];
            await Context.Channel.SendMessageAsync("Random doujinshi: " + doujinshi.prettyTitle + "   " +
                                                   "URL: " + doujinshi.url);
        }
        
        [Command("hentai")]
        public async Task HHentai(CommandContext Context)
        {
            var r = new Random();
            var result = await NHentaiSharp.Core.SearchClient.SearchAsync();
            var page = r.Next(0, result.numPages) + 1;
            result = await NHentaiSharp.Core.SearchClient.SearchAsync(page);
            var doujinshi = result.elements[r.Next(0, result.elements.Length)];
            await Context.Channel.SendMessageAsync("Random doujinshi: " + doujinshi.prettyTitle + "   " +
                                                   "URL: " + doujinshi.url);
        }
    }
}