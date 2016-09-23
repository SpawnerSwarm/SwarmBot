using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Trileans;

namespace SwarmBot.Chat
{
    public class NexusStats
    {
		public static string configDir = File.ReadAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Documents/Projects/SwarmBot/SwarmBot/bin/Release/path.txt"))[0];
        private string url;

        public void Connect()
        {
            using (StreamReader sr = File.OpenText(Path.Combine(configDir, "nexus.txt")))
            {
                Match info = Regex.Match(sr.ReadToEnd(), @"(.+);(.+);(.+);(.+).+");
                url = info.Groups[4].Value;
                Console.Write(sr.ReadToEnd());
            }
        }

        public trilean getItemById(string id)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead("http://nexus-stats.com/api");
            StreamReader reader = new StreamReader(stream);
            string content = reader.ReadToEnd();
            JArray json = JArray.Parse(content);
            List<JToken> items = json.Where(x => Regex.IsMatch(x["Title"].ToString(), @"\A" + id + @"\Z", RegexOptions.IgnoreCase) || Regex.IsMatch(x["id"].ToString(), @"\A" + id + @"\Z", RegexOptions.IgnoreCase)).ToList();
            if(items.Count == 1)
            {
                return new trilean(true, JsonConvert.DeserializeObject<Item>(items[0].ToString()));
            } else if (items.Count < 1)
            {
                return new trilean(false, NexusErrorCode.NotFound);
            } else
            {
                return new trilean(false, NexusErrorCode.MultipleFound);
            }
        }
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
