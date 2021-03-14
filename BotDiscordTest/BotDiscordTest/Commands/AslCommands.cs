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
using Quartz;
using Quartz.Impl;

namespace SavvyBot.Commands
{
    public class AslCommands
    {
        private static readonly HttpClient Client = new HttpClient();
        private static readonly HtmlWeb Web = new HtmlWeb();
        private static HtmlDocument _doc;
        private static readonly DiscordEmbedBuilder Embed = new DiscordEmbedBuilder();

        [Command("dadjoke")]
        public async Task DadJoke(CommandContext context)
        {
            Client.DefaultRequestHeaders.Add("Accept","application/json");
            var response = await Client.GetAsync("https://icanhazdadjoke.com/");
            
            if (response.IsSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                
                dynamic tmp = JsonConvert.DeserializeObject(responseBody);

                Console.WriteLine(tmp);
                if (tmp != null) await context.Channel.SendMessageAsync((string) tmp.joke);
            }
        }

        [Command("butt")]
        public async Task WouldYouPressTheButton(CommandContext context)
        {
            PeeLol();
            Embed.Color = DiscordColor.Azure;
            var interactivity = context.Client.GetInteractivityModule();
            
            var httpContent = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("https://api2.willyoupressthebutton.com/api/v2/dilemma/", httpContent);
            var jsonApi = JObject.Parse(await response.Content.ReadAsStringAsync());
            
            var txt1 = jsonApi["dilemma"]?["txt1"]?.ToObject<string>();
            var txt2 = jsonApi["dilemma"]?["txt2"]?.ToObject<string>();
            Embed.Title = "Would you press the button?";
            Embed.Description = txt1?.Replace("&#039;", "'") + " but, \n" + txt2?.Replace("&#039;", "'");
            await context.Channel.SendMessageAsync("", false, Embed);

            var reply = await interactivity.WaitForMessageAsync(x => x.Channel == context.Channel && x.Author == context.User && x.Content.ToLower() == "yes" || x.Content.ToLower() == "no");

            var yes = jsonApi["dilemma"]?["yes"]?.ToObject<float>();
            var no = jsonApi["dilemma"]?["no"]?.ToObject<float>();
            Embed.Title = "Button";
            if (reply.Message.Content.ToLower() == "yes")
            {
                var percentage = (yes / (yes + no)) * 100;
                if (percentage != null)
                {
                    Embed.Description = (int) percentage + "% of people said yes";
                    await context.Channel.SendMessageAsync("", false, Embed);
                }
            }
            else
            {
                var percentage = (no / (yes + no)) * 100;
                if (percentage != null)
                {
                    Embed.Description = (int) percentage + "% of people said no";
                    await context.Channel.SendMessageAsync("", false, Embed);
                }
            }
        }

        [Command("pee")]
        public async Task Pee(CommandContext context)
        {
            Embed.Title = "pee";
            Embed.Description = "";
            Embed.ImageUrl =
                "https://cdn.discordapp.com/attachments/735641365320302593/767028760268111933/20200905232425_1.jpg";
            await context.Channel.SendMessageAsync("", false, Embed);
        }

        #region asl Shit
        
        [Command("hs")]
        public async Task HandSpeak(CommandContext context, string sign)
        {
            PeeLol();
            if (CheckUrlStatus("https://www.handspeak.com/word/" + sign[0] + "/" + sign + ".mp4"))
            {
                await context.Channel.SendMessageAsync("https://www.handspeak.com/word/" + sign[0] + "/" + sign + ".mp4");
            }
            else
            {
                Embed.Color = DiscordColor.Azure;
                Embed.Title = "Hand Speak";
                Embed.Description = "Sign not found :confused:";
                await context.Channel.SendMessageAsync("", false, Embed);
            }
        }

