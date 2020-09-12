using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using Discord.Commands;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Random = System.Random;
using Newtonsoft.Json.Linq;

namespace BotDiscordTest.Modules

{
    public class Test : ModuleBase<SocketCommandContext>
    {
        private string _signUrl;
        private static readonly HttpClient Client = new HttpClient();

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

            _signUrl = await ExtractMedia(url);

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
            var meaningList = await Text(html);
            var list = await ExtractSigns(html);
            
            if (list.Count <= 1)
            {
                await Context.Channel.SendMessageAsync("Sign not found :confused:");
            }
            else
            {
                Program.UserId = Context.User.ToString();
                var message = "";
                
                var links = new string[list.Count];
                for (var i = 0; i < list.Count; i++)
                {
                    links[i] = "https://www.signingsavvy.com/" + list[i];
                    message += (i+1) + ": " + meaningList[i] + "\n";
                }
                Program.Links = links;
                await Context.Channel.SendMessageAsync(message + "\n" + "reply the number for the sign you want");
            }
        }
        public static async Task<string> ExtractMedia(string url)
        {
            var html = await Client.GetStringAsync(url);
            var output = "";
        
            var regex = new Regex("(?:href)=[\"]?(media/.*?)[\"]+", RegexOptions.Singleline | RegexOptions.CultureInvariant);
            if (!regex.IsMatch(html)) return output;
            foreach (Match match in regex.Matches(html))
            {
                output = match.Groups[1].Value;
            }
            return output;
        }
        private static async Task<List<string>> ExtractSigns(string url)
        {
            var html = await Client.GetStringAsync(url);
            var output = new List<string>();
        
            var regex = new Regex("(?:href)=[\"]?(sign/.*?)[\"]+", RegexOptions.Singleline | RegexOptions.CultureInvariant);
            if (!regex.IsMatch(html)) return output;
            foreach (Match match in regex.Matches(html))
            {
                output.Add(match.Groups[1].Value);
            }
            return output;
        }
        private static async Task<List<string>> Text(string url)
        {
            var html = await Client.GetStringAsync(url);
            var list = new List<string>();
            var regex = new Regex("(as in [\"|']?(.*?)[\"|'|>])+", RegexOptions.Singleline | RegexOptions.CultureInvariant);
            if (!regex.IsMatch(html)) return list;
            foreach (Match match in regex.Matches(html))
            {
                var internalArr = match.Groups[1].Value.Split("&quot");
                var value = internalArr[0] + internalArr[1];
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