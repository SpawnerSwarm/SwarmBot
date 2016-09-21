﻿//using DiscordSharp;
//using DiscordSharp.Objects;
using DiscordSharp.Events;
using Discord;
using Discord.API;
using Discord.Commands;
using Discord.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.IO;
using Trileans;
using SwarmBot.XML;
using SwarmBot.Chat;

namespace SwarmBot
{
    class Discord
    {
        public static string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SwarmBot\\");
        public static DiscordClient client/* = new DiscordClient(token, true)*/;
        private static string email;
        private static string password;
        private static string token;
        private static ulong archiveServerID;
        public static Server Swarm;
        public static string memberDBPath = Path.Combine(configDir + "PersonellDB.xml");

        public static bool SendInvite(string recipient, string Game, MessageEventArgs e)
        {
            //SendInvite(firstMatch, fifthMatch, firstMatch.StartsWith("<@"), e);

            bool isTag = recipient.StartsWith("<@");

            if (isTag)
            {
                //e.Server.GetMemberByKey(recipient.Replace("<", "").Replace("@", "").Replace(">", "")).SendMessage("@" + e.User.Name + " has invited you to play: " + Game);
                getDiscordMemberByID(recipient.Replace("<", "").Replace("@", "").Replace(">", ""), e.Server).SendMessage("@" + e.User.Name + " has invited you to play: " + Game);
            }
            else {
                e.Server.FindUsers(recipient, true).ToArray()[0].SendMessage("@" + e.User.Name + " has invited you to play: " + Game);
            }

            return true;
        }

        public static User getDiscordMemberByID(string ID, Server server)
        {
            return server.GetUser(ulong.Parse(ID.Replace("!", "")));
        }