        [Command("sts")]
        public async Task SpreadTheStign(CommandContext context, string sign)
        {
            PeeLol();
            var load = Web.Load("https://www.spreadthesign.com/en.us/search/?q=" + sign);

            if (load.DocumentNode.SelectSingleNode("//div[@class='col-md-7']") == null)
            {
                Embed.Color = DiscordColor.Azure;
                Embed.Title = "Spread the sign";
                Embed.Description = "Sign not found :confused:";
                await context.Channel.SendMessageAsync("", false, Embed);
            }
            else
            {
                var search = load.DocumentNode.CssSelect(".col-md-7");
                var node = search.CssSelect("video").First();
                await context.Channel.SendMessageAsync(node.GetAttributeValue("src"));
            }
        }

        [Command("vrasl")]
        public async Task Tasl(CommandContext context, string sign)
        {
            PeeLol();
            var content = new JsonContent {sign = sign, type = "VRSL"};

            var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(content));
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("http://18.133.33.219:3000/signsearch", httpContent);
            
            var responseString = await response.Content.ReadAsStringAsync();
            var jsonApi = JObject.Parse(responseString);
            
            if (jsonApi["result"]?.ToString() == "SignNotFound")
            {
                try
                {
                    await IrlSign(context, sign, 1);
                }
                catch (Exception)
                {
                    Embed.Description = "Sign not found :confused:";
                    await context.Channel.SendMessageAsync("", false, Embed);
                    throw;
                }
            }
            else
            {
                await context.Channel.SendMessageAsync(jsonApi["result"]?["url"]?.ToString());
            }
        }
        
        [Command("asl")]
        public async Task Irlasl(CommandContext context, params string[] sign)
        {
            PeeLol();
            Embed.Color = DiscordColor.Azure;
            Embed.Title = "Signing Savvy";
            var input = RemoveSpecialChars(sign.Aggregate("", (current, t) => current + (t + " ")));
            var selection = ContainsInt(sign) ?? 1;
            if (Bot.cache.ContainsKey(input + " | " + selection))
            {
                await context.Channel.SendMessageAsync(Bot.cache[input + " | " + selection]);
            }
            else
            {
                try
                {
                    await IrlSign(context, input, selection);
                }
                catch (Exception)
                {
                    Embed.Description = "Sign not found :confused:";
                    await context.Channel.SendMessageAsync("", false, Embed);
                    throw;
                }
            }
        }

        [Command("daysign")]
        public async Task SignOfTheDay(CommandContext context)
        {
            var website = Web.Load("https://www.signingsavvy.com/");
            
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
            var random = new Random();
            var a = random.Next(0, 26);
            var ch = (char)('A' + a);
            
            var site = Web.Load("https://www.signingsavvy.com/browse/" + ch);
            var search = site.DocumentNode.CssSelect(".search_results");
            var htmlNodes = search.CssSelect("li").ToArray();

            var rnd = random.Next(0,htmlNodes.Length);

            
            /*
            char[] letters = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z' };

            foreach (var letter in letters)
            {
                var site = Web.Load("https://www.signingsavvy.com/browse/" + letter);
                var search = site.DocumentNode.CssSelect(".search_results");
                var htmlNodes = search.CssSelect("li").ToArray();
                foreach (var node in htmlNodes)
                {
                    var nodesLol = node.CssSelect("a").ToArray();
                    var contextLol = node.CssSelect("em").ToArray();
                    var link = ExtractMedia("https://www.signingsavvy.com/" + nodesLol[0].GetAttributeValue("href"));
                    Console.WriteLine(link);
                }
            }
            */


            

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
            await context.Channel.SendMessageAsync(link + "\n" + "What is this sign? You have 3 attempts | Requested by: " + user[1].Trim(')'));

            Console.WriteLine(nodesLol[0].InnerText.ToLower());
            Console.WriteLine(value);
            var i = 0;
            var poguMessage = string.Empty;
            
            Embed.Color = DiscordColor.Azure;
            Embed.Title = "Asl Test";
            
            while (i < 3)
            {
                var reply = await interactivity.WaitForMessageAsync
                    (x => x.Channel == context.Channel && x.Author == context.User);
                if (words.Contains(reply.Message.Content.ToLower()))
                {
                    Embed.Description = "Good job! you got it :smile:";
                    await context.Channel.SendMessageAsync("", false, Embed);
                    i = 10;
                    break;
                }
                i++;
                if ((3 - i) == 0) continue;
                Embed.Description = "Nope, you've got " + (3 - i) + " attempts left";
                await context.Channel.SendMessageAsync("", false, Embed);
            }

            poguMessage = words.Where((t, x) => x < 5).Aggregate(poguMessage, (current, t) => current + (t + " | "));

            if (i != 10)
            {
                Embed.Description = "Too bad, try again next time!" + "\n" + "The sign was: " + poguMessage + value;
                await context.Channel.SendMessageAsync("", false, Embed);
            }
            else
            {
                Embed.Description = "The sign was: " + poguMessage + value;
                await context.Channel.SendMessageAsync("", false, Embed);
            }
            
        }

