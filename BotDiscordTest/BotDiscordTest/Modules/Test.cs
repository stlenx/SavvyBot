using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using Discord.Commands;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Random = System.Random;
using Newtonsoft.Json.Linq;
using ScrapySharp.Extensions;

namespace BotDiscordTest.Modules

{
    public class Test : ModuleBase<SocketCommandContext>
    {
        private string _signUrl;
        private static readonly HttpClient Client = new HttpClient();
        private static HtmlWeb _web = new HtmlWeb();

        #region Hentai

        [Command("hentaiTags")]
        public async Task HentaiWithTags(params string[] tags)
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
        public async Task Hentai()
        {
            var r = new Random();
            var result = await NHentaiSharp.Core.SearchClient.SearchAsync();
            var page = r.Next(0, result.numPages) + 1;
            result = await NHentaiSharp.Core.SearchClient.SearchAsync(page);
            var doujinshi = result.elements[r.Next(0, result.elements.Length)];
            await Context.Channel.SendMessageAsync("Random doujinshi: " + doujinshi.prettyTitle + "   " +
                                                   "URL: " + doujinshi.url);
        }

        #endregion

        [Command("dadJoke")]
        public async Task DadJoke()
        {
            using var response = await ApiHelper.ApiClient.GetAsync("https://icanhazdadjoke.com/");
            if (response.IsSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                
                dynamic tmp = JsonConvert.DeserializeObject(responseBody);

                if (tmp != null) await Context.Channel.SendMessageAsync((string) tmp.joke);
            }
        }

        #region asl Shit

        [Command("vrAsl")]
        public async Task Tasl(string sign)
        {
            var content = new JsonContent {Sign = sign};

            var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(content));
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("http://18.133.33.219:3000/signsearch", httpContent);
            
            var responseString = await response.Content.ReadAsStringAsync();
            var jsonApi = JObject.Parse(responseString);
            
            if (jsonApi["result"]?.ToString() == "SignNotFound")
            {
                await IrlSign(sign);
            }
            else
            {
                await Context.Channel.SendMessageAsync(jsonApi["result"]?["url"]?.ToString());
            }
        }
        
        [Command("asl")]
        public async Task Irlasl(params string[] sign)
        {
            var input = sign.Aggregate("", (current, t) => current + (t + " "));
            await IrlSign(input);
        }
        
        #endregion
        
        [Command("help")]
        public async Task Help()
        {
            await Context.Channel.SendMessageAsync
            ("!hentai - Gives random hentai \n " +
             "!hentaiTags - Gives random hentai with the indicated tags (separate tags with a space) \n" + 
             "!dadJoke - Gives random dad joke \n" +
             "!asl - gives the sign for the word you send (ex: !asl hello) \n" +
             "!vrAsl- gives the sign for the word you send (vr) (ex: !asl hello) (returns normal sign if not found)");
        }

        #region Functions

        private async Task IrlSign(string sign)
        {
            var realUrl = RemoveSpecialChars(sign);
            
            var url = "https://www.signingsavvy.com/search/" + realUrl;

            _signUrl = ExtractMedia(url);

            if (_signUrl != "")
            {
                await Context.Channel.SendMessageAsync("https://www.signingsavvy.com/" + _signUrl);
            }
            else
            {
                await GetOtherStuff(url);
            }
        }
        
        private async Task GetOtherStuff(string html)
        {
            var meaningList = Text(html);
            var list = ExtractSigns(html);
            
            if (list.Count <= 1)
            {
                await Context.Channel.SendMessageAsync("Sign not found :confused:");
            }
            else
            {
                var message = "";
                var links = new Program.ReturnLink[list.Count];
                for (var i = 0; i < list.Count; i++)
                {
                    links[i].Link = "https://www.signingsavvy.com/" + list[i];
                    links[i].AsInContext = "";
                    message += (i+1) + ": " + meaningList[i] + "\n";
                }
                Program.StoredUserResponseDict.Add(Context.User.ToString(), links);
                await Context.Channel.SendMessageAsync(message + "\n" + "reply the number for the sign you want");
            }
        }
        public static string ExtractMedia(string url)
        {
            var output = "";
            
            var doc = _web.Load(url);

            if (doc.DocumentNode.SelectSingleNode("//div[@class='videocontent']") == null) return output;
            var search = doc.DocumentNode.CssSelect(".videocontent");
            var node = search.CssSelect("link").First();
            output = node.GetAttributeValue("href");
            
            return output;
        }
        private static List<string> ExtractSigns(string url)
        {
            var output = new List<string>();
            
            var doc = _web.Load(url);
            var search = doc.DocumentNode.CssSelect(".search_results");
            foreach (var node in search.CssSelect("a"))
            {
                output.Add(node.GetAttributeValue("href",""));
            }
            return output;
        }
        private static List<string> Text(string url)
        {
            var list = new List<string>();
            var doc = _web.Load(url);
            var search = doc.DocumentNode.CssSelect(".search_results");
            foreach (var node in search.CssSelect("li"))
            {
                var result = node.CssSelect("em").First();
                var internalArr = result.InnerHtml.Split("&quot");
                var value = internalArr[0] + internalArr[1] + internalArr[2];
                list.Add(value);
            }
            return list;
        }
        private static string RemoveSpecialChars(string str)
        {
            var chars = new[] { ",", ".", "/", "!", "@", "#", "$", "%", "^", "&", "*", "'", @"\", ";", "_", "(", ")", ":", "|", "[", "]","<",">","=", "-", "+" };
            
            foreach (var t in chars)
            {
                if (str.Contains(t))
                {
                    str = str.Replace(t, "");
                }
            }
            return str;
        }
        
        #endregion
    }
    internal class JsonContent
    {
        public string Sign;
    }
}
