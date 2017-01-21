using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SwarmBot.XML;
using Trileans;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;


namespace SwarmBot.Modules
{
    class NexusStatsModule : ModuleBase
    {
        [Command("pricecheck"), Alias("pc"), Summary("Check the price of a warframe item")]
        public async Task priceCheck(string id)
        {
            DiscordCommandArgs e = new DiscordCommandArgs
            {
                e = Context.Message
            };

            trilean t = await NexusStats.getItemById(id);
            if (t == true)
            {
                Item i = (Item)t.embedded;
                //e.Channel.SendMessage(i.Title);
                string message = "```xl\n";
                Component[] set = i.Components.Where(x => x.name == "Set").ToArray();
                if (set.Length > 0)
                {
                    message += "Average complete set price is " + set[0].avg + "\n";
                }
                foreach (Component component in i.Components)
                {
                    if (component.name != "Set")
                    {
                        message += "\t" + component.name + " average price is " + component.avg + "\n";
                    }
                }
                message += "Supply is at " + i.SupDem[0] + "%\n";
                message += "Demand is at " + i.SupDem[1] + "%\n";
                message += "\n```\nThis feature is in Beta. Stats provided by https://nexus-stats.com.";
                await e.e.Channel.SendMessageAsync(message);
            }
            else
            {
                await e.e.Channel.SendMessageAsync("Sorry, could not find the item you requested.");
            }
        }
    }

    public class NexusStats
    {
        private static string url
        {
            get
            {
                using (StreamReader sr = File.OpenText(Path.Combine(Config.AppDataPath, "nexus.txt")))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public static async Task<trilean> getItemById(string id)
        {
            WebClient client = new WebClient();
            Stream stream = await client.OpenReadTaskAsync(url);
            StreamReader reader = new StreamReader(stream);
            string content = await reader.ReadToEndAsync();
            JArray json = JArray.Parse(content);
            List<JToken> items = json.Where(x => Regex.IsMatch(x["Title"].ToString(), @"\A" + id + @"\Z", RegexOptions.IgnoreCase) || Regex.IsMatch(x["id"].ToString(), @"\A" + id + @"\Z", RegexOptions.IgnoreCase)).ToList();
            if (items.Count == 1)
            {
                return new trilean(true, JsonConvert.DeserializeObject<Item>(items[0].ToString()));
            }
            else if (items.Count < 1)
            {
                return new trilean(false, NexusErrorCode.NotFound);
            }
            else
            {
                return new trilean(false, NexusErrorCode.MultipleFound);
            }
        }
    }

    enum NexusErrorCode
    {
        NotFound = 404,
        MultipleFound = 405
    }

    public class Item
    {
        public string _id;
        public string Title;
        public string Type;
        public short[] SupDem;
        public short[] SupDemNum;
        public Component[] Components;
    }
    public class Component
    {
        public string name;
        public string avg;
        public string comp_val_rt;
        public bool visible;
    }
}
