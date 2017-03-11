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
            string err = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (err != "") { Program.Log(err); }

            return output;
        }

        public static List<Alert> Alerts
        {
            get
            {
                string data = runCommand("alerts");
                JArray json = JArray.Parse(data);
                List<Alert> alerts = new List<Alert>();
                foreach (JObject obj in json)
                {
                    alerts.Add(JsonConvert.DeserializeObject<Alert>(obj.ToString()));
                }
                alerts.Sort((x, y) => TimeSpan.Compare(x.remainingTime, y.remainingTime));
                return alerts;
            }
        }
        //Don't ask me why the fissures don't use the Mission format
        public static List<Fissure> Fissures
        {
            get
            {
                string data = runCommand("fissures");
                JArray json = JArray.Parse(data);
                List<Fissure> fissures = new List<Fissure>();
                foreach (JObject obj in json)
                {
                    fissures.Add(JsonConvert.DeserializeObject<Fissure>(obj.ToString()));
                }
                fissures = fissures.OrderBy(x => x.tierNum).ToList();
                return fissures;
            }
        }
    }

    public class WorldStateEvent
    {
        public string activation;
        public string expiry;

        private static readonly TimeSpan __defTime;

        private TimeSpan _remainingTime;
        public TimeSpan remainingTime
        {
            get
            {
                if (_remainingTime != __defTime) { return _remainingTime; }
                else
                {
                    TimeSpan rem = DateTime.Parse(expiry.Split('.').First()) - DateTime.UtcNow;
                    _remainingTime = rem;
                    return rem;
                }
            }
        }
    }

    public class Alert : WorldStateEvent
    {
        public string id;
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

    public class Fissure : WorldStateEvent
    {
        public string id;
        public string node;
        public string missionType;
        public string enemy;
        public string tier;
        public short tierNum;
    }

    public class Mission
    {
        public string node;
        public string type;
        public string faction;
        public Alert.AlertRewards reward;
        public short minEnemyLevel;
        public short maxEnemyLevel;
        public short maxWaveNum;
        public bool nightmare;
        public bool archwingRequired;
    }
}