        public static void Connect()
        {
            using (StreamReader sr = File.OpenText(Path.Combine(configDir, "config.txt")))
            {
                Match info = Regex.Match(sr.ReadToEnd(), @"(.+);(.+);(.+);(.+);.+");
                email = info.Groups[1].Value;
                password = info.Groups[2].Value;
                token = "Bot " + info.Groups[3].Value;
                archiveServerID = ulong.Parse(info.Groups[4].Value);
            }

            //client = new DiscordClient();

            /*client.ClientPrivateInformation.Email = email;
            client.ClientPrivateInformation.Password = password;*/

            try
            {
                //client.Autoconnect = true;
                //client.SendLoginRequest();
                client.ExecuteAndWait(async () =>
                {
                    await client.Connect(token);
                });
                //client.Connect(token);

            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong!\n" + e.Message + "\nPress any key to close this window.");
            }
        }
        public static void help(MessageEventArgs e)
        {
            e.Channel.SendMessage(@"I am the SwarmBot created by @Mardan. View my source code: https://github.com/SpawnerSwarm/SwarmBot. I can:

--   Search the Warframe Wiki (!wfwiki <page name>)

--   Search the Spiral Knights Wiki (!skwiki <page name>)

--   Link you to the Guild Mail(!guildmail)

--   Send you the latest news (!news)

--   Update the news [Officer +] (!updateNews <News>)

--   Invite a group of players to play a game (!invite <Number of invitees> <Discord username 1>, [Discord username 2], [Discord username 3], [Discord username 4] <Game name>

--   Get a member's information (!getMember <@Member> [--verbose|-v])

--   Create a new member entry [Veteran +] (!createMember <@Member> [--date|-d 01/01/0001] [--steam|-s Steam Name] [--populate|-p Deprecated])

--   Promote a member [Officer +] (!promote <@Member> [--force|-f (Rank)] [--date|-d 01/01/0001] [--ignore-max-capacity|--ignore-capacity|-i])

--   Add donated forma to a member's account [Officer +] (!addForma <@Member> <1-9>

--   Price check a Warframe item using Nexus Stats (!pc <Item Name>)

--   Access the Swarm Emotes system (!e|!emotes)");
            Console.WriteLine("Sent help because of this message: " + e.Message.Text);
        }
        public static void wiki(MessageEventArgs e, string wiki)
        {
            string target = null;
            if (wiki == "wf")
            {
                target = Regex.Match(e.Message.RawText,
                    @"!wfwiki (.+)",
                    RegexOptions.Singleline)
                .Groups[1].Value;
                e.Channel.SendMessage("<@" + e.User.Id + "> http://warframe.wikia.com/wiki/" + target.Replace(" ", "_"));
            }
            else if(wiki == "sk")
            {
                target = Regex.Match(e.Message.RawText,
                    @"!skwiki (.+)",
                    RegexOptions.Singleline)
                .Groups[1].Value;
                e.Channel.SendMessage("<@" + e.User.Id + "> http://wiki.spiralknights.com/" + target.Replace(" ", "_"));
            }
            Console.WriteLine("Sent " + target.Replace(" ", "_") + " to " + e.User.Id + " because of this message: " + e.Message.Text);
        }
        public static void invite(MessageEventArgs e, MatchCollection cmd, string inviteSubject, string[] inviteTargetKeys)
        {
            foreach(string inviteTargetKey in inviteTargetKeys)
            {
                getDiscordMemberByID(inviteTargetKey, e.Server).SendMessage(e.User.Name + " has invited you to play " + inviteSubject);
            }            
        }
        public static void getMember(User member, MessageEventArgs e, bool verbose)
        {
            try
            {
                XMLDocument memberDB = new XMLDocument(memberDBPath);
                XMLMember xMember = memberDB.getMemberById(member.Id);
                Console.WriteLine(xMember.name + " is a(n) " + xMember.rank);
                trilean isReady = xMember.checkReadyForRankUp();
                string message = "```xl\n";
                message += xMember.name + " is a(n) " + xMember.rank + "\n";
                if (isReady == 1)
                {
                    if ((XMLErrorCode)isReady.embedded == XMLErrorCode.Old)
                    {
                        message += "We can't tell whether or not " + xMember.name + " is ready for an upgrade, but they probably are since our data dates back before this bot's conception.\n";
                    } else if((XMLErrorCode)isReady.embedded == XMLErrorCode.Maximum)
                    {
                        message += "He/She has reached the maximum possible rank.\n";
                    }
                }
                else if(isReady == 2)
                {
                    /*if (isReady.table[0])
                    {
                        message += "He/she is eligible for a rankup.\n";
                    }
                    else if (!isReady.table[0] && (XMLErrorCode)isReady.embedded != XMLErrorCode.Maximum)
                    {
                        message += "He/she is not eligible for a rankup at this time.\n";
                    }
                    else if ((XMLErrorCode)isReady.embedded == XMLErrorCode.Maximum)
                    {
                        message += "He/She has reached the maximum possible rank.\n";
                    }
                    if ((XMLErrorCode)isReady.embedded != XMLErrorCode.Maximum)
                    {
                        message += "It has been " + Regex.Match((string)isReady.embedded, @"(.+)\.(?:.+)?").Groups[1].Value + " days since their last rankup.\n";
                    }*/
                    message += "He/she is not eligible for a rankup at this time.\n";
                    message += "It has been " + Regex.Match((string)isReady.embedded, @"(.+)\.(?:.+)?").Groups[1].Value + " days since their last rankup.\n";
                }
                else if(isReady == 0)
                {
                    message += "He/she is eligible for a rankup.\n";
                    message += "It has been " + Regex.Match((string)isReady.embedded, @"(.+)\.(?:.+)?").Groups[1].Value + " days since their last rankup.\n";
                }
                message += xMember + " has donated " + xMember.forma + " Forma\n";
                if(verbose)
                {
                    if (xMember.WFName != "" && xMember.WFName != "NaN") { message += "\nWarframe name is " + xMember.WFName; }
                    if (xMember.SKName != "" && xMember.SKName != "NaN") { message += "\nSpiral Knights name is " + xMember.SKName; }
                    if (xMember.steamName != "" && xMember.steamName != "NaN") { message += "\nSteam name is " + xMember.steamName; }
                }
                message += "```";
                Console.WriteLine(message);
                e.Channel.SendMessage(message);
            } catch(Exception exception)
            {
              Console.WriteLine("Error: " + exception);
            }
        }
        public static void promote(User member, MessageEventArgs e, string force, string date, bool isForce, bool help, bool ignoreCapacity)
        {
            if(help)
            {
                e.Channel.SendMessage("Help text");
            } else
            {
                Console.WriteLine(member.Name + " " + force + " " + help);
                XMLDocument memberDB = new XMLDocument(memberDBPath);
                XMLMember xMember = memberDB.getMemberById(member.Id);
                XMLMember xAuthor = memberDB.getMemberById(e.User.Id);
                if(xAuthor.checkPermissions(Rank.Officer)) {
                    if (e.User.Id != (ulong)xMember.discordId || memberDB.getMemberById(e.User.Id).checkPermissions(Rank.GuildMaster))
                    {
                        trilean isRankMaxed;
                        if(!isForce)
                        {
                            isRankMaxed = memberDB.checkRankMaxed(xMember);
                        } else
                        {
                            isRankMaxed = memberDB.checkRankMaxed(xMember, force);
                        }
                        if (isRankMaxed.value == 2 && (!ignoreCapacity || !xAuthor.checkPermissions(Rank.GuildMaster)))
                        {
                            e.Channel.SendMessage("```xl\nError: The rank you have requested to be promoted to is currently at maximum capacity.\nPlease contact a Guild Master if you believe this is in error.\n```");
                        }
                        else if (isRankMaxed.value == 1)
                        {
                            e.Channel.SendMessage("```xl\nAn unexpected error occured fetching Rank Capacity\n```");
                        }
                        else if (isRankMaxed.value == 0 || (ignoreCapacity && xAuthor.checkPermissions(Rank.General)))
                        {
                            if (isForce)
                            {
                                if (date != "")
                                {
                                    DateTime dateTime;
                                    bool s = false;
                                    try
                                    {
                                        dateTime = DateTime.Parse(date);
                                    }
                                    catch
                                    {
                                        e.Channel.SendMessage("```xl\nError: Invalid Date\n```");
                                        dateTime = DateTime.Now;
                                        s = true;
                                    }
                                    if (s != true)
                                    {
                                        trilean trilean = xMember.promote(dateTime, xAuthor, force);
                                        e.Channel.SendMessage(trilean.value.ToString());
                                        if (trilean)
                                        {
                                            member.AddRoles(e.Server.FindRoles((Rank)trilean.embedded).ToArray()[0]);
                                            //e.Server.AssignRoleToMember(e.Server.Roles.Find(x => x.Name == (string)trilean.embedded), member);
                                            e.Channel.SendMessage("```xl\nSuccessfully promoted " + xMember.name + " to " + trilean.embedded + "\n```");
                                        }
                                        else if (trilean.table[1])
                                        {
                                            if ((XMLErrorCode)trilean.embedded == XMLErrorCode.MultipleFound)
                                            {
                                                e.Channel.SendMessage("```xl\nError: Multiple Members found\n```");
                                            }
                                            else if ((XMLErrorCode)trilean.embedded == XMLErrorCode.Maximum)
                                            {
                                                e.Channel.SendMessage("```xl\nCan't promote " + xMember + " because they are already at maximum rank.\n```");
                                            }
                                            else if ((XMLErrorCode)trilean.embedded == XMLErrorCode.Greater)
                                            {
                                                e.Channel.SendMessage("```xl\nCan't promote " + xMember + " because the destination rank is higher than your rank!\n```");
                                            }
                                            else
                                            {
                                                e.Channel.SendMessage("```xl\nAn error occured\n```");
                                            }
                                        }
                                    }
                                }
                                else if (date == "")
                                {
                                    trilean trilean = xMember.promote(DateTime.Now, xAuthor, force);
                                    if (trilean)
                                    {
                                        member.AddRoles(e.Server.FindRoles((Rank)trilean.embedded).ToArray()[0]);
                                        //e.Server.AssignRoleToMember(e.Server.Roles.Find(x => x.Name == (string)trilean.embedded), member);
                                        e.Channel.SendMessage("```xl\nSuccessfully promoted " + xMember.name + " to " + force + "\n```");
                                    }
                                    else if (trilean.table[1])
                                    {
                                        if ((XMLErrorCode)trilean.embedded == XMLErrorCode.MultipleFound)
                                        {
                                            e.Channel.SendMessage("```xl\nError: Multiple Members found\n```");
                                        }
                                        else if ((XMLErrorCode)trilean.embedded == XMLErrorCode.Maximum)
                                        {
                                            e.Channel.SendMessage("```xl\nCan't promote " + xMember.name + " because they are already at maximum rank.\n```");
                                        }
                                        else if ((XMLErrorCode)trilean.embedded == XMLErrorCode.Greater)
                                        {
                                            e.Channel.SendMessage("```xl\nCan't promote " + xMember + " because the destination rank is higher than your rank!\n```");
                                        }
                                        else
                                        {
                                            e.Channel.SendMessage("```xl\nAn error occured\n```");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (date != "")
                                {
                                    DateTime dateTime = DateTime.Now;
                                    bool s = false;
                                    try
                                    {
                                        dateTime = DateTime.Parse(date);
                                    }
                                    catch
                                    {
                                        //throw new Exception("Error: Invalid Date");
                                        e.Channel.SendMessage("```xl\nError: Invalid Date\n```");
                                        s = true;
                                    }
                                    if (s != true)
                                    {
                                        trilean trilean = xMember.promote(dateTime, xAuthor);
                                        if (trilean)
                                        {
                                            member.AddRoles(e.Server.FindRoles((Rank)trilean.embedded).ToArray()[0]);
                                            //e.Server.AssignRoleToMember(e.Server.Roles.Find(x => x.Name == (string)trilean.embedded), member);
                                            e.Channel.SendMessage("```xl\nSuccessfully promoted " + xMember.name + " to " + trilean.embedded + "\n```");
                                        }
                                        else if (trilean.table[1])
                                        {
                                            if ((XMLErrorCode)trilean.embedded == XMLErrorCode.MultipleFound)
                                            {
                                                e.Channel.SendMessage("```xl\nError: Multiple Members found\n```");
                                            }
                                            else if ((XMLErrorCode)trilean.embedded == XMLErrorCode.Maximum)
                                            {
                                                e.Channel.SendMessage("```xl\nCan't promote " + xMember + " because they are already at maximum rank.\n```");
                                            }
                                            else if ((XMLErrorCode)trilean.embedded == XMLErrorCode.Greater)
                                            {
                                                e.Channel.SendMessage("```xl\nCan't promote " + xMember + " because the destination rank is higher than your rank!\n```");
                                            }
                                            else
                                            {
                                                e.Channel.SendMessage("```xl\nAn error occured\n```");
                                            }
                                        }
                                    }
                                }
                                else if (date == "")
                                {
                                    trilean trilean = xMember.promote(DateTime.Now, xAuthor);
                                    if (trilean)
                                    {
                                        member.AddRoles(e.Server.FindRoles((Rank)trilean.embedded).ToArray()[0]);
                                        //e.Server.AssignRoleToMember(e.Server.Roles.Find(x => x.Name == (string)trilean.embedded), member);
                                        e.Channel.SendMessage("```xl\nSuccessfully promoted " + xMember.name + " to " + trilean.embedded + "\n```");
                                    }
                                    else if (trilean.table[1])
                                    {
                                        if ((XMLErrorCode)trilean.embedded == XMLErrorCode.MultipleFound)
                                        {
                                            e.Channel.SendMessage("```xl\nError: Multiple Members found\n```");
                                        }
                                        else if ((XMLErrorCode)trilean.embedded == XMLErrorCode.Maximum)
                                        {
                                            e.Channel.SendMessage("```xl\nCan't promote " + xMember + " because they are already at maximum rank.\n```");
                                        }
                                        else if ((XMLErrorCode)trilean.embedded == XMLErrorCode.Greater)
                                        {
                                            e.Channel.SendMessage("```xl\nCan't promote " + xMember + " because the destination rank is higher than your rank!\n```");
                                        }
                                        else
                                        {
                                            e.Channel.SendMessage("```xl\nAn error occured\n```");
                                        }
                                    }
                                }
                            }
                        }
                    } else
                    {
                        e.Channel.SendMessage("```xl\nSorry, you can't promote yourself!\n```");
                    }
                } else
                {
                    e.Channel.SendMessage("```xl\nSorry, you don't have the permissions to do that\n```");
                }
            }
        }
        public static void createMember(User member, MessageEventArgs e, bool isSettingSteam, bool isSteamNumerical, bool isSettingDate, string date, string steamId)
        {
            XDocument memberDB = XDocument.Load(Path.Combine(configDir, "PersonellDB.xml"));
            IEnumerable<XElement> hasPermission = memberDB.Descendants("Rank").Where(x => x.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == e.User.Name));
            foreach (XElement p in hasPermission)
            {
                Console.WriteLine(p.Value);
                IEnumerable<XElement> hasPermissionNum = memberDB.Descendants("Define").Where(x => x.Attribute("name").Value == p.Value);
                foreach (XElement q in hasPermissionNum)
                {

                    if (Int32.Parse(q.Value) >= 4)
                    {
                        Console.WriteLine(q.Value);
                        Console.WriteLine("Creating");
                        Console.WriteLine(member.Name);
                        XDocument memberDBT = XDocument.Load(Path.Combine(configDir, "PersonellDB.xml"));
                        IEnumerable<XElement> doc = memberDBT.Descendants("Database");
                        foreach (XElement h in doc)
                        {
                            bool jExists = false;
                            IEnumerable<XElement> exists = memberDBT.Descendants("DiscordId").Where(x => x.Value == member.Id.ToString());
                            foreach (XElement j in exists)
                            {
                                jExists = true;
                            }
                            if (!jExists)
                            {
                                IEnumerable<XElement> existsBackup = memberDBT.Descendants("Discord").Where(x => x.Value == member.Name);
                                foreach (XElement k in existsBackup)
                                {
                                    jExists = true;
                                    IEnumerable<XElement> setId = memberDBT.Descendants("DiscordId").Where(x => x.Value == member.Name);
                                    foreach (XElement l in setId)
                                    {
                                        k.SetValue(member.Id);
                                        memberDBT.Save(Path.Combine(configDir, "PersonellDB.xml"));
                                    }
                                }
                                if (!jExists)
                                {
                                    //DateTime now = DateTime.Parse(DateTime.Now.ToString(new CultureInfo("en-us")));
                                    DateTime now = DateTime.Now;
                                    h.Add(
                                        new XElement("Member",
                                            new XElement("Name", member.Name),
                                            new XElement("Rank", "Recruit"),
                                            new XElement("RankupHistory",
                                                new XElement("Rankup", new XAttribute("name", "Recruit"), Regex.Match(now.ToString(), @"(.+) [0-9]+:[0-9]+:[0-9]+ .M").Groups[1].Value),
                                                new XElement("Rankup", new XAttribute("name", "Member"), "NaN"),
                                                new XElement("Rankup", new XAttribute("name", "Member II"), "NaN"),
                                                new XElement("Rankup", new XAttribute("name", "Veteran"), "NaN"),
                                                new XElement("Rankup", new XAttribute("name", "Officer"), "NaN"),
                                                new XElement("Rankup", new XAttribute("name", "General"), "NaN"),
                                                new XElement("Rankup", new XAttribute("name", "Guild Master"), "NaN")),
                                            new XElement("Names",
                                                new XElement("Warframe", ""),
                                                new XElement("SpiralKnights", ""),
                                                new XElement("Discord", member.Name),
                                                new XElement("DiscordId", member.Id),
                                                new XElement("Steam", steamId),
                                                new XElement("SteamId", new XAttribute("numerical", isSteamNumerical.ToString()), steamId)),
                                            new XElement("FormaDonated", 0))
                                        );
                                    if (isSettingDate)
                                    {
                                        IEnumerable<XElement> i = h.Descendants("Member").Where(x => x.Descendants("Name").Any(y => y.Value == member.Name));
                                        foreach (XElement ii in i)
                                        {
                                            IEnumerable<XElement> j = i.Descendants("Rankup").Where(y => y.Attribute("name").Value == "Recruit");
                                            foreach (XElement jj in j)
                                            {
                                                ii.SetValue(date);
                                            };
                                        };
                                    };
                                    Console.WriteLine(h);
                                    Console.WriteLine(isSettingDate);
                                    Console.WriteLine(now.ToString());
                                    Console.WriteLine(Regex.Match(now.ToString(), @"(.+) [0-9]+:[0-9]+:[0-9]+ .M").Groups[1].Value);
                                    Console.WriteLine("Created");
                                    memberDBT.Save(Path.Combine(configDir, "PersonellDB.xml"));
                                    e.Channel.SendMessage("Successfully created member " + member.Name);
                                }
                                else
                                {
                                    e.Channel.SendMessage("An error occured, that member already exists!");
                                    Console.WriteLine("exists");
                                }

                            }
                            else
                            {
                                e.Channel.SendMessage("An error occured, that member already exists!");
                                Console.WriteLine("exists");
                            }

                        }
                    }
                    else
                    {
                        Console.WriteLine(q.Value);
                        Console.WriteLine("Sorry, you don't seem to have permission to perform that action!");
                    }
                }
            }
        }
        public static void updateMember(string member, MessageEventArgs e, string node, string targetValue, string attribute, string attributeValue, bool isSettingAttribute, bool isGettingByAttribute)
        {
            XMLDocument memberDB = new XMLDocument(memberDBPath);
            XMLMember xAuthor = memberDB.getMemberById(e.User.Id);
            if(xAuthor.checkPermissions(Rank.Officer))
            {
                XMLMember xMember = memberDB.getMemberById(member);
                if (isSettingAttribute)
                {
                    xMember.xE.Descendants(node).First().Attribute(attribute).Value = attributeValue;
                } else if (isGettingByAttribute) {
                    xMember.xE.Descendants(node).Where(x => x.Attribute(attribute).Value == attributeValue).First().Value = targetValue;
                } else if (!isGettingByAttribute && !isSettingAttribute) {
                    xMember.xE.Descendants(node).First().Value = targetValue;
                }
                xMember.x.Save(memberDBPath);
            } else
            {
                e.Channel.SendMessage("```xl\nSorry, you don't seem to have permission to perform that action!\n```");
            }
        }
        public static void news(MessageEventArgs e)
        {
            using (StreamReader sr = File.OpenText(Path.Combine(configDir, "news.txt")))
            {
                string s = sr.ReadToEnd();
                e.Channel.SendMessage(s);
            }
        }
        public static void updateNews(MessageEventArgs e, string news, bool silent, string force = "general")
        {
            XMLDocument memberDB = new XMLDocument(memberDBPath);
            XMLMember xMember = memberDB.getMemberById(e.User.Id);
            Console.WriteLine(force);
            if (xMember.checkPermissions("Officer"))
            {
                if (xMember.name != "Mardan")
                {
                    File.WriteAllText(Path.Combine(configDir, "news.txt"), DateTime.Now + " -- " + news);
                    e.Channel.SendMessage("News Updated: " + news);
                    if (!silent) { client.FindServers("Spawner Swarm").ToArray()[0].FindChannels(force, ChannelType.Text, true).ToArray()[0].SendMessage("News Updated: " + news); }
                    //if (!silent) { client.Servers.Where(x => x.Name == "Spawner Swarm").ToArray()[0].Find(x => x.Name == force).SendMessage("News Updated: " + news); }
                }
                else
                {
                    if (Regex.IsMatch(news, @"SwarmBot [^ ]+ [^ ]+ patch notes are live: https:\/\/github\.com\/SpawnerSwarm\/SwarmBot\/commit\/.+", RegexOptions.IgnoreCase))
                    {
                        e.Channel.SendMessage("Patch notes updated");
                        client.FindServers("Spawner Swarm").ToArray()[0].FindChannels("swarmbotpatchnotes").ToArray()[0].SendMessage(DateTime.Now + " -- " + news);
                        //client.GetChannelByName("swarmbotpatchnotes").SendMessage(DateTime.Now + " -- " + news);
                    }
                    else
                    {
                        File.WriteAllText(Path.Combine(configDir, "news.txt"), DateTime.Now + " -- " + news);
                        e.Channel.SendMessage("News Updated: " + news);
                        if (!silent) { client.FindServers("Spawner Swarm").ToArray()[0].FindChannels(force, ChannelType.Text, true).ToArray()[0].SendMessage("News Updated: " + news); }
                        //if (!silent) { client.GetServersList().Find(x => x.Name == "Spawner Swarm").Channels.Find(x => x.Name == force).SendMessage("News Updated: " + news); }
                    }
                }
            }
            else
            {
                e.Channel.SendMessage("Sorry, you don't have permission to perform that action.");
            }
        }
        public static void getEmote(MessageEventArgs e, Emotes emotes, string cmd = "list")
        {
            XMLDocument memberDB = new XMLDocument(memberDBPath);
            XMLMember author = memberDB.getMemberById(e.User.Id);

            if(cmd.StartsWith("list"))
            {
                if(cmd == "list")
                {
                    var block = emotes.list(0, memberDB);
                    Console.WriteLine(block == "");
                    Console.WriteLine(block);
                    e.Channel.SendMessage(block);
                } else
                {
                    int i = 0;
                    try
                    {
                        i = int.Parse(cmd.Replace("list", "").Replace("-", ""));
                        e.Channel.SendMessage(emotes.list(i, memberDB));
                    } catch
                    {
                        trilean t = emotes.getEmote(cmd.Replace("list", ""));
                        if (t)
                        {
                            Emote emote = (Emote)t.embedded;
                            bool hasPermission = emote.getEligible(author);
                            e.Channel.SendMessage(emotes.getEmoteData((Emote)t.embedded, memberDB, hasPermission));
                        } else if (t.value == 1 && (XMLErrorCode)t.embedded == XMLErrorCode.MultipleFound)
                        {
                            e.Channel.SendMessage("```xl\nError: Multiple emotes found.\n```");
                        } else if (t.value == 2)
                        {
                            e.Channel.SendMessage("```xl\nError: Requested emote was not found.\n```");
                        } else
                        {
                            e.Channel.SendMessage("```xl\nAn unknown error occured.\n```");
                        }
                    }
                }
            }
            else
            {
                //Emote emote = (Emote)emotes.getEmote(cmd).embedded;
                trilean t = emotes.getEmote(cmd);
                if (t.value == 0)
                {
                    Emote emote = (Emote)t.embedded;
                    if (emote.getEligible(author))
                    {
                        e.Channel.SendMessage(emote.URL);
                    }
                    else
                    {
                        e.Channel.SendMessage("Sorry, you don't have permission to send that emote!");
                    }
                } else if(t == false)
                {
                    e.Channel.SendMessage("Could not find emote " + cmd);
                } else if(t.value == 1)
                {
                    if((XMLErrorCode)t.embedded == XMLErrorCode.MultipleFound)
                    {
                        e.Channel.SendMessage("Error: Multiple emotes found by that ref");
                    } else
                    {
                        e.Channel.SendMessage("An unknown error occured");
                    }
                }
            }
        }
        public static void createEmote(MessageEventArgs e, Emotes emotes, string name, string URL, string reference, short reqRank, string author = "Mardan")
        {
            XMLDocument memberDB = new XMLDocument(memberDBPath);
            XMLMember xAuthor = memberDB.getMemberByUsername(author);
            Emote emote = new Emote(URL, name, reference, reqRank, xAuthor);
            if (e.User.Name == "Mardan")
            {
                trilean success = emotes.newEmote(emote);

                if (success.table[1] == true)
                {
                    e.Channel.SendMessage("There was an error creating a new emote; an emote with the same " + success.embedded + " already exists");
                }
                else
                {
                    e.Channel.SendMessage("Successfully created emote " + name + "!");
                }
            }
            else
            {
                e.Channel.SendMessage("Sorry, you don't have permissions to add emotes. Send an application to a Guild Master instead.");
            }
        }
        public static void addForma(MessageEventArgs e, User member, short formas)
        {
            XMLDocument memberDB = new XMLDocument(memberDBPath);
            XMLMember author = memberDB.getMemberById(e.User.Id);
            XMLMember xMember = memberDB.getMemberById(member.Id);
            if (author.checkPermissions(5))
            {
                if (author.discordId != xMember.discordId || author.checkPermissions(Rank.GuildMaster))
                {
                    if (memberDB.getDefine(author.rank, DefineType.Promotion) > memberDB.getDefine(xMember.rank, DefineType.Promotion))
                    {
                        trilean t = xMember.addForma(formas);
                        if (t.table[1])
                        {
                            if ((XMLErrorCode)t.embedded == XMLErrorCode.MultipleFound)
                            {
                                e.Channel.SendMessage("Error: Multiple members were found. Could not add forma.");
                            }
                            else
                            {
                                e.Channel.SendMessage("An unknown error occured. Unable to add forma");
                            }
                        }
                        else if (t)
                        {
                            e.Channel.SendMessage("Successfully added " + formas + " formas to " + xMember);
                        }
                    } else
                    {
                        e.Channel.SendMessage("You must be a higher rank to add forma to " + xMember);
                    }
                } else
                {
                    e.Channel.SendMessage("You can't add forma to yourself!");
                }
            }
        }
        public static void priceCheck(MessageEventArgs e, string id)
        {
            trilean t = Program.nexus.getItemById(id);
            if (t.value == 0)
            {
                Item i = (Item)t.embedded;
                //e.Channel.SendMessage(i.Title);
                string message = "```xl\n";
                Component[] set = i.Components.Where(x => x.name == "Set").ToArray();
                if (set.Length > 0)
                {
                    message += "Average complete set price is " + set[0].avg + "\n";
                }
                foreach(Component component in i.Components)
                {
                    if(component.name != "Set")
                    {
                        message += "\t" + component.name + " average price is " + component.avg + "\n";
                    }
                }
                message += "Supply is at " + i.SupDem[0] + "%\n";
                message += "Demand is at " + i.SupDem[1] + "%\n";
                message += "\n```\nThis feature is in Beta. Stats provided by https://nexus-stats.com.";
                e.Channel.SendMessage(message);
            }
            else
            {
                e.Channel.SendMessage("Sorry, could not find the item you requested.");
            }
        }
        public static void getMemberCount(MessageEventArgs e)
        {
            XMLDocument memberDB = new XMLDocument(memberDBPath);
            string message = "```xl\n";
            for (short i = 1; i <= 7; i++)
            {
                string rankName = memberDB.getDefineName(i, DefineType.Promotion);
                int memberCount = memberDB.document.Descendants("Member").Where(x => x.Descendants("Rank").ToArray()[0].Value == rankName).ToArray().Count();
                message += rankName + ": " + memberCount + " out of " + memberDB.getDefine(rankName, DefineType.RankCapacity) + "\n";
            }
            message += "\n```";
            e.Channel.SendMessage(message);
        }
        public static void listEvents(MessageEventArgs e, Events events, int page = 0)
        {
            XMLDocument memberDB = new XMLDocument(memberDBPath);
            string block = events.list(page, memberDB);
            e.Channel.SendMessage(block);
        }
        public static void displayEvent(MessageEventArgs e, Events events, string _ref = "Latest")
        {
            Event _event = events.getLatestEvent();
            if(_ref != "Latest")
            {
                trilean t = events.getEvent(_ref);
                if(t.value == 0)
                {
                    _event = (Event)t.embedded;
                } else if(t.value == 1)
                {
                    e.Channel.SendMessage("```xl\nSorry, multiple events were found by that ref.\n```");
                    return;
                } else if(t.value == 2)
                {
                    e.Channel.SendMessage("```xl\nSorry, that event could not be found.\n```");
                    return;
                }
            }
            string block = "";
            block += _event.icon + " **" + _event.name + "** " + _event.icon + "\n\n";
            block += _event.lotusText;
            block += "\n\nTasks:\n\n";
            foreach(eTask task in _event.tasks)
            {
                block += "\t " + task.getTask() + "\n";
            }
            block += "\t " + _event.finalTask.getTask();
            block += "\nRewards:\n\n";
            foreach(Reward reward in _event.rewards)
            {
                block += "\t " + reward.getReward() + "\n";
            }
            block += "\n" + _event.specialText;
            e.Channel.SendMessage(block);
        }
        public static async Task archive(MessageEventArgs e, string tChannel)
        {
            XMLDocument memberDB = new XMLDocument(memberDBPath);
            if (memberDB.getMemberById(e.User.Id).checkPermissions(Rank.GuildMaster))
            {
                List<Channel> channelList = e.Server.FindChannels(tChannel, ChannelType.Text, true).ToList();
                if(channelList.Count != 1) { await e.Channel.SendMessage("```xl\nError: Channel not found or multiple found.\n```"); return; }
                Channel channel = channelList[0];
                bool b = false;
                Message[] messages = await channel.DownloadMessages();
                //messages = messages.Reverse().ToArray();
                while (!b)
                {
                    Message[] newMessages = await channel.DownloadMessages(100, messages[messages.Length - 2].Id);
                    //Console.WriteLine(messages.Length);
                    //Console.WriteLine(messages[0].Text + "  ||  " + newMessages[0].Text);
                    if (messages.Length % 100 != 0 || newMessages.Last().Id == messages.Last().Id)
                    {
                        b = true;
                    }
                    else
                    {
                        Message[] tempMessages = messages;
                        messages = new Message[tempMessages.Length + newMessages.Length - 1];
                        //Array.Copy(tempMessages, 0, messages, 0, tempMessages.Length - 1);
                        //Console.WriteLine(messages.Length);
                        //Array.Copy(newMessages, 1, messages, tempMessages.Length, newMessages.Length - 1);
                        for (int i = 0; i < tempMessages.Length; i++)
                        {
                            messages[i] = tempMessages[i];
                            Console.WriteLine(i);
                        }
                        Console.WriteLine(newMessages.Length + " " + messages.Length);
                        for (int i = 1; i < newMessages.Length; i++)
                        {
                            Console.WriteLine(i - 1 + tempMessages.Length);
                            messages[i - 1 + tempMessages.Length] = newMessages[i];
                        }
                    }                 
                }
                messages = messages.Reverse().ToArray();
                await e.Channel.SendMessage(messages.Length.ToString());
                Channel destChannel = await client.GetServer(archiveServerID).CreateChannel(channel.Name, ChannelType.Text);
                for(int i = 0; i < messages.Length; i++)
                {
                    await destChannel.SendMessage("(" + messages[i].Timestamp + ") " + messages[i].User.Name + ": " + messages[i].Text);
                    System.Threading.Thread.Sleep(100);
                }
            }
            else
            {
                await e.Channel.SendMessage("```xl\nError: You do not have permission to perform that action.\n```");
            }
        } 
    }
}
