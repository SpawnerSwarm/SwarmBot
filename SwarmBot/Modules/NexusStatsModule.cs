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
        public async Task priceCheck(string id, [Remainder] string moreId = "")
        {
            if (moreId != "") { id += $" {moreId}"; }
            DiscordCommandArgs e = new DiscordCommandArgs
            {
                e = Context.Message
            };

            trilean t = await NexusStats.getItemById(id);
            if (t == true)
            {
                Item i = (Item)t.embedded;

                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle(i.Title)
                    .WithDescription($"Price Check for search term {id}")
                    .WithUrl($"https://nexus-stats.com/{i.Type}/{i.id.Replace(" ", "%20")}")
                    .WithThumbnailUrl($"https://nexus-stats.com/img/items/{i.Title.Replace(" ", "%20")}-min.png");

                if (i.Type == "Mods" || i.Type == "Arcane")
                {
                    builder.AddField(x =>
                    {
                        x.Name = "Single Unit";
                        x.Value = i.Component("Single Unit").getAverage();
                        x.Value += $"\n\nDemand: {i.SupDem[1]}%, Supply: {i.SupDem[0]}%";
                    });
                }
                else if (i.Type == "Prime")
                {
                    foreach(Component component in i.Components.Where(x => x.name != "Set"))
                    {
                        builder.AddField(x =>
                        {
                            x.Name = component.name;
                            x.Value = component.getAverage();
                            x.IsInline = true;
                        });
                    }
                    builder.AddField(x =>
                    {
                        x.Name = "Set";
                        x.Value = i.Component("Set").getAverage();
                        x.Value += $"\n\nDemand: {i.SupDem[1]}%, Supply: {i.SupDem[0]}%";
                        x.IsInline = true;
                    });
                }

                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    .WithIconUrl("https://nexus-stats.com/img/logo.png")
                    .WithText("NexusStats - https://nexus-stats.com");
                builder.WithFooter(footer);

                await ReplyAsync("", embed: builder.Build());
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
        public string id;
        public string Title;
        public string Type;
        public short[] SupDem;
        public short[] SupDemNum;
        public Component[] Components;

        public Component Component(string name)
        {
            List<Component> components = Components.Where(x => x.name == name).ToList();
            if(components.Count == 1)
            {
                return components.First();
            }
            else
            {
                return new ComponentBuilder()
                    .WithName("No Data")
                    .WithAvg("No Data");
            }
        }
    }
    public class Component
    {
        public string name;
        public string avg;
        public string comp_val_rt;
        public bool visible;

        public string getAverage()
        {
            return (this.avg == "" ? "No Data" : avg);
        }
    }
    public class ComponentBuilder : Component
    {
        public ComponentBuilder WithName(string name)
        {
            this.name = name;
            return this;
        }
        public ComponentBuilder WithAvg(string avg)
        {
            this.avg = avg;
            return this;
        }
        public ComponentBuilder WithComp_Val_Rt(string comp_val_rt)
        {
            this.comp_val_rt = comp_val_rt;
            return this;
        }
        public ComponentBuilder WithVisible(bool visible)
        {
            this.visible = visible;
            return this;
        }
        public Component Build()
        {
            return (Component)this;
        }
    }
}
