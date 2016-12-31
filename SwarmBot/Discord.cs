using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Modules;
using Discord.Commands;
using SwarmBot.UI;
using SwarmBot.XML;
using System.Text.RegularExpressions;
using Trileans;

namespace SwarmBot
{
    class Discord
    {
        public static DiscordClient client;

        public static async void initializeDiscordClient()
        {
            Program.Log("Initializing SwarmBot Discord...");
            try
            {
                /*client.ExecuteAndWait(async () =>
                {
                    await Task.Run(() => client.Connect(Config.discordToken, TokenType.Bot));
                });*/
                await Task.Run(() => client.Connect(Config.discordToken, TokenType.Bot));
            }
            catch
            {
                Program.Log("Discord connection failed.");
            }
        }
        public static User getDiscordMemberByID(string ID, Server server)
        {
            return server.GetUser(ulong.Parse(ID.Replace("!", "")));
        }
        public static async Task help(DiscordCommandArgs e)
        {
            Program.Log("test");
            #region helptext
            await e.e.Channel.SendMessage(@"I am the SwarmBot created by @Mardan. View my source code: https://github.com/SpawnerSwarm/SwarmBot. I can:
--   Search the Warframe Wiki (!wfwiki <page name>)
--   Search the Spiral Knights Wiki (!skwiki <page name>)
--   Link you to the Guild Mail(!guildmail)
--   Send you the latest news (!news)
--   Update the news [Officer +] (!updateNews <News>)
--   Invite a group of players to play a game (!invite <Number of invitees> <Discord username 1>, [Discord username 2], [Discord username 3], [Discord username 4] <Game name>
--   Get a member's information (!getMember <@Member> [--verbose|-v])
--   Create a new member entry [Veteran +] (!createMember <@Member> [--date|-d 01/01/0001] [--steam-id|-s Steam ID] [--populate|-p Deprecated])
--   Promote a member [Officer +] (!promote <@Member> [--force|-f (Rank)] [--date|-d 01/01/0001] [--ignore-max-capacity|--ignore-capacity|-i])
--   Add donated forma to a member's account [Officer +] (!addForma <@Member> <1-9>
--   Price check a Warframe item using Nexus Stats (!pc <Item Name>)
--   Access the Swarm Emotes system (!e|!emotes)");
            #endregion
        }
        public static async Task wiki(DiscordCommandArgs e)
        {
            Match cmd = Regex.Match(e.e.Message.Text, "!([^ ]+) (.+)");
            if(cmd.Groups[1].Value == "wfwiki") { await e.e.Channel.SendMessage("http://warframe.wikia.com/wiki/" + cmd.Groups[2].Value.Replace(" ", "_")); }
            else if(cmd.Groups[1].Value == "skwiki") { await e.e.Channel.SendMessage("http://wiki.spiralknights.com/" + cmd.Groups[2].Value.Replace(" ", "_")); }
        }
        public static async Task getMember(DiscordCommandArgs e)
        {
            try
            {
                XMLDocument memberDB = new XMLDocument(Config.MemberDBPath);
                XMLMember member = memberDB.getMemberById(e.member.Id);
                trilean isReady = member.checkReadyForRankup();
                string message = "```xl\n";
                message += member.name + " is a(n) " + member.rank + "\n";
                if (isReady.value == TrileanValue.Tri)
                {
                    switch ((XMLErrorCode)isReady.embedded)
                    {
                        case XMLErrorCode.Old: message += "We can't tell whether or not " + member.name + " is ready for an upgrade, but they probably are since our data dates back before this bot's conception.\n"; break;
                        case XMLErrorCode.Maximum: message += "He/She has reached the maximum possible rank.\n"; break;
                    }
                }
                else if (isReady.value == TrileanValue.False)
                {
                    message += "He/she is not eligible for a rankup at this time.\n";
                    message += "It has been " + Regex.Match((string)isReady.embedded, @"(.+)\.(?:.+)?").Groups[1].Value + " days since their last rankup.\n";
                }
                else if (isReady.value == TrileanValue.True)
                {
                    message += "He/she is eligible for a rankup.\n";
                    message += "It has been " + Regex.Match((string)isReady.embedded, @"(.+)\.(?:.+)?").Groups[1].Value + " days since their last rankup.\n";
                }
                message += member.name + " has donated " + member.forma + " Forma\n";
                if (e.verbose)
                {
                    if (member.WFName != "" && member.WFName != "NaN") { message += "\nWarframe name is " + member.WFName; }
                    if (member.SKName != "" && member.SKName != "NaN") { message += "\nSpiral Knights name is " + member.SKName; }
                    if (member.steamName != "" && member.steamName != "NaN") { message += "\nSteam name is " + member.steamName; }
                }
                message += "```";
                await e.e.Channel.SendMessage(message);
            }
            catch(XMLException x)
            {
                switch(x.errorCode)
                {
                    case XMLErrorCode.MultipleFound:
                        await e.e.Channel.SendMessage("`An error occured: Multiple members were found`"); break;
                    case XMLErrorCode.NotFound:
                        await e.e.Channel.SendMessage("`An error occured: No members were found`"); break;
                    case XMLErrorCode.Unknown:
                        await e.e.Channel.SendMessage("`An unknown error occured`"); break;
                }
                Program.Log("An error occured while retreiving data for member " + e.member.Name + ", " + x.message + ": " + x.errorCode);
                return;
            }
        }
        public static async Task promote(DiscordCommandArgs e)
        {
            try
            {
                XMLDocument memberDB = new XMLDocument(Config.MemberDBPath);
                XMLMember member = memberDB.getMemberById(e.member.Id);
                XMLMember author = memberDB.getMemberById(e.e.User.Id);
                if (!author.checkPermissions(Rank.Officer)) { await e.e.Channel.SendMessage("```xl\nSorry, you don't have the permissions to do that\n```"); return; }
                if (e.e.User.Id == (ulong)member.discordId && !author.checkPermissions(Rank.GuildMaster)) { await e.e.Channel.SendMessage("```xl\nSorry, you can't promote yourself!\n```"); return; }
                if (member.rank == Rank.GuildMaster && e.force == "") { await e.e.Channel.SendMessage("```xl\nCan't promote " + member.name + " because they are already at maximum rank.\n```"); return; }

                Rank targetRank = e.force == "" ? (Rank)(member.rank + 1) : (Rank)e.force;
                if (targetRank >= author.rank && author.rank != Rank.GuildMaster) { await e.e.Channel.SendMessage("```xl\nCan't promote " + member.name + " because the destination rank is higher than your rank!\n```"); return; }

                trilean isRankMaxed;
                try
                {
                    isRankMaxed = memberDB.checkRankMaxed(targetRank);
                    if (isRankMaxed && (!e.ignore || !author.checkPermissions(Rank.GuildMaster))) { await e.e.Channel.SendMessage("```xl\nError: The Rank you have requested to promote to is currently at maximum capacity.\nPlease contact a Guild Master if you believe this is in error"); return; }

                    DateTime targetDate;
                    if (e.date == "") { targetDate = DateTime.Today; }
                    else
                    {
                        try { targetDate = DateTime.Parse(e.date); }
                        catch { await e.e.Channel.SendMessage("```xl\nError: Invalid Date. Must be in MM/DD/YYYY format.\n```"); return; }
                    }

                    try
                    {
                        trilean trilean = member.Promote(targetDate, targetRank);
                        await e.member.AddRoles(e.e.Server.FindRoles((Rank)trilean.embedded).First());
                        await e.e.Channel.SendMessage("```xl\nSuccessfully promoted " + member.name + " to " + trilean.embedded + "\n```");
                    }
                    catch (XMLException x)
                    {
                        switch (x.errorCode)
                        {
                            case XMLErrorCode.MultipleFound: await e.e.Channel.SendMessage("```xl\nError: Multiple Members found\n```"); break;
                            case XMLErrorCode.Maximum: await e.e.Channel.SendMessage("```xl\nCan't promote " + member.name + " because they are already at maximum rank.\n```"); break;
                            case XMLErrorCode.Greater: await e.e.Channel.SendMessage("```xl\nCan't promote " + member.name + " because the destination rank is higher than your rank!\n```"); break;
                            case XMLErrorCode.Unknown: await e.e.Channel.SendMessage("```xl\nAn error occured\n```"); break;
                        }
                    }
                }
                catch (Exception x)
                {
                    await e.e.Channel.SendMessage("An error occured: " + x.Message);
                    Program.Log("ERROR: " + author.name + " tried to promote " + member.name + " to " + targetRank + " but encountered an exception. " + x.Message);
                }
            }
            catch (XMLException x)
            {
                switch (x.errorCode)
                {
                    case XMLErrorCode.MultipleFound:
                        await e.e.Channel.SendMessage("`An error occured: Multiple members were found`"); break;
                    case XMLErrorCode.NotFound:
                        await e.e.Channel.SendMessage("`An error occured: No members were found`"); break;
                    case XMLErrorCode.Unknown:
                        await e.e.Channel.SendMessage("`An unknown error occured`"); break;
                }
                Program.Log("An error occured while promoting member " + e.member.Name + ", " + x.message + ": " + x.errorCode);
                return;
            }
        }

        public static async Task createMember(DiscordCommandArgs e)
        {
            try
            {
                XMLDocument memberDB = new XMLDocument(Config.MemberDBPath);
                XMLMember author = memberDB.getMemberById(e.e.User.Id);
                if (!author.checkPermissions(Rank.Veteran)) { await e.e.Channel.SendMessage("```xl\nSorry, you don't have the permissions to do that\n```"); return; }

                try { memberDB.getMemberById(e.member.Id); }
                catch (XMLException x)
                {
                    if (x.errorCode == XMLErrorCode.MultipleFound) { await e.e.Channel.SendMessage("`An error occured. Multiple members were found for the provided ID.`"); return; }
                    else if (x.errorCode != XMLErrorCode.NotFound) { await e.e.Channel.SendMessage("`An unexpected error occured. " + x.message + "`"); return; }
                }

                DateTime date;
                if (e.date != "") { try { date = DateTime.Parse(e.date); } catch { await e.e.Channel.SendMessage("Invalid Date"); return; } }
                else { date = DateTime.Today; }

                long steamId;
                if (e.steam != "") { try { steamId = Int64.Parse(e.steam); } catch { await e.e.Channel.SendMessage("Steam ID must be numeric"); return; } }
                else { steamId = 0; }

                Program.Log("Creating member " + e.member.Name + "...");
                trilean t = memberDB.createMember(e.member.Name, date, steamId, e.member.Id);
                if (t.value == TrileanValue.False) { await e.e.Channel.SendMessage("An unexpected error occured. Could not create member."); return; }
                else
                {
                    await e.e.Channel.SendMessage("Success! Created member " + e.member.Name + "!");
                    Program.Log("Successfully created member " + e.member.Name);
                }
                memberDB.Save(Config.MemberDBPath);
            }
            catch (XMLException x)
            {
                switch (x.errorCode)
                {
                    case XMLErrorCode.MultipleFound:
                        await e.e.Channel.SendMessage("`An error occured: Multiple members were found`"); break;
                    case XMLErrorCode.NotFound:
                        await e.e.Channel.SendMessage("`An error occured: No members were found`"); break;
                    case XMLErrorCode.Unknown:
                        await e.e.Channel.SendMessage("`An unknown error occured`"); break;
                }
                Program.Log("An error occured while creating member " + e.member.Name + ", " + x.message + ": " + x.errorCode);
                return;
            }
        }

        //Emotes
        public static async Task getEmote(DiscordCommandArgs e)
        {
            Emotes emotes = new Emotes(Config.EmoteDBPath);
            if(e.reference.StartsWith("list"))
            {
                short page;
                try { page = (e.reference == "list" ? (short)0 : Int16.Parse(e.reference.Replace("list", "").Replace("-", ""))); }
                catch(Exception x)
                {
                    Program.Log("Expected emote list page number. Received " + e.reference + " instead: " + x.Message);
                    await e.e.Channel.SendMessage("Error: Invalid page number");
                    return;
                }

                string message = "";
                try
                {
                    message += "Page " + (page == 0 ? 1 : page) + ". To move to the next page, use \"!e list " + (page == 0 ? 2 : page + 1) + "\". To view information about a specific emote, use \"!e list <emote_ref>\".\n";
                    List<Emote> eList = emotes.getBatchEmotes(page, 5);
                    foreach(Emote emote in eList)
                    {
                        message += "```xl\nName: " + emote.name.Replace('\'', 'ꞌ'); //Replaces the apostraphe with a Latin Small Letter Saltillo (U+A78C) so it won't break Discord formatting (as much).
                        message += "\nReference: " + emote.reference;
                        message += "\nRequired Rank: " + emote.requiredRank;
                        message += "\nCreator: " + emote.creator + "\n```\n";
                    }
                    if(!message.Contains("Name")) { message = "http://i.imgur.com/zdMAeE9.png"; }
                }
                catch(XMLException x)
                {
                    if(x.errorCode == XMLErrorCode.NotFound) { await e.e.Channel.SendMessage("http://i.imgur.com/zdMAeE9.png"); return; }
                }
                await e.e.Channel.SendMessage(message);
                return;
            }
            else
            {
                Emote emote;
                try { emote = emotes.getEmote(e.reference); }
                catch(XMLException x)
                {
                    switch(x.errorCode)
                    {
                        case XMLErrorCode.NotFound: await e.e.Channel.SendMessage("Error: Could not find requested emote."); Program.Log("Error: No emotes found for ref " + e.reference); break;
                        case XMLErrorCode.MultipleFound: await e.e.Channel.SendMessage("Error: Found multiple emotes."); Program.Log("Error: Multiple emotes found for ref " + e.reference); break;
                    }
                    return;
                }
                await e.e.Channel.SendMessage(emote.content);
                return;
            }
        }

        public static async Task createEmote(DiscordCommandArgs e)
        {
            XMLDocument memberDB = new XMLDocument(Config.MemberDBPath);
            if (!memberDB.getMemberById(e.e.User.Id).checkPermissions(Rank.Veteran)) { await e.e.Channel.SendMessage("```xl\nSorry, you don't have the permissions to do that\n```"); return; }

            Rank rank;
            try { rank = short.Parse(e.force); }
            catch { await e.e.Channel.SendMessage("Error: Invalid Rank"); return; }

            Emote emote = new Emote(e.name, e.reference, e.id, rank);
            Emotes emotes = new Emotes(Config.EmoteDBPath);
            try { emotes.addEmote(emote); }
            catch(XMLException x)
            {
                await e.e.Channel.SendMessage("`An error occurred while creating the emote. An emote with the same " + x.message + " already exists");
                Program.Log("Unable to create emote. An emote with the same " + x.message + " already exists");
                return;
            }
            await e.e.Channel.SendMessage("Successfully created emote " + emote.name + "!");
        }

        public static async Task tagMember(DiscordCommandArgs e)
        {
            if(e.force == "rank")
            {
                XMLDocument memberDB = new XMLDocument(Config.MemberDBPath);
                XMLMember member = memberDB.getMemberById(e.member.Id);
                
                for(int i = 1; i <= member.rank; i++)
                {
                    await e.member.AddRoles(e.e.Server.FindRoles(((Rank)i).ToString(), true).First());
                    await Task.Delay(300);
                }
                await e.e.Channel.SendMessage("Successfully gave you the rank tags!");
                Program.Log("Added rank tags through " + member.rank.ToString() + " to member " + member.name);
                return;
            }
            switch(e.force)
            {
                case "overwatch": await e.member.AddRoles(e.e.Server.FindRoles("Overwatch", true).First()); break;
                case "warframe": await e.member.AddRoles(e.e.Server.FindRoles("Warframe", true).First()); break;
                case "sk": await e.member.AddRoles(e.e.Server.FindRoles("Spiral Knights", true).First()); break;
                case "bot": await e.member.AddRoles(e.e.Server.FindRoles("Bot Notifications", true).First()); break;
            }
            await e.e.Channel.SendMessage("Successfully gave you the " + e.force + " tag!");
            Program.Log("Added " + e.force + " tag to member " + e.member.Name);
            return;
        }

        public static async Task untagMember(DiscordCommandArgs e)
        {
            switch (e.force)
            {
                case "overwatch": await e.member.RemoveRoles(e.e.Server.FindRoles("Overwatch", true).First()); break;
                case "warframe": await e.member.AddRoles(e.e.Server.FindRoles("Warframe", true).First()); break;
                case "sk": await e.member.AddRoles(e.e.Server.FindRoles("Spiral Knights", true).First()); break;
                case "bot": await e.member.AddRoles(e.e.Server.FindRoles("Bot Notifications", true).First()); break;
            }
            await e.e.Channel.SendMessage("Successfully removed the " + e.force + " tag!");
            Program.Log("Removed " + e.force + " tag from member " + e.member.Name);
            return;
        }

        public static async Task getMemberCount(DiscordCommandArgs e)
        {
            XMLDocument memberDB = new XMLDocument(Config.MemberDBPath);
            string message = "```xl\n";
            for (short i = 1; i <= 7; i++)
            {
                Rank rank = i;
                int memberCount = memberDB.document.Element("Database").Elements("Member").Where(x => x.Element("Rank").Value == rank.ToString()).Count();
                message += rank.ToString() + ": " + memberCount + " out of " + memberDB.getDefine(rank.ToString(), DefineType.RankCapacity) + "\n";
            }
            message += "\n```";
            await e.e.Channel.SendMessage(message);
        }
    }

    public class DiscordCommandArgs
    {
        public MessageEventArgs e;
        public User member;
        public string name;
        public string steam;
        public string discord;
        public string id;
        public string reference;
        public bool verbose;
        public string date;
        public string force;
        public bool ignore;

        /*public DiscordCommandArgs(MessageEventArgs e, User member = null, string name = null, string steam = null, string discord = null, string id = null, string reference = null, bool verbose = false, string date = null, string force = null, bool isForce = false, bool ignore = false)
        {
            this.e = e;
            this.member = member;
            this.name = name;
            this.steam = steam;
            this.discord = discord;
            this.id = id;
            this.reference = reference;
            this.verbose = verbose;
            this.date = date;
            this.force = force;
            this.isForce = isForce;
            this.ignore = ignore;
        }*/
    }
}
