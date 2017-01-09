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
            string credits = db.getCreditTrackingForMember(Context.User.Id).GetAwaiter().GetResult()?.Value;
            try { keywordList = await db.getKeywordsForMember(Context.User.Id); } catch {
                message = $"No existing registration detected. Welcome to the SwarmBot Warframe Alerts system. ";
            };
            if(keywordList?.Count() != 0 || credits != null)
            {
                message += $"Detected existing tracking for {Context.User.Username}. You are currently tracking:\n\n";
                foreach(Keyword keyword in keywordList)
                {
                    message += $"\t- **{keyword.key}**\n";
                }
                if (credits != null)
                {
                    message += $"\t- Credit amounts over **{credits}**\n";
                }
                message += '\n';
            } else if(await db.memberExists(Context.User.Id))
            {
                message += $"Detected existing tracking for {Context.User.Username}. You are currently not tracking any keywords. ";
            }
            message += "Available commands are:\n";
            message += $"\t-** {initialCommandUsed}** -- Display this text or check your current notification status\n";
            message += $"\t-** {initialCommandUsed} track/add/new/follow <keywords>** -- Add a keyword to track\n";
                message += $"\t\t-** {initialCommandUsed} track/add/new/follow credits <amount>** -- Track alerts with credit amounts over a number\n";
            message += $"\t-** {initialCommandUsed} untrack/remove/unfollow <keywords>** -- Remove a keyword from tracking\n";
                message += $"\t\t-** {initialCommandUsed} untrack/remove/unfollow credits** -- Stop tracking alerts based on credit amounts\n";

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
                Keyword keyword = await db.getOrCreateKeyword(param.Replace(",", ""));
                if (db.getKeywordsForMember(Context.User.Id).GetAwaiter().GetResult().Where(x => Regex.IsMatch(param, x.key, RegexOptions.IgnoreCase)).Count() != 0) { message += "**Error:** you are already following that keyword\n"; }
                else
                {
                    await keyword.addMemberToTracking(Context.User, db);
                    await db.addKeywordToMember(Context.User.Id, keyword);
                    message += $"You are now following {param}\n";
                }
            }
            await ReplyAsync(message);
        }

        [Command("track credits"), Alias("add credits", "new credits", "follow credits")]
        public async Task trackCredits(int creditAmount)
        {
            AlertsDB db = new AlertsDB(Config.AlertsDBPath);
            string message = "";
            await db.createMemberIfNull(Context.User);
            string currentCreditAmount = db.getCreditTrackingForMember(Context.User.Id).GetAwaiter().GetResult()?.Value;

            if (currentCreditAmount == null || int.Parse(currentCreditAmount) < creditAmount)
            {
                await db.addCreditTrackingForMember(Context.User, creditAmount);
                message += $"You are now following credit amounts over {creditAmount}\n";
            }
            else
            {
                message += $"**Error:** You are already following credit amounts over {creditAmount}\n";
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
                Keyword keyword = await db.getKeyword(param.Replace(",", ""));
                if (db.getKeywordsForMember(Context.User.Id).GetAwaiter().GetResult().Where(x => Regex.IsMatch(param, x.key, RegexOptions.IgnoreCase)).Count() == 0) { message += "**Error:** you are not following that keyword\n"; }
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

        [Command("untrack credits"), Alias("remove credits", "unfollow credits")]
        public async Task untrackCredits()
        {
            AlertsDB db = new AlertsDB(Config.AlertsDBPath);
            string message = "";
            await db.createMemberIfNull(Context.User);
            string currentCreditAmount = db.getCreditTrackingForMember(Context.User.Id).GetAwaiter().GetResult()?.Value;

            if (currentCreditAmount != null)
            {
                await db.removeCreditTrackingFromMember(Context.User.Id);
                message += $"You are no longer following credit amounts\n";
            }
            else
            {
                message += $"**Error:** You are not following a credit amount\n";
            }
            await ReplyAsync(message);
        }
    }
}
