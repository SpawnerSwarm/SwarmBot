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

namespace SwarmBot.Modules
{
    class TaggingModule : ModuleBase
    {
        [Command("tagme"), Alias("tag"), Summary("Applies a discord notification tag for a specific group"), RequireContext(ContextType.Guild)]
        public async Task tagme([Summary("The tag to be applied")] string tag)
        {
            if (!Regex.IsMatch(tag, "overwatch|warframe|sk|rank|bot", RegexOptions.IgnoreCase)) { await ReplyAsync("Error: Argument invalid. Correct format is \"!tagme (overwatch/warframe/sk/bot/rank)\""); return; }
            tag = tag.ToLower();
            DiscordCommandArgs e = new DiscordCommandArgs
            {
                e = Context.Message,
                member = Context.User,
                force = tag
            };

            if (e.force == "rank")
            {
                XMLDocument memberDB = new XMLDocument(Config.MemberDBPath);
                XMLMember member = memberDB.getMemberById(e.member.Id);

                for (int i = 1; i <= member.rank; i++)
                {
                    //await (e.member as IGuildUser).AddRolesAsync(((e.e.Channel as IGuildChannel)?.Guild as IGuild).Roles.Where(x => x.Name == ((Rank)i).ToString()).First());
                    await Discord.applyRoleToMember(e.member, e.e, (Rank)i);
                    await Task.Delay(300);
                }
                await e.e.Channel.SendMessageAsync("Successfully gave you the rank tags!");
                Program.Log("Added rank tags through " + member.rank.ToString() + " to member " + member.name);
                return;
            }
            switch (e.force)
            {
                case "overwatch": await Discord.applyRoleToMember(e.member, e.e, "Overwatch"); break;
                case "warframe": await Discord.applyRoleToMember(e.member, e.e, "Warframe"); break;
                case "sk": await Discord.applyRoleToMember(e.member, e.e, "Spiral Knights"); break;
                case "bot": await Discord.applyRoleToMember(e.member, e.e, "Bot Notifications"); break;
            }
            await e.e.Channel.SendMessageAsync("Successfully gave you the " + e.force + " tag!");
            Program.Log("Added " + e.force + " tag to member " + e.member.Username);
            return;
        }

        [Command("tagme"), Alias("tag"), Summary("Applies a discord notification tag for a specific group"), RequireContext(ContextType.Guild)]
        public async Task tagmeHelpText()
        {
            await ReplyAsync("Error: Argument blank. Correct format is \"!tagme (overwatch/warframe/sk/bot/rank)\"");
        }

        [Command("untagme"), Alias("untag"), Summary("Removes a discord notification tag for a specific group"), RequireContext(ContextType.Guild)]
        public async Task untagme([Summary("The tag to be removed")] string tag)
        {
            if (!Regex.IsMatch(tag, "overwatch|warframe|sk|bot", RegexOptions.IgnoreCase)) { await ReplyAsync("Error: Argument invalid. Correct format is \"!untagme (overwatch/warframe/sk/bot)\""); return; }
            tag = tag.ToLower();
            DiscordCommandArgs e = new DiscordCommandArgs
            {
                e = Context.Message,
                member = Context.User,
                force = tag
            };

            switch (e.force)
            {
                case "overwatch": await Discord.removeRoleFromMember(e.member, e.e, "Overwatch"); break;
                case "warframe": await Discord.removeRoleFromMember(e.member, e.e, "Warframe"); break;
                case "sk": await Discord.removeRoleFromMember(e.member, e.e, "Spiral Knights"); break;
                case "bot": await Discord.removeRoleFromMember(e.member, e.e, "Bot Notifications"); break;
            }
            await e.e.Channel.SendMessageAsync("Successfully removed the " + e.force + " tag!");
            Program.Log("Removed " + e.force + " tag from member " + e.member.Username);
            return;
        }

        [Command("untagme"), Alias("untag"), Summary("Removes a discord notification tag for a specific group"), RequireContext(ContextType.Guild)]
        public async Task untagmeHelpText()
        {
            await ReplyAsync("Error: Argument blank. Correct format is \"!untagme (overwatch/warframe/sk/bot)\"");
        }
    }
}
