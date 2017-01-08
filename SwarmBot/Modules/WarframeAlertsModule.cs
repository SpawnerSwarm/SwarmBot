using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SwarmBot.XML;
using System.Xml.Linq;
using Trileans;
using System.Text.RegularExpressions;

namespace SwarmBot.Warframe
{
    [Group("warframealerts"), Alias("wfalerts", "alerts", "wf")]
    public partial class WarframeAlertsModule : ModuleBase
    {
        [Command()]
        public async Task Default()
        {
            AlertsDB db = new AlertsDB(Config.AlertsDBPath);

            string initialCommandUsed = Context.Message.Content.Split(' ')[0];
            string message = "";
            List<Keyword> keywordList = new List<Keyword>();
            try { keywordList = await db.getKeywordsForMember(Context.User.Id); } catch {
                message = $"No existing registration detected. Welcome to the SwarmBot Warframe Alerts system. ";
            };
            if(keywordList?.Count() != 0)
            {
                message += $"Detected existing tracking for {Context.User.Username}. You are currently tracking:\n\n";
                foreach(Keyword keyword in keywordList)
                {
                    message += $"\t- **{keyword.key}**\n";
                }
                message += '\n';
            } else if(await db.memberExists(Context.User.Id))
            {
                message += $"Detected existing tracking for {Context.User.Username}. You are currently not tracking any keywords. ";
            }
            message += "Available commands are:\n";
            message += $"\t-** {initialCommandUsed}** -- Display this text or check your current notification status\n";
            message += $"\t-** {initialCommandUsed} track/add/new/follow** (Keywords to add tracking for, separated by spaces) -- Add a keyword to track\n";
            message += $"\t-** {initialCommandUsed} untrack/remove/unfollow** (Keywords to remove tracking from, separated by spaces) -- Remove a keyword from tracking\n";

            await ReplyAsync(message);
        }

        [Command("track"), Alias("add", "new", "follow")]
        public async Task track(params string[] keywords)
        {
            AlertsDB db = new AlertsDB(Config.AlertsDBPath);
            string message = "";
            await db.createMemberIfNull(Context.User);

            foreach(string param in keywords)
            {
                Keyword keyword = await db.getOrCreateKeyword(param);
                if (db.getKeywordsForMember(Context.User.Id).GetAwaiter().GetResult().Where(x => Regex.IsMatch(x.key, param, RegexOptions.IgnoreCase)).Count() != 0) { message += "**Error:** you are already following that keyword\n"; }
                else
                {
                    await keyword.addMemberToTracking(Context.User, db);
                    await db.addKeywordToMember(Context.User.Id, keyword);
                    message += $"You are now following {param}\n";
                }
            }
            await ReplyAsync(message);
        }

        [Command("untrack"), Alias("remove", "unfollow")]
        public async Task untrack(params string[] keywords)
        {
            AlertsDB db = new AlertsDB(Config.AlertsDBPath);
            string message = "";
            await db.createMemberIfNull(Context.User);

            foreach (string param in keywords)
            {
                Keyword keyword = await db.getKeyword(param);
                if (db.getKeywordsForMember(Context.User.Id).GetAwaiter().GetResult().Where(x => x.key == param).Count() == 0) { message += "**Error:** you are not following that keyword\n"; }
                else
                {
                    await keyword.removeMemberFromTracking(Context.User, db);
                    await db.removeKeywordFromMember(Context.User.Id, keyword);
                    await db.removeKeywordIfEmpty(keyword);
                    message += $"You are no longer tracking {param}\n";
                }
            }
            await ReplyAsync(message);
        }
    }
}
