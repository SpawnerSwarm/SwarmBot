using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Diagnostics;
using Discord.WebSocket;
using SwarmBot.XML;
using Trileans;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SwarmBot.Warframe
{
    class WarframeModule : ModuleBase
    {
        [Command("warframe"), Alias("wf"), Summary(""), RequireContext(ContextType.Guild)]
        public async Task warframe()
        {
            string initialCommandUsed = Context.Message.Content.Split(' ')[0];

            CommandInfoDB info = new CommandInfoDB(Config.CommandInfoDBPath);
            string message = ((DBSubModule)info.getCommandByName("warframe")).getDescriptionForCommandUsed(initialCommandUsed);

            await ReplyAsync(message);
        }
        
    }

    public class WorldState
    {
        private static string runCommand(string arg)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c node {Config.wfJSPath} {Config.wfWorldStateURL} {arg}"; // Note the /c command (*)
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            //* Read the output (or the error)
            string output = process.StandardOutput.ReadToEnd();
            Console.WriteLine(output);
            string err = process.StandardError.ReadToEnd();
            Console.WriteLine(err);
            process.WaitForExit();

            if (err != null) { Program.Log(err); }

            return output;
        }

        public static List<Alert> Alerts
        {
            get
            {
                string data = runCommand("alerts");
                Program.Log(data);
                JArray json = JArray.Parse(data);
                List<Alert> alerts = new List<Alert>();
                foreach (JObject obj in json)
                {
                    alerts.Add(JsonConvert.DeserializeObject<Alert>(obj.ToString()));
                }
                return alerts;
            }
        }
    }

    public enum Faction
    {
        Grineer,
        Corpus,
        Infested,
        Corrupted,
        Orokin
    }

    public class Alert
    {
        public string id;
        public string activation;
        public string expiry;
        public Mission mission;

        public class AlertRewards
        {
            public string[] items;
            public CountedItem[] countedItems;
            public int credits;

            public class CountedItem
            {
                public int count;
                public string type;
            }
        }
    }

    public class Mission
    {
        public string node;
        private string type;
        private string faction;
        public Alert.AlertRewards reward;
        public short minEnemyLevel;
        public short maxEnemyLevel;
        public short maxWaveNum;
        public bool nightmare;
        public bool archwingRequired;
        
        public Type missionType
        {
            get
            {
                Type type;
                Enum.TryParse<Type>(this.type, out type);
                return type;
            }
        }
        public Faction missionFaction
        {
            get
            {
                Faction faction;
                Enum.TryParse<Faction>(this.faction, out faction);
                return faction;
            }
        }

        public enum Type
        {
            Excavation,
            Sabotage,
            MobileDefense,
            Assassination,
            Extermination,
            Hive,
            Defense,
            Interception,
            Rathuum,
            Conclave,
            Rescue,
            Spy,
            Survival,
            Capture,
            DarkSector,
            Hijack,
            Assault
        }
    }
}