        #endregion
        
        [Command("help")]
        public async Task Help(CommandContext context)
        {
            PeeLol();
            Embed.Color = DiscordColor.Azure;
            Embed.Title = "Commands";
            Embed.Description = "!dadjoke - Gives random dad joke \n" +
                                "!asl - Gives the sign for the word you send (ex: !asl hello) \n" +
                                "!vrasl- Gives the sign for the word you send (vr) (ex: !asl hello) (returns normal sign if not found) \n" +
                                "!daysign - Gives the sign of the day \n" +
                                "!asltest - Game! Can you guess the sign? \n" +
                                "!hs - Gives the sign for the word you send from handspeak (experimental, only one word) \n" +
                                "!bsl - Gives the sign for the word you send in bsl (temporal, not 100% viable) \n" +
                                "!sts - Gives the sign for the word you send from Spread The Sign";
            await context.Channel.SendMessageAsync("", false, Embed);
        }

        [Command("remember")]
        public async Task Remember(CommandContext context, params int[] nums)
        {
            if (context.User.ToString() != "stlenx" && context.User.Discriminator != "1822")
            {
                var s = await context.Guild.GetMemberAsync(context.User.Id);
                await s.SendMessageAsync("You do not have permission poo poo head");
            }
            else
            {
                var interactivity = context.Client.GetInteractivityModule();
                
                var u = await context.Guild.GetMemberAsync(context.User.Id);

                await context.Channel.SendMessageAsync("Please enter your message u cunt");
                var reply = await interactivity.WaitForMessageAsync
                    (x => x.Channel == context.Channel && x.Author == context.User);
                
                var factory = new StdSchedulerFactory();
                var scheduler = await factory.GetScheduler();

                await scheduler.Start();

                var job = JobBuilder.Create<reminders>()
                    .WithIdentity(CreateMd5(reply.Message.Content),"group1").Build();
                job.JobDataMap.Put("u", u);
                job.JobDataMap.Put("m", reply.Message.Content);
                
                var trigger = TriggerBuilder.Create().WithIdentity(CreateMd5(reply.Message.Content) + "cum", "group1")
                    .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(nums[0], nums[1])).ForJob(job).StartNow().Build();
                
                await scheduler.ScheduleJob(job, trigger);

                await context.Channel.SendMessageAsync("okay noted i will remind u hihi :blush:");
                
                var jobs = scheduler.GetCurrentlyExecutingJobs();
                foreach (var joblol in jobs.Result)
                {
                    Console.WriteLine(joblol.JobDetail.Description);
                }
            }
        }

        private static string CreateMd5(string input)
        {
            // Use input string to calculate MD5 hash
            using var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            
            // Convert the byte array to hexadecimal string
            var sb = new StringBuilder();
            foreach (var t in hashBytes)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }

        private static async Task IrlSign(CommandContext context, string sign, int selection)
        {
            var url = "https://www.signingsavvy.com/search/" + sign;
            var signUrl = ExtractMedia(url);
            
            if (signUrl != null)
            {
                if (selection > 1)
                {
                    var originalLink = ExtractSecondUrl(url);
                    await context.Channel.SendMessageAsync(ExtractMedia("https://www.signingsavvy.com/" + originalLink.Remove(originalLink.Length - 1) + selection));
                    Bot.cache.Add(sign + " | " + selection, ExtractMedia("https://www.signingsavvy.com/" + originalLink.Remove(originalLink.Length - 1) + selection));
                }
                else
                {
                    Bot.cache.Add(sign + " | " + selection, signUrl);
                    await context.Channel.SendMessageAsync(signUrl);
                }
            }
            else
            {
                await GetOtherStuff(context, url, selection, sign);
            }
        }

        private static async Task GetOtherStuff(CommandContext context, string html, int selection, string sign)
        {
            Embed.Description = "Sign not found :confused:";
            Embed.Title = char.ToUpper(sign[0]) + sign.Substring(1);

            if (Bot.cacheList.ContainsKey(sign))
            {
                var message = Bot.cacheList[sign][0];
                var interactivity = context.Client.GetInteractivityModule();
                
                Embed.Description = message + "\n" + "reply the number for the sign you want";
                await context.Channel.SendMessageAsync("", false, Embed);
                    
                var reply = await interactivity.WaitForMessageAsync
                    (x => x.Channel == context.Channel && x.Author == context.User && IsDigitsOnly(x.Content));
                    
                if (Bot.cache.ContainsKey(sign + " | " + selection + " | listed" + int.Parse(reply.Message.Content)))
                {
                    await context.Channel.SendMessageAsync(Bot.cache[sign + " | " + selection + " | listed" + int.Parse(reply.Message.Content)]);
                }
                else
                {
                    if (selection > 1)
                    {
                        var originalLink = ExtractSecondUrl(Bot.cacheList[sign][int.Parse(reply.Message.Content)]);
                        await context.Channel.SendMessageAsync(ExtractMedia("https://www.signingsavvy.com/" + originalLink.Remove(originalLink.Length - 1) + selection));
                        Bot.cache.Add(sign + " | " + selection + " | listed" + int.Parse(reply.Message.Content), 
                            ExtractMedia("https://www.signingsavvy.com/" + originalLink.Remove(originalLink.Length - 1) + selection));
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync(ExtractMedia(Bot.cacheList[sign][int.Parse(reply.Message.Content)]));
                        Bot.cache.Add(sign + " | " + selection + " | listed" + int.Parse(reply.Message.Content), 
                            ExtractMedia(Bot.cacheList[sign][int.Parse(reply.Message.Content)]));
                    }
                }
            }
            else
            {
                var list = ExtractSigns();
            if (list == null)
            {
                await context.Channel.SendMessageAsync("", false, Embed);
            }
            else
            {
                var meaningList = Text(html);

                if (list.Count <= 1)
                {
                    await context.Channel.SendMessageAsync("", false, Embed);
                }
                else
                {
                    var interactivity = context.Client.GetInteractivityModule();
                    
                    var links = new List<string>();
                    var message = "";
                    
                    Bot.cacheList.Add(sign, new List<string>());
                    Bot.cacheList[sign].Add("message");
                    
                    for (var i = 0; i < list.Count; i++)
                    {
                        links.Add("https://www.signingsavvy.com/" + list[i]);
                        message += (i+1) + ": " + meaningList[i] + "\n";
                        
                        Bot.cacheList[sign].Add("https://www.signingsavvy.com/" + list[i]);
                    }

                    Embed.Description = message + "\n" + "reply the number for the sign you want";
                    Bot.cacheList[sign][0] = message + "\n" + "reply the number for the sign you want";
                    
                    await context.Channel.SendMessageAsync("", false, Embed);
                    
                    var reply = await interactivity.WaitForMessageAsync
                        (x => x.Channel == context.Channel && x.Author == context.User && IsDigitsOnly(x.Content));
                    
                    if (Bot.cache.ContainsKey(sign + " | " + selection + " | listed" + int.Parse(reply.Message.Content)))
                    {
                        await context.Channel.SendMessageAsync(Bot.cache[sign + " | " + selection + " | listed" + int.Parse(reply.Message.Content)]);
                    }
                    else
                    {
                        if (selection > 1)
                        {
                            var originalLink = ExtractSecondUrl(links[int.Parse(reply.Message.Content)-1]);
                            await context.Channel.SendMessageAsync(ExtractMedia("https://www.signingsavvy.com/" + originalLink.Remove(originalLink.Length - 1) + selection));
                            Bot.cache.Add(sign + " | " + selection + " | listed" + int.Parse(reply.Message.Content), 
                                ExtractMedia("https://www.signingsavvy.com/" + originalLink.Remove(originalLink.Length - 1) + selection));
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync(ExtractMedia(links[int.Parse(reply.Message.Content)-1]));
                            Bot.cache.Add(sign + " | " + selection + " | listed" + int.Parse(reply.Message.Content), 
                                ExtractMedia(links[int.Parse(reply.Message.Content)-1]));
                        }
                    }
                }
            }
            }
        }
        
        private static bool IsDigitsOnly(string str) => str.All(c => c >= '0' && c <= '9');

        private static int? ContainsInt(IEnumerable<string> input)
        {
            foreach (var word in input)
            {
                if (IsDigitsOnly(word))
                {
                    return int.Parse(word);
                }
            }
            return null;
        }

        private static string ExtractMedia(string url)
        {
            _doc = Web.Load(url);

            if (_doc.DocumentNode.SelectSingleNode("//div[@class='videocontent']") == null) return null;
            var search = _doc.DocumentNode.CssSelect(".videocontent");
            var node = search.CssSelect("link").First();
            return "https://www.signingsavvy.com/" + node.GetAttributeValue("href");
        }

        private static string ExtractSecondUrl(string url)
        {
            _doc = Web.Load(url);
            var load = _doc;
            var search = load.DocumentNode.CssSelect(".signing_header");
            var node = search.CssSelect("a").Last();
            return node.GetAttributeValue("href");
        }

        private static List<string> ExtractSynon()
        {
            var search = _doc.DocumentNode.CssSelect(".desc");
            var nodes = search.CssSelect("a").ToArray();
            return (from node in nodes let shitArr = node.InnerText.ToLower().Split(" ") 
                where node.InnerText.ToLower() != "sign up now!" && shitArr[0] != "variation" && node.InnerText.ToLower() != "word lists" 
                select node.InnerText.Split("(") into kankerArr 
                select kankerArr[0].ToLower().Replace(" ", string.Empty)).ToList();
        }

        private static List<string> ExtractSigns()
        {
            if (_doc.DocumentNode.SelectSingleNode("//div[@class='.search_results']") != null)
            {
                return null;
            }
            var search = _doc.DocumentNode.CssSelect(".search_results");

            var enumerable = search as HtmlNode[] ?? search.ToArray();
            var htmlNodes = enumerable.CssSelect("a").ToArray();

            return htmlNodes.Length > 10 ? null : enumerable.CssSelect("a").Select(node => node.GetAttributeValue("href", "")).ToList();
        }

        private static List<string> Text(string url)
        {
            var search = Web.Load(url).DocumentNode.CssSelect(".search_results");
            return (from node in search.CssSelect("li") select node.CssSelect("em").First() into result 
                select result.InnerHtml.Split("&quot") into internalArr select internalArr[0] + internalArr[1] + internalArr[2]).ToList();
        }

        private static bool CheckUrlStatus(string website)
        {
            try
            {
                if (WebRequest.Create(website) is HttpWebRequest request)
                {
                    request.Method = "HEAD";
                    using var response = (HttpWebResponse) request.GetResponse();
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        private static void PeeLol() => Embed.ImageUrl = new Random().Next(0, 100) == 50 ? "https://cdn.discordapp.com/attachments/735641365320302593/767028760268111933/20200905232425_1.jpg" : "";
        
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
