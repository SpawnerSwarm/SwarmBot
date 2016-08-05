using System;
using System.IO;
using System.Text.RegularExpressions;
using SwarmBot.MongoDB;
using Newtonsoft.Json;
using Trileans;

namespace SwarmBot.Nexus
{
    public class NexusStats
    {
        public static string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SwarmBot\\");
        private string address, database, collection;
        internal Mongo mongoClient;
        internal MDatabase itemDatabase;

        public void Connect()
        {
            using (StreamReader sr = File.OpenText(Path.Combine(configDir, "nexus.txt")))
            {
                Match info = Regex.Match(sr.ReadToEnd(), @"(.+);(.+);(.+).+");
                address = info.Groups[1].Value;
                database = info.Groups[2].Value;
                collection = info.Groups[3].Value;
                Console.Write(sr.ReadToEnd());
            }
            mongoClient = new Mongo(address);
        }

        public trilean getItemById(string id)
        {
            itemDatabase = mongoClient.GetDatabase(database, new string[] { collection });
            trilean t = itemDatabase.getBsonDocumentByPropertyEq("_id", id, itemDatabase.collections[0]);
            if(t.value == 0)
            {
                return new trilean(true, JsonConvert.DeserializeObject<Item>((string)t.embedded));
            } else
            {
                t = itemDatabase.getBsonDocumentByPropertyEq("Title", id, itemDatabase.collections[0]);
                if(t.value == 0)
                {
                    return new trilean(true, JsonConvert.DeserializeObject<Item>((string)t.embedded));
                } else
                {
                    return false;
                }
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
