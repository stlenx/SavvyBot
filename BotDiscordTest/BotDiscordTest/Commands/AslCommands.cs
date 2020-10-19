using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace SavvyBot.Commands
{
    public class AslCommands
    {
        static HttpClient client = new HttpClient();
        private static HtmlWeb _web = new HtmlWeb();
        private static HtmlDocument doc;
        static DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

        [Command("dadjoke")]
        public async Task DadJoke(CommandContext Context)
        {
            client.DefaultRequestHeaders.Add("Accept","application/json");
            var response = await client.GetAsync("https://icanhazdadjoke.com/");
            
            if (response.IsSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                
                dynamic tmp = JsonConvert.DeserializeObject(responseBody);

                if (tmp != null) await Context.Channel.SendMessageAsync((string) tmp.joke);
            }
        }

        [Command("butt")]
        public async Task WouldYouPressTheButton(CommandContext context)
        {
            PeeLol();
            embed.Color = DiscordColor.Azure;
            var interactivity = context.Client.GetInteractivityModule();
            var content = new JsonContent {sign = "cow", type = "VRSL"};

            var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(content));
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api2.willyoupressthebutton.com/api/v2/dilemma/", httpContent);
            var responseString = await response.Content.ReadAsStringAsync();
            var jsonApi = JObject.Parse(responseString);
            
            var txt1 = jsonApi["dilemma"]?["txt1"]?.ToObject<string>();
            var txt2 = jsonApi["dilemma"]?["txt2"]?.ToObject<string>();
            embed.Title = "Would you press the button?";
            embed.Description = txt1.Replace("&#039;", "'") + " but, \n" + txt2.Replace("&#039;", "'");
            await context.Channel.SendMessageAsync("", false, embed);

            var reply = await interactivity.WaitForMessageAsync(x => x.Channel == context.Channel && x.Author == context.User && x.Content.ToLower() == "yes" || x.Content.ToLower() == "no");

            var yes = jsonApi["dilemma"]?["yes"]?.ToObject<float>();
            var no = jsonApi["dilemma"]?["no"]?.ToObject<float>();
            embed.Title = "Button";
            if (reply.Message.Content.ToLower() == "yes")
            {
                var percentage = (yes / (yes + no)) * 100;
                if (percentage != null)
                {
                    embed.Description = (int) percentage + "% of people said yes";
                    await context.Channel.SendMessageAsync("", false, embed);
                }
            }
            else
            {
                var percentage = (no / (yes + no)) * 100;
                if (percentage != null)
                {
                    embed.Description = (int) percentage + "% of people said no";
                    await context.Channel.SendMessageAsync("", false, embed);
                }
            }
        }

        [Command("pee")]
        public async Task Pee(CommandContext context)
        {
            embed.Title = "pee";
            embed.Description = "";
            embed.ImageUrl =
                "https://cdn.discordapp.com/attachments/735641365320302593/767028760268111933/20200905232425_1.jpg";
            await context.Channel.SendMessageAsync("", false, embed);
        }

        #region asl Shit
        
        [Command("hs")]
        public async Task HandSpeak(CommandContext Context,string sign)
        {
            PeeLol();
            if (CheckUrlStatus("https://www.handspeak.com/word/" + sign[0] + "/" + sign + ".mp4"))
            {
                await Context.Channel.SendMessageAsync("https://www.handspeak.com/word/" + sign[0] + "/" + sign + ".mp4");
            }
            else
            {
                embed.Color = DiscordColor.Azure;
                embed.Title = "Hand Speak";
                embed.Description = "Sign not found :confused:";
                await Context.Channel.SendMessageAsync("", false, embed);
            }
        }

        [Command("sts")]
        public async Task SpreadTheStign(CommandContext context, string sign)
        {
            PeeLol();
            var load = _web.Load("https://www.spreadthesign.com/en.us/search/?q=" + sign);

            if (load.DocumentNode.SelectSingleNode("//div[@class='col-md-7']") == null)
            {
                embed.Color = DiscordColor.Azure;
                embed.Title = "Hand Speak";
                embed.Description = "Sign not found :confused:";
                await context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                var search = load.DocumentNode.CssSelect(".col-md-7");
                var node = search.CssSelect("video").First();
                await context.Channel.SendMessageAsync(node.GetAttributeValue("src"));
            }
        }

        [Command("vrasl")]
        public async Task Tasl(CommandContext Context, string sign)
        {
            PeeLol();
            var content = new JsonContent {sign = sign, type = "VRSL"};

            var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(content));
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://18.133.33.219:3000/signsearch", httpContent);
            
            var responseString = await response.Content.ReadAsStringAsync();
            var jsonApi = JObject.Parse(responseString);
            
            if (jsonApi["result"]?.ToString() == "SignNotFound")
            {
                await IrlSign(Context, sign, 1);
            }
            else
            {
                await Context.Channel.SendMessageAsync(jsonApi["result"]?["url"]?.ToString());
            }
        }
        
        [Command("asl")]
        public async Task Irlasl(CommandContext Context, params string[] sign)
        {
            PeeLol();
            embed.Color = DiscordColor.Azure;
            embed.Title = "Signing Savvy";
            var input = sign.Aggregate("", (current, t) => current + (t + " "));
            var selection = ContainsInt(sign) ?? 1;
            await IrlSign(Context, input, selection);
        }

        [Command("daysign")]
        public async Task SignOfTheDay(CommandContext context)
        {
            var website = _web.Load("https://www.signingsavvy.com/");
            
            var searchTwo = website.GetElementbyId("sotd");
            var a = searchTwo.CssSelect("p").ToArray();

            var search = website.DocumentNode.CssSelect(".videocontent");
            var node = search.CssSelect("link").First();
            await context.Channel.SendMessageAsync(a[1].InnerText + "\n" + "https://www.signingsavvy.com/" + node.GetAttributeValue("href"));
        }

        [Command("asltest")]
        public async Task AslTest(CommandContext context)
        {
            PeeLol();
            var interactivity = context.Client.GetInteractivityModule();
            Random random = new Random();
            int a = random.Next(0, 26);
            char ch = (char)('A' + a);
            
            var site = _web.Load("https://www.signingsavvy.com/browse/" + ch);
            var search = site.DocumentNode.CssSelect(".search_results");
            var htmlNodes = search.CssSelect("li").ToArray();

            var rnd = random.Next(0,htmlNodes.Length);

            var nodesLol = htmlNodes[rnd].CssSelect("a").ToArray();
            var contextLol = htmlNodes[rnd].CssSelect("em").ToArray();
            var link = ExtractMedia("https://www.signingsavvy.com/" + nodesLol[0].GetAttributeValue("href"));
            var words = ExtractSynon();

            words.Add(nodesLol[0].InnerText.ToLower().Replace(" ", string.Empty));
            var value = "";
            if (contextLol.Length != 0)
            {
                var internalArr = contextLol[0].InnerHtml.Split("&quot");
                value =internalArr[0] + internalArr[1] + internalArr[2];
            }

            var user = context.User.ToString().Split('(');
            await context.Channel.SendMessageAsync("https://www.signingsavvy.com/" + link + "\n" + "What is this sign? You have 3 attempts | Requested by: " + user[1].Trim(')'));

            Console.WriteLine(nodesLol[0].InnerText.ToLower());
            Console.WriteLine(value);
            var i = 0;
            var poguMessage = string.Empty;
            
            embed.Color = DiscordColor.Azure;
            embed.Title = "Asl Test";
            
            while (i < 3)
            {
                var reply = await interactivity.WaitForMessageAsync
                    (x => x.Channel == context.Channel && x.Author == context.User);
                if (words.Contains(reply.Message.Content.ToLower()))
                {
                    embed.Description = "Good job! you got it :smile:";
                    await context.Channel.SendMessageAsync("", false, embed);
                    i = 10;
                    break;
                }
                i++;
                if ((3 - i) == 0) continue;
                embed.Description = "Nope, you've got " + (3 - i) + " attempts left";
                await context.Channel.SendMessageAsync("", false, embed);
            }

            poguMessage = words.Where((t, x) => x < 5).Aggregate(poguMessage, (current, t) => current + (t + " | "));

            if (i != 10)
            {
                embed.Description = "Too bad, try again next time!" + "\n" + "The sign was: " + poguMessage + value;
                await context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                embed.Description = "The sign was: " + poguMessage + value;
                await context.Channel.SendMessageAsync("", false, embed);
            }
        }

        #endregion
        
        [Command("help")]
        public async Task Help(CommandContext Context)
        {
            PeeLol();
            embed.Color = DiscordColor.Azure;
            embed.Title = "Commands";
            embed.Description = "!dadjoke - Gives random dad joke \n" +
                                "!asl - Gives the sign for the word you send (ex: !asl hello) \n" +
                                "!vrasl- Gives the sign for the word you send (vr) (ex: !asl hello) (returns normal sign if not found) \n" +
                                "!daysign - Gives the sign of the day \n" +
                                "!asltest - Game! Can you guess the sign? \n" +
                                "!hs - Gives the sign for the word you send from handspeak (experimental, only one word) \n" +
                                "!bsl - Gives the sign for the word you send in bsl (temporal, not 100% viable) \n" +
                                "!sts - Gives the sign for the word you send from Spread The Sign";

            await Context.Channel.SendMessageAsync("", false, embed);
        }
        private async Task IrlSign(CommandContext Context, string sign, int selection)
        {
            var signn = RemoveSpecialChars(sign);
            var url = "https://www.signingsavvy.com/search/" + signn;

            var signUrl = ExtractMedia(url);

            if (signUrl != null)
            {
                if (selection > 1)
                {
                    var OriginalLink = ExtractSecondUrl(url);
                    await Context.Channel.SendMessageAsync("https://www.signingsavvy.com/" + ExtractMedia("https://www.signingsavvy.com/" + OriginalLink.Remove(OriginalLink.Length - 1) + selection));
                }
                else
                {
                    await Context.Channel.SendMessageAsync("https://www.signingsavvy.com/" + signUrl);
                }
            }
            else
            {
                await GetOtherStuff(Context, url, signn, selection);
            }
        }

        private async Task GetOtherStuff(CommandContext Context, string html, string sign, int selection)
        {
            embed.Description = "Sign not found :confused:";
            var list = ExtractSigns(sign);
            if (list == null)
            {
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                var meaningList = Text(html);

                if (list.Count <= 1)
                {
                    await Context.Channel.SendMessageAsync("", false, embed);
                }
                else
                {
                    var interactivity = Context.Client.GetInteractivityModule();
                    var links = new string[list.Count];
                    var message = "";
                    for (var i = 0; i < list.Count; i++)
                    {
                        links[i] = "https://www.signingsavvy.com/" + list[i];
                        message += (i+1) + ": " + meaningList[i] + "\n";
                    }

                    embed.Description = message + "\n" + "reply the number for the sign you want";
                    await Context.Channel.SendMessageAsync("", false, embed);
                    
                    var reply = await interactivity.WaitForMessageAsync
                        (x => x.Channel == Context.Channel && x.Author == Context.User && IsDigitsOnly(x.Content));

                    if (selection > 1)
                    {
                        var OriginalLink = ExtractSecondUrl(links[Int32.Parse(reply.Message.Content)-1]);
                        await Context.Channel.SendMessageAsync("https://www.signingsavvy.com/" + ExtractMedia("https://www.signingsavvy.com/" + OriginalLink.Remove(OriginalLink.Length - 1) + selection));
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("https://www.signingsavvy.com/" + ExtractMedia(links[Int32.Parse(reply.Message.Content)-1]));
                    }
                }
            }
        }
        
        private static bool IsDigitsOnly(string str)
        {
            return str.All(c => c >= '0' && c <= '9');
        }

        private static int? ContainsInt(IEnumerable<string> input)
        {
            foreach (var word in input)
            {
                if (IsDigitsOnly(word))
                {
                    return Int32.Parse(word);
                }
            }
            return null;
        }

        private static string ExtractMedia(string url)
        {
            doc = _web.Load(url);

            if (doc.DocumentNode.SelectSingleNode("//div[@class='videocontent']") == null) return null;
            var search = doc.DocumentNode.CssSelect(".videocontent");
            var node = search.CssSelect("link").First();
            return node.GetAttributeValue("href");
        }

        private static string ExtractSecondUrl(string url)
        {
            var load = _web.Load(url);
            var search = load.DocumentNode.CssSelect(".signing_header");
            var node = search.CssSelect("a").Last();
            return node.GetAttributeValue("href");
        }

        private static List<string> ExtractSynon()
        {
            var list = new List<string>();
            var search = doc.DocumentNode.CssSelect(".desc");
            var nodes = search.CssSelect("a").ToArray();
            foreach (var node in nodes)
            {
                var shitArr = node.InnerText.ToLower().Split(" ");
                if (node.InnerText.ToLower() == "sign up now!" || shitArr[0] == "variation" || node.InnerText.ToLower() == "word lists") continue;
                var kankerArr = node.InnerText.Split("(");
                list.Add(kankerArr[0].ToLower().Replace(" ", string.Empty));
            }
            return list;
        }

        private static List<string> ExtractSigns(string sign)
        {
            if (doc.DocumentNode.SelectSingleNode("//div[@class='.search_results']") != null)
            {
                return null;
            }
            var search = doc.DocumentNode.CssSelect(".search_results");

            var htmlNodes = search.CssSelect("a").ToArray();

            return htmlNodes.Length > 10 ? null : search.CssSelect("a").Select(node => node.GetAttributeValue("href", "")).ToList();
        }

        private static List<string> Text(string url)
        {
            var list = new List<string>();
            doc = _web.Load(url);
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
        protected bool CheckUrlStatus(string Website)
        {
            try
            {
                var request = WebRequest.Create(Website) as HttpWebRequest;
                request.Method = "HEAD";
                using var response = (HttpWebResponse)request.GetResponse();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }

        private static void PeeLol()
        {
            Random n = new Random();
            embed.ImageUrl = n.Next(0, 100) == 50 ? "https://cdn.discordapp.com/attachments/735641365320302593/767028760268111933/20200905232425_1.jpg" : "";
        }
        private static string RemoveSpecialChars(string input)
        {
            var chars = new[]
            {
                ",", ".", "/", "!", "@", "#", "$", "%", "^", "&", "*", "'", @"\", ";", "_", "(", ")", ":", "|", "[",
                "]", "<", ">", "=", "-", "+", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0"
            };
            foreach (var t in chars)
            {
                if (input.Contains(t))
                {
                    input = input.Replace(t, "");
                }
            }
            return input;
        }
    }
    internal class JsonContent
    {
        public string sign;
        public string type;
    }
}