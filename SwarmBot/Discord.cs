﻿using DiscordSharp;
using DiscordSharp.Objects;
using DiscordSharp.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.IO;
using System.Globalization;
using SwarmBot;
using Trileans;

namespace SwarmBot
{
    class Discord
    {
        public static DiscordClient client = new DiscordClient();
        private static string email;
        private static string password;
        public static DiscordServer Swarm;
        public static string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SwarmBot\\");

        public static bool SendInvite(string recipient, string Game, DiscordMessageEventArgs e)
        {
            //SendInvite(firstMatch, fifthMatch, firstMatch.StartsWith("<@"), e);

            bool isTag = recipient.StartsWith("<@");

            if (isTag)
            {
                e.Channel.Parent.GetMemberByKey(recipient.Replace("<", "").Replace("@", "").Replace(">", "")).SendMessage("@" + e.Message.Author.Username + " has invited you to play: " + Game);
            }
            else {
                e.Channel.Parent.GetMemberByUsername(recipient).SendMessage("@" + e.Message.Author.Username + " has invited you to play: " + Game);
            }

            return true;
        }

        public static void Connect()
        {
            using (StreamReader sr = File.OpenText(Path.Combine(configDir, "config.txt")))
            {
                Match info = Regex.Match(sr.ReadToEnd(), @"(.+);(.+);(.+)");
                email = info.Groups[1].Value;
                password = info.Groups[2].Value;
            }

            client.ClientPrivateInformation.Email = email;
            client.ClientPrivateInformation.Password = password;

            try
            {
                client.SendLoginRequest();
                client.Connect();
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong!\n" + e.Message + "\nPress any key to close this window.");
            }
        }
        public static void help(DiscordMessageEventArgs e)
        {
            e.Channel.SendMessage("I am the SwarmBot created by @Mardan. View my source code: https://github.com/SpawnerSwarm/SwarmBot. I can:");
            e.Channel.SendMessage("--   Search the Warframe Wiki (!wfwiki <page name>)");
            e.Channel.SendMessage("--   Search the Spiral Knights Wiki (!skwiki <page name>)");
            e.Channel.SendMessage("--   Link you to the Guild Mail (!guildmail)");
            e.Channel.SendMessage("--   Send you the latest news (!news)");
            e.Channel.SendMessage("--   Update the news [Officer +] (!updateNews <News>)");
            e.Channel.SendMessage("--   Invite a group of players to play a game (!invite <Number of invitees> <Discord username 1>, [Discord username 2], [Discord username 3], [Discord username 4] <Game name>");
            e.Channel.SendMessage("--   Get a member's information (!getMember <@Member>)");
            e.Channel.SendMessage("--   Create a new member entry [Veteran +] (!createMember <@Member> [--date|-d 01/01/0001] [--steam|-s Steam Name] [--populate|-p Deprecated])");
            e.Channel.SendMessage("--   Promote a member [Officer +] (!promote <@Member> [--force|-f (Rank)] [--date|-d 01/01/0001]");
            //e.Channel.SendMessage("More functions will be added soon; feel free to pm @Mardan with suggestions!");
            Console.WriteLine("Sent help because of this message: " + e.MessageText);
        }
        public static void wiki(DiscordMessageEventArgs e, string wiki)
        {
            string target = null;
            if (wiki == "wf")
            {
                target = Regex.Match(e.MessageText,
                    @"!wfwiki (.+)",
                    RegexOptions.Singleline)
                .Groups[1].Value;
                e.Channel.SendMessage("<@" + e.Message.Author.ID + "> http://warframe.wikia.com/wiki/" + target.Replace(" ", "_"));
            }
            else if(wiki == "sk")
            {
                target = Regex.Match(e.MessageText,
                    @"!skwiki (.+)",
                    RegexOptions.Singleline)
                .Groups[1].Value;
                e.Channel.SendMessage("<@" + e.Message.Author.ID + "> http://wiki.spiralknights.com/" + target.Replace(" ", "_"));
            }
            Console.WriteLine("Sent " + target.Replace(" ", "_") + " to " + e.Author.ID + " because of this message: " + e.MessageText);
        }
        public static void invite(DiscordMessageEventArgs e)
        {
            string numInvitees = Regex.Match(e.MessageText, @"([1-8])", RegexOptions.Singleline).Groups[1].Value;
            if (numInvitees != "")
            {
                if (numInvitees == "1")
                {
                    Match matches = Regex.Match(e.MessageText, @"!invite (?:[1-8]) (?:@)?(.+) (.+)");
                    string firstMatch = matches.Groups[1].Value;
                    string secondMatch = matches.Groups[2].Value;
                    /*Console.WriteLine(firstMatch);
                    Console.WriteLine(secondMatch);*/
                    // In the future, this will also dispatch a steam message.
                    try
                    {
                        SendInvite(firstMatch, secondMatch, e);
                    }
                    catch (Exception)
                    {
                        e.Channel.SendMessage("Sorry, something went wrong. Perhaps your syntax is off?");
                    }
                }
                else if (numInvitees == "2")
                {
                    Match matches = Regex.Match(e.MessageText, @"!invite (?:[1-8]) (?:@)?(.+) (?:@)?(.+) (.+)");
                    string firstMatch = matches.Groups[1].Value;
                    string secondMatch = matches.Groups[2].Value;
                    string thirdMatch = matches.Groups[3].Value;
                    try
                    {
                        SendInvite(firstMatch, thirdMatch, e);
                        SendInvite(secondMatch, thirdMatch, e);
                    }
                    catch (Exception)
                    {
                        e.Channel.SendMessage("Sorry, something went wrong. Perhaps your syntax is off?");
                    }
                }
                else if (numInvitees == "3")
                {
                    Match matches = Regex.Match(e.MessageText, @"!invite (?:[1-8]) (?:@)?(.+) (?:@)?(.+) (?:@)?(.+) (.+)");
                    string firstMatch = matches.Groups[1].Value;
                    string secondMatch = matches.Groups[2].Value;
                    string thirdMatch = matches.Groups[3].Value;
                    string fourthMatch = matches.Groups[4].Value;
                    try
                    {
                        SendInvite(firstMatch, fourthMatch, e);
                        SendInvite(secondMatch, fourthMatch, e);
                        SendInvite(thirdMatch, fourthMatch, e);
                    }
                    catch (Exception)
                    {
                        e.Channel.SendMessage("Sorry, something went wrong. Perhaps your syntax is off?");
                    }
                }
                else if (numInvitees == "4")
                {
                    Match matches = Regex.Match(e.MessageText, @"!invite (?:[1-8]) (?:@)?(.+) (?:@)?(.+) (?:@)?(.+) (?:@)?(.+) (.+)");
                    string firstMatch = matches.Groups[1].Value;
                    string secondMatch = matches.Groups[2].Value;
                    string thirdMatch = matches.Groups[3].Value;
                    string fourthMatch = matches.Groups[4].Value;
                    string fifthMatch = matches.Groups[5].Value;

                    try
                    {
                        SendInvite(firstMatch, fifthMatch, e);
                        SendInvite(secondMatch, fifthMatch, e);
                        SendInvite(thirdMatch, fifthMatch, e);
                        SendInvite(fourthMatch, fifthMatch, e);
                    }
                    catch (Exception)
                    {
                        e.Channel.SendMessage("Sorry, something went wrong. Perhaps your syntax is off?");
                    }

                }
                else
                {
                    e.Channel.SendMessage("Sorry, only 1-4 invitees are supported with one command!");
                };
            }
            else
            {
                e.Channel.SendMessage("Error: Number of invitees not specified.");
            };
        }
        public static void getMember(DiscordMember member, DiscordMessageEventArgs e, bool force, bool help)
        {
            if(help)
            {
                e.Channel.SendMessage("Help text");
            } else
            {
                if(force)
                {
                    var destChannel = e.Channel;
                }
                try
                {
                    Console.WriteLine(member + " " + force + " " + help);
                    XMLDocument memberDB = new XMLDocument(Path.Combine(configDir + "PersonellDB.xml"));
                    XMLMember xMember = memberDB.getMemberById(member.ID);
                    Console.WriteLine(xMember.name + " is a(n) " + xMember.rank);
                    e.Channel.SendMessage(xMember.name + " is a(n) " + xMember.rank);
                    trilean isReady = xMember.checkReadyForRankUp();
                    if (isReady.table[1])
                    {
                        e.Channel.SendMessage("We can't tell whether or not " + xMember.name + " is ready for an upgrade, but they probably are since our data dates back before this bot's conception");
                    }
                    else {
                        if (isReady.table[0])
                        {
                            e.Channel.SendMessage("He/she is eligible for a rankup.");
                        }
                        else if (!isReady.table[0] && isReady.embedded != "Max")
                        {
                            e.Channel.SendMessage("He/she is not eligible for a rankup at this time.");
                        } else if(isReady.embedded == "Max")
                        {
                            e.Channel.SendMessage("He/she has reached the maximum possible rank");
                        }
                        if(isReady.embedded != "Max")
                        {
                            e.Channel.SendMessage("It has been " + isReady.embedded + " days since their last rankup.");
                        } 
                    }
                } catch(Exception exception)
                {
                  Console.WriteLine("Error: " + exception);
                }
            }
        }
        public static void promote(DiscordMember member, DiscordMessageEventArgs e, string force, string date, bool isForce, bool help)
        {
            /*if (!help)
            {
                Console.WriteLine(member.Username + " " + force + " " + help);
                XDocument memberDB = XDocument.Load(Path.Combine(configDir, "PersonellDB.xml"));
                IEnumerable<XElement> hasPermission = memberDB.Descendants("Rank").Where(x => x.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == e.Author.Username));
                foreach (XElement p in hasPermission)
                {
                    IEnumerable<XElement> hasPermissionNum = memberDB.Descendants("Define").Where(x => x.Attribute("name").Value == p.Value);
                    foreach (XElement q in hasPermissionNum)
                    {
                        if (Int32.Parse(q.Value) >= 5)
                        {
                            IEnumerable<XElement> rank = memberDB.Descendants("Rank")
                            .Where(x => x.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == member.Username));
                            foreach (XElement h in rank)
                            {
                                var hRank = h.Value;
                                IEnumerable<XElement> hRankId = memberDB.Descendants("Define")
                                    .Where(x => x.Attribute("name").Value == hRank);
                                foreach (XElement i in hRankId)
                                {
                                    Console.WriteLine(i.Value);
                                    Console.WriteLine(Int32.Parse(i.Value));
                                    int iHRankId = Int32.Parse(i.Value);

                                    var iHRankIdProgressed = iHRankId + 1;
                                    IEnumerable<XElement> toRank = memberDB.Descendants("Define")
                                        .Where(x => x.Value == iHRankIdProgressed.ToString());
                                    foreach (XElement j in toRank)
                                    {
                                        string jToRank = force;
                                        if (jToRank == "")
                                        {
                                            jToRank = j.Attribute("name").Value;
                                        }
                                        Console.WriteLine(jToRank);
                                        try { e.Channel.Parent.AssignRoleToMember(e.Channel.Parent.Roles.Find(x => x.Name == jToRank), member); } catch (Exception) { Console.WriteLine("No Rank exists"); };
                                        h.Value = jToRank;
                                        DateTime now = DateTime.Parse(DateTime.Now.ToString(new CultureInfo("en-us")));
                                        Console.WriteLine(now);
                                        IEnumerable<XElement> lastRankUp = memberDB.Descendants("RankupHistory").Descendants("Rankup")
                                            .Where(x => x.Parent.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == member.Username))
                                            .Where(x => x.Attribute("name").Value == jToRank);
                                        foreach (XElement k in lastRankUp)
                                        {
                                            if (date != "") { k.Value = date; }
                                            else { k.Value = Regex.Match(now.ToString(), @"(.+) [1-9]+:[0-9]+:[0-9]+ .M").Groups[1].Value; }
                                        }
                                        memberDB.Save(Path.Combine(configDir, "PersonellDB.xml"));
                                        e.Channel.SendMessage("Successfully promoted " + member.Username + " to " + jToRank);

                                    }

                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Sorry, you don't seem to have permission to perform that action!");
                        }
                    }



                }
            }*/
            if(help)
            {
                e.Channel.SendMessage("Help text");
            } else
            {
                Console.WriteLine(member.Username + " " + force + " " + help);
                XMLDocument memberDB = new XMLDocument(Path.Combine(configDir, "PersonellDB.xml"));
                XMLMember xMember = memberDB.getMemberById(member.ID);
                if(memberDB.getMemberById(e.Author.ID).checkPermissions("Officer")) {
                    if(isForce)
                    {
                        if(date != "")
                        {
                            DateTime dateTime;
                            string s = "n";
                            try
                            {
                                dateTime = DateTime.Parse(date);
                            } catch
                            {
                                throw new Exception("Error: Invalid Date");
                                e.Channel.SendMessage("Error: Invalid Date");
                                s = "y";
                            }
                            if(s != "y")
                            {
                                xMember.promote(dateTime, force);
                            }                      
                        } else if(date == "")
                        {
                            trilean trilean = xMember.promote(DateTime.Now, force);
                            if(trilean)
                            {
                                e.Channel.Parent.AssignRoleToMember(e.Channel.Parent.Roles.Find(x => x.Name == trilean.embedded), member);
                                e.Channel.SendMessage("Successfully promoted " + xMember.name + " to " + force);
                            } else if(trilean.table[1])
                            {
                                if (trilean.embedded == "Multiple")
                                {
                                    e.Channel.SendMessage("Error: Multiple Members found");
                                }
                                else if (trilean.embedded == "Max")
                                {
                                    e.Channel.SendMessage("Can't promote " + xMember.name + " because they are already at maximum rank.");
                                }
                                else
                                {
                                    e.Channel.SendMessage("An error occured");
                                }
                            }
                        }
                    } else
                    {
                        if (date != "")
                        {
                            DateTime dateTime;
                            string s = "n";
                            try
                            {
                                dateTime = DateTime.Parse(date);
                            }
                            catch
                            {
                                throw new Exception("Error: Invalid Date");
                                e.Channel.SendMessage("Error: Invalid Date");
                                s = "y";
                            }
                            if (s != "y")
                            {
                                xMember.promote(dateTime);
                            }
                        }
                        else if (date == "")
                        {
                            trilean trilean = xMember.promote(DateTime.Now);
                            if(trilean) {
                                e.Channel.Parent.AssignRoleToMember(e.Channel.Parent.Roles.Find(x => x.Name == trilean.embedded), member);
                                e.Channel.SendMessage("Successfully promoted " + xMember.name + " to " + trilean.embedded);
                            } else if (trilean.table[1])
                            {
                                if (trilean.embedded == "Multiple")
                                {
                                    e.Channel.SendMessage("Error: Multiple Members found");
                                }
                                else if (trilean.embedded == "Max")
                                {
                                    e.Channel.SendMessage("Can't promote " + xMember + " because they are already at maximum rank.");
                                }
                                else
                                {
                                    e.Channel.SendMessage("An error occured");
                                }
                            }
                        }
                    }
                } else
                {
                    e.Channel.SendMessage("Sorry, you don't have the permissions to do that");
                }
            }
        }
        public static void createMember(DiscordMember member, DiscordMessageEventArgs e, bool isSettingSteam, bool isSteamNumerical, bool isSettingDate, string date, string steamId)
        {
            XDocument memberDB = XDocument.Load(Path.Combine(configDir, "PersonellDB.xml"));
            IEnumerable<XElement> hasPermission = memberDB.Descendants("Rank").Where(x => x.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == e.Author.Username));
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
                        Console.WriteLine(member.Username);
                        XDocument memberDBT = XDocument.Load(Path.Combine(configDir, "PersonellDB.xml"));
                        IEnumerable<XElement> doc = memberDBT.Descendants("Database");
                        foreach (XElement h in doc)
                        {
                            bool jExists = false;
                            IEnumerable<XElement> exists = memberDBT.Descendants("DiscordId").Where(x => x.Value == member.ID);
                            foreach (XElement j in exists)
                            {
                                jExists = true;
                            }
                            if (!jExists)
                            {
                                IEnumerable<XElement> existsBackup = memberDBT.Descendants("Discord").Where(x => x.Value == member.Username);
                                foreach (XElement k in existsBackup)
                                {
                                    jExists = true;
                                    IEnumerable<XElement> setId = memberDBT.Descendants("DiscordId").Where(x => x.Value == member.Username);
                                    foreach (XElement l in setId)
                                    {
                                        k.SetValue(member.ID);
                                        memberDBT.Save(Path.Combine(configDir, "PersonellDB.xml"));
                                    }
                                }
                                if (!jExists)
                                {
                                    //DateTime now = DateTime.Parse(DateTime.Now.ToString(new CultureInfo("en-us")));
                                    DateTime now = DateTime.Now;
                                    h.Add(
                                        new XElement("Member",
                                            new XElement("Name", member.Username),
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
                                                new XElement("Discord", member.Username),
                                                new XElement("DiscordId", member.ID),
                                                new XElement("Steam", steamId),
                                                new XElement("SteamId", new XAttribute("numerical", isSteamNumerical.ToString()), steamId)))
                                        );
                                    if (isSettingDate)
                                    {
                                        IEnumerable<XElement> i = h.Descendants("Member").Where(x => x.Descendants("Name").Any(y => y.Value == member.Username));
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
                                    e.Channel.SendMessage("Successfully created member " + member.Username);
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
        public static void updateMember(DiscordMember member, DiscordMessageEventArgs e, string node, string targetValue, string attribute, string attributeValue, bool isSettingAttribute, bool isGettingByAttribute)
        {
            XDocument memberDB = XDocument.Load(Path.Combine(configDir, "PersonellDB.xml"));
            IEnumerable<XElement> hasPermission = memberDB.Descendants("Rank").Where(x => x.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == e.Author.Username));
            foreach (XElement p in hasPermission)
            {
                Console.WriteLine(p.Value);
                IEnumerable<XElement> hasPermissionNum = memberDB.Descendants("Define").Where(x => x.Attribute("name").Value == p.Value);
                foreach (XElement q in hasPermissionNum)
                {
                    if (Int32.Parse(q.Value) >= 5)
                    {
                        Console.WriteLine(q.Value);
                        XDocument memberDBT = XDocument.Load(Path.Combine(configDir, "PersonellDB.xml"));
                        IEnumerable<XElement> doc = memberDBT.Descendants("Database");
                        foreach (XElement h in doc)
                        {
                            Console.WriteLine("Loaded Document");
                            bool multiple = false;
                            bool multipleFound = false;
                            string successMessage = null;
                            IEnumerable<XElement> docMember = h.Descendants("Member").Where(x => x.Descendants("Discord").Any(y => y.Value == member.Username));
                            foreach (XElement i in docMember)
                            {
                                if (isGettingByAttribute)
                                {
                                    IEnumerable<XElement> iNode = i.Descendants(node).Where(x => x.Attribute(attribute).Value == attributeValue);
                                    foreach (XElement j in iNode)
                                    {
                                        Console.WriteLine("Found node");
                                        if (!multiple)
                                        {
                                            multiple = true;
                                            j.Value = targetValue;
                                            successMessage = "Successfully set " + node + " of type " + attributeValue + " of " + member.Username + " to " + targetValue;
                                            Console.WriteLine(j.Value);
                                        }
                                        else
                                        {
                                            Console.WriteLine("multiple");
                                            multipleFound = true;
                                        }
                                    }
                                }
                                else if (isSettingAttribute)
                                {
                                    IEnumerable<XAttribute> iAttribute = i.Descendants(node).Attributes(attribute);
                                    foreach (XAttribute j in iAttribute)
                                    {
                                        Console.WriteLine("Found node");
                                        if (!multiple)
                                        {
                                            j.Value = attributeValue;
                                            successMessage = "Successfully set " + attribute + " of " + node + " of " + member.Username + " to " + attributeValue;
                                            Console.WriteLine(j.Value);
                                            multiple = true;
                                        }
                                        else
                                        {
                                            Console.WriteLine("multiple");
                                            multipleFound = true;
                                        }
                                    }
                                }
                                else if (!isGettingByAttribute && !isSettingAttribute)
                                {
                                    IEnumerable<XElement> iNode = i.Descendants(node);
                                    foreach (XElement j in iNode)
                                    {
                                        Console.WriteLine("Found node");
                                        if (!multiple)
                                        {
                                            multiple = true;
                                            j.Value = targetValue;
                                            successMessage = "Successfully set " + node + " of " + member.Username + " to " + targetValue;
                                        }
                                        else
                                        {
                                            Console.WriteLine("multiple");
                                            multipleFound = true;
                                        }
                                    }
                                }
                                //
                                if (multipleFound)
                                {
                                    e.Channel.SendMessage("An error occured. Multiple nodes were found; please try being more specific.");
                                }
                                else
                                {
                                    e.Channel.SendMessage(successMessage);
                                    memberDBT.Save(Path.Combine(configDir, "PersonellDB.xml"));
                                }
                            }
                        }
                    }
                    else
                    {
                        e.Channel.SendMessage("Sorry, you don't seem to have permission to perform that action!");
                    }
                }
            }
        }
        public static void news(DiscordMessageEventArgs e)
        {
            using (StreamReader sr = File.OpenText(Path.Combine(configDir, "news.txt")))
            {
                string s = sr.ReadToEnd();
                e.Channel.SendMessage(s);
            }
        }
        public static void updateNews(DiscordMessageEventArgs e)
        {
            bool hasPermission = false;
            XDocument memberDB = XDocument.Load(Path.Combine(configDir, "PersonellDB.xml"));
            IEnumerable<XElement> permission = memberDB.Descendants("Rank").Where(x => x.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == e.Author.Username));
            foreach (XElement p in permission)
            {
                Console.WriteLine(p.Value);
                IEnumerable<XElement> hasPermissionNum = memberDB.Descendants("Define").Where(x => x.Attribute("name").Value == p.Value);
                foreach (XElement q in hasPermissionNum)
                {
                    if (Int32.Parse(q.Value) >= 5)
                    {
                        hasPermission = true;
                    }
                }
            }
            if (hasPermission)
            {
                string news = e.MessageText.Replace("!updateNews ", "");
                File.WriteAllText(Path.Combine(configDir, "news.txt"), DateTime.Now + " -- " + news);
                client.GetChannelByName("general").SendMessage("News Updated: " + news);
            }
        }
        public static void populate(DiscordMember member, DiscordMessageEventArgs e)
        {
            XMLDocument memberDB = new XMLDocument(Path.Combine(configDir, "PersonellDB.xml"));
            XElement[] memberArray = memberDB.document.Descendants("Member").ToArray();
            for(int i = 0;i<memberArray.Length;i++)
            {
                memberArray[i].Add(new XElement("FailedTrial", "false"));
            }
            Console.WriteLine(memberDB.document);
            memberDB.Save(Path.Combine(configDir, "PersonellDB.xml"));
        }
        public static void pmUpdateNews(DiscordMember member, DiscordPrivateMessageEventArgs e)
        {
            bool hasPermission = false;
            XDocument memberDB = XDocument.Load(Path.Combine(configDir, "PersonellDB.xml"));
            IEnumerable<XElement> permission = memberDB.Descendants("Rank").Where(x => x.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == member.Username));
            foreach (XElement p in permission)
            {
                Console.WriteLine(p.Value);
                IEnumerable<XElement> hasPermissionNum = memberDB.Descendants("Define").Where(x => x.Attribute("name").Value == p.Value);
                foreach (XElement q in hasPermissionNum)
                {
                    if (Int32.Parse(q.Value) >= 5)
                    {
                        hasPermission = true;
                    }
                }
            }
            if (hasPermission)
            {
                string news = e.Message.Replace("!updateNews ", "");
                File.WriteAllText(Path.Combine(configDir, "news.txt"), DateTime.Now + " -- " + news);
                e.Author.SendMessage("News Updated: " + news);
                client.GetChannelByName("general").SendMessage("News Updated: " + news);
            }
        }
    }
}
