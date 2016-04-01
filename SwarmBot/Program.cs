//Rename this file to Program.cs and replace the username and password with your own info.

using DiscordSharp;
using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.IO;
using System.Globalization;

namespace SwarmBot
{
    class Program
    {
        static bool SendInvite(string recipient, string Game, DiscordSharp.Events.DiscordMessageEventArgs e)
        {
            //SendInvite(firstMatch, fifthMatch, firstMatch.StartsWith("<@"), e);

            bool isTag = recipient.StartsWith("<@");

            if (isTag)
            {
                e.Channel.parent.members.Find(x => x.ID == recipient.Replace("<", "").Replace("@", "").Replace(">", "")).SendMessage("@" + e.message.author.Username + " has invited you to play: " + Game);
            }
            else {
                e.Channel.parent.members.Find(x => x.Username == recipient).SendMessage("@" + e.message.author.Username + " has invited you to play: " + Game);
            }

            return true;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Initializing SwarmBot...");
            XDocument configFile = XDocument.Load("config.xml");
            string email = configFile.Descendants("email").ToString();
            string password = configFile.Descendants("password").ToString();
            DiscordClient client = new DiscordClient();
            client.ClientPrivateInformation.email = email;
            client.ClientPrivateInformation.password = password;

            client.Connected += (sender, e) =>
            {
                Console.WriteLine("Connected! User: " + e.user.Username);
                client.UpdateCurrentGame("Type !help for help");
            };

            client.MessageReceived += (sender, e) =>
            {
                if (e.author.Username != "SwarmBot")
                {
                    if (e.message_text == "!help")
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
                        Console.WriteLine("Sent help because of this message: " + e.message_text);
                    }
                    else if (e.message_text.StartsWith("!wfwiki"))
                    {
                        string target = Regex.Match(e.message_text,
                            @"!wfwiki (.+)",
                            RegexOptions.Singleline)
                        .Groups[1].Value;
                        e.Channel.SendMessage("<@" + e.message.author.ID + "> http://warframe.wikia.com/wiki/" + target.Replace(" ", "_"));
                        Console.WriteLine("Sent " + target.Replace(" ", "_") + " to " + e.author.ID + " because of this message: " + e.message_text);
                    }
                    else if (e.message_text.StartsWith("!skwiki"))
                    {
                        string target = Regex.Match(e.message_text,
                            @"!skwiki (.+)",
                            RegexOptions.Singleline)
                            .Groups[1].Value;
                        e.Channel.SendMessage("<@" + e.message.author.ID + "> http://wiki.spiralknights.com/" + target.Replace(" ", "_"));
                        Console.WriteLine("Sent " + target.Replace(" ", "_") + " to " + e.author.Username + " because of this message: " + e.message_text);

                    }
                    else if (e.message_text.StartsWith("!guildmail"))
                    {
                        string guildmail = "https://onedrive.live.com/redir?resid=F485764E97178E7C!5421&authkey=!AC4Oi92whZUO3xo&ithint=file%2cpdf";
                        e.Channel.SendMessage(guildmail);
                    }
                    else if (e.message_text.StartsWith("!invite"))
                    {
                        string numInvitees = Regex.Match(e.message_text, @"([1-8])", RegexOptions.Singleline).Groups[1].Value;
                        if (numInvitees != "")
                        {
                            if (numInvitees == "1")
                            {
                                Match matches = Regex.Match(e.message_text, @"!invite (?:[1-8]) (?:@)?(.+) (.+)");
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
                                Match matches = Regex.Match(e.message_text, @"!invite (?:[1-8]) (?:@)?(.+) (?:@)?(.+) (.+)");
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
                                Match matches = Regex.Match(e.message_text, @"!invite (?:[1-8]) (?:@)?(.+) (?:@)?(.+) (?:@)?(.+) (.+)");
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
                                Match matches = Regex.Match(e.message_text, @"!invite (?:[1-8]) (?:@)?(.+) (?:@)?(.+) (?:@)?(.+) (?:@)?(.+) (.+)");
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
                    else if (e.message_text.StartsWith("!getMember"))
                    {
                        Match cmd = Regex.Match(e.message_text, @"!getMember (?:<@(.+)>)?(?: )?(?:(-f))?(?: )?(?:(-h))?");
                        string member = e.Channel.parent.members.Find(x => x.ID == cmd.Groups[1].Value).Username;
                        bool force = cmd.Groups[2].Value.Equals("-f");
                        bool help = cmd.Groups[3].Value.Equals("-h");

                        if (help)
                        {
                            e.Channel.SendMessage("Help text");
                        }
                        else {
                            if (force)
                            {
                                var destChannel = e.Channel;
                            }
                            try
                            {
                                Console.WriteLine(member + " " + force + " " + help);
                                XDocument memberDB = XDocument.Load("PersonellDB.xml");
                                IEnumerable<XElement> rank = memberDB.Descendants("Rank")
                                    .Where(x => x.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == member));
                                foreach (XElement h in rank)
                                {
                                    var hRank = h.Value;
                                    IEnumerable<XElement> namelist = h.Parent.Descendants("Name");
                                    string memberName = null;
                                    foreach (XElement name in namelist)
                                    {
                                        memberName = name.Value;
                                    }
                                    Console.WriteLine(memberName + " is a(n) " + hRank);
                                    e.Channel.SendMessage(memberName + " is a(n) " + hRank);
                                    IEnumerable<XElement> lastRankUp = memberDB.Descendants("RankupHistory").Descendants("Rankup")
                                        .Where(x => x.Parent.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == member))
                                        .Where(x => x.Attribute("name").Value == hRank);
                                    foreach (XElement i in lastRankUp)
                                    {
                                        var iLastRankUp = i.Value;
                                        Console.WriteLine(iLastRankUp);
                                        if (iLastRankUp == "Old")
                                        {
                                            e.Channel.SendMessage("We can't tell whether or not " + member + " is ready for an upgrade, but they probably are since our data dates back before this bot's conception.");
                                        }
                                        else {
                                            DateTime compareTo = DateTime.Parse(iLastRankUp);
                                            Console.WriteLine(compareTo);
                                            DateTime now = DateTime.Parse(DateTime.Now.ToString(new CultureInfo("en-us")));
                                            Console.WriteLine(now);
                                            double time = (now - compareTo).TotalDays;
                                            Console.WriteLine(time + " Days");
                                            IEnumerable<XElement> isEligible = memberDB.Descendants("Define").Where(x => x.Attribute("name").Value == hRank + "LastRankUp");
                                            foreach (XElement j in isEligible)
                                            {
                                                bool jIsEligible = Int32.Parse(j.Value) <= time;
                                                if (jIsEligible)
                                                {
                                                    Console.WriteLine("He/she is eligible for a rank up.");
                                                    e.Channel.SendMessage("He/she is eligible for a rank up.");
                                                }
                                                else
                                                {
                                                    e.Channel.SendMessage("He/she is not eligible for a rank up.");
                                                }
                                                e.Channel.SendMessage("It has been " + time + " days since their last rankup");
                                            }
                                        }
                                    }
                                }


                            }
                            catch (Exception)
                            {
                                Console.WriteLine("Error");
                            }
                        }
                    }
                    else if (e.message_text.StartsWith("!promote"))
                    {
                        Match cmd = Regex.Match(e.message_text, @"!promote <@(.+)>(?: -f \((.+)\))?(?: --date ([^ ]+)| -d ([^ ]+))?(?: (-h))?");
                        DiscordMember member = e.Channel.parent.members.Find(x => x.ID == cmd.Groups[1].Value);
                        string force = cmd.Groups[2].Value;
                        string date = "";
                        if (cmd.Groups[3].Value != "")
                        {
                            date = cmd.Groups[3].Value;
                        } else if (cmd.Groups[4].Value != "")
                        {
                            date = cmd.Groups[4].Value;
                        }
                        bool isForce = force != "";
                        bool help = cmd.Groups[3].Value.Equals("-h");

                        if (!help)
                        {
                            Console.WriteLine(member.Username + " " + force + " " + help);
                            XDocument memberDB = XDocument.Load("PersonellDB.xml");
                            IEnumerable<XElement> hasPermission = memberDB.Descendants("Rank").Where(x => x.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == e.author.Username));
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
                                                    try { e.Channel.parent.AssignRoleToMember(e.Channel.parent.roles.Find(x => x.name == jToRank), member); } catch (Exception) { Console.WriteLine("No Rank exists"); };
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
                                                    memberDB.Save("PersonellDB.xml");
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
                        }
                    }
                    else if (e.message_text.StartsWith("!createMember"))
                    {
                        Match cmd = Regex.Match(e.message_text, @"!createMember <@(.+)>(?: --date ([^ ]+)| -d ([^ ]+))?(?: --steam ([^ 0-9]+)| -s ([^ 0-9]+)| --steam ([^ ][0-9]+)| -s ([^ ][0-9]+))?( --populate| -p)?");
                        DiscordMember member = e.Channel.parent.members.Find(x => x.ID == cmd.Groups[1].Value);
                        Console.WriteLine(cmd.Groups[1].Value);
                        bool isSettingSteam = false;
                        bool isSteamNumerical = false;
                        bool isSettingDate = false;
                        string date = "";
                        string steamId = "";
                        if (cmd.Groups[2].Value != "")
                        {
                            isSettingDate = true;
                            date = cmd.Groups[2].Value;
                        } else if (cmd.Groups[3].Value != "")
                        {
                            isSettingDate = true;
                            date = cmd.Groups[3].Value;
                        }

                        if (cmd.Groups[4].Value != "")
                        {
                            isSettingSteam = true;
                            isSteamNumerical = false;
                            steamId = cmd.Groups[4].Value;
                        }
                        else if (cmd.Groups[5].Value != "")
                        {
                            isSettingSteam = true;
                            isSteamNumerical = false;
                            steamId = cmd.Groups[5].Value;
                        }
                        else if (cmd.Groups[6].Value != "")
                        {
                            isSettingSteam = true;
                            isSteamNumerical = true;
                            steamId = cmd.Groups[6].Value;
                        } else if (cmd.Groups[7].Value != "")
                        {
                            isSettingSteam = true;
                            isSteamNumerical = true;
                            steamId = cmd.Groups[7].Value;
                        }
                        XDocument memberDB = XDocument.Load("PersonellDB.xml");
                        IEnumerable<XElement> hasPermission = memberDB.Descendants("Rank").Where(x => x.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == e.author.Username));
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
                                    XDocument memberDBT = XDocument.Load("PersonellDB.xml");
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
                                                    memberDBT.Save("PersonellDB.xml");
                                                }
                                            }
                                            if (!jExists)
                                            {
                                                DateTime now = DateTime.Parse(DateTime.Now.ToString(new CultureInfo("en-us")));

                                                h.Add(
                                                    new XElement("Member",
                                                        new XElement("Name", member.Username),
                                                        new XElement("Rank", "Recruit"),
                                                        new XElement("RankupHistory",
                                                            new XElement("Rankup", new XAttribute("name", "Recruit"), Regex.Match(now.ToString(), @"(.+) [1-9]+:[0-9]+:[0-9]+ .M").Groups[1].Value),
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
                                                    IEnumerable<XElement> i = h.Descendants("Rankup").Where(x => x.Attribute("name").Value == "Recruit");
                                                    foreach (XElement ii in i) { ii.SetValue(date); };
                                                }
                                                Console.WriteLine(h);
                                                Console.WriteLine("Created");
                                                memberDBT.Save("PersonellDB.xml");
                                                e.Channel.SendMessage("Successfully created member " + member.Username);
                                            } else
                                            {
                                                e.Channel.SendMessage("An error occured, that member already exists!");
                                                Console.WriteLine("exists");
                                            }

                                        } else
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
                    else if (e.message_text.StartsWith("!updateMember"))
                    {
                        try {
                            Match cmd = Regex.Match(e.message_text, @"!updateMember <@(.+)>(?: ([^ .:]+)(?:\.([^ ]+) \((.+)\))?(?:\:([^ ]+))? (?:\((.+)\)))?");
                            DiscordMember member = e.Channel.parent.members.Find(x => x.ID == cmd.Groups[1].Value);
                            for (int i = 1; i <= 6; i++)
                            {
                                Console.WriteLine(i.ToString() + ": " + cmd.Groups[i].Value);
                            }
                            string node = cmd.Groups[2].Value;
                            string targetValue = null;
                            string attribute = null;
                            string attributeValue = null;
                            bool isSettingAttribute = false;
                            bool isGettingByAttribute = false;
                            if (cmd.Groups[3].Value != "") //Getting by Attribute (ex Rankup)
                            {
                                isGettingByAttribute = true;
                                attribute = cmd.Groups[3].Value;
                                attributeValue = cmd.Groups[4].Value;
                                targetValue = cmd.Groups[6].Value;
                                Console.WriteLine("Getting");
                            } else if (cmd.Groups[5].Value != "") //Setting an Attribute (ex SteamId numerical)
                            {
                                isSettingAttribute = true;
                                attribute = cmd.Groups[5].Value;
                                attributeValue = cmd.Groups[6].Value;
                                Console.WriteLine(cmd.Groups[5].Value);
                                Console.WriteLine("Setting");
                            } else //Neither (ex Name)
                            {
                                targetValue = cmd.Groups[6].Value;
                                Console.WriteLine("else");
                            }
                            XDocument memberDB = XDocument.Load("PersonellDB.xml");
                            IEnumerable<XElement> hasPermission = memberDB.Descendants("Rank").Where(x => x.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == e.author.Username));
                            foreach (XElement p in hasPermission)
                            {
                                Console.WriteLine(p.Value);
                                IEnumerable<XElement> hasPermissionNum = memberDB.Descendants("Define").Where(x => x.Attribute("name").Value == p.Value);
                                foreach (XElement q in hasPermissionNum)
                                {
                                    if (Int32.Parse(q.Value) >= 5)
                                    {
                                        Console.WriteLine(q.Value);
                                        XDocument memberDBT = XDocument.Load("PersonellDB.xml");
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
                                                        } else
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
                                                        } else
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
                                                    memberDBT.Save("PersonellDB.xml");
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
                        } catch (Exception)
                        {
                            e.Channel.SendMessage("Something went wrong. Make sure you're tagging the member and using correct syntax.");
                        }
                    }
                    else if (e.message_text == "!news")
                    {
                        using (StreamReader sr = File.OpenText("news.txt"))
                        {
                            string s = sr.ReadToEnd();
                            e.Channel.SendMessage(s);
                        }
                    }
                    else if (e.message_text.StartsWith("!updateNews"))
                    {
                        bool hasPermission = false;
                        XDocument memberDB = XDocument.Load("PersonellDB.xml");
                        IEnumerable<XElement> permission = memberDB.Descendants("Rank").Where(x => x.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == e.author.Username));
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
                        if(hasPermission)
                        {
                            string news = e.message_text.Replace("!updateNews ", "");
                            System.IO.File.WriteAllText("news.txt", DateTime.Now + " -- " + news);
                            e.Channel.SendMessage("News Updated: " + news);
                        }
                    }
                    else if (e.message_text.StartsWith("!populate"))
                    {
                        Console.WriteLine("hi");
                        XDocument memberDB = XDocument.Load("PersonellDB.xml");
                        IEnumerable<XElement> memberToPopulate = memberDB.Descendants("Member").Where(x => x.Descendants("Names").Descendants("Discord").Any(y => y.Value == e.Channel.parent.members.Find(z => z.ID == Regex.Match(e.message_text.Replace("!populate ", ""), @"<@(.+)>").Groups[1].Value).Username));
                        foreach (XElement h in memberToPopulate)
                        {
                            Console.WriteLine("Found member");
                            IEnumerable<XElement> memberId = h.Descendants("DiscordId");
                            foreach(XElement i in memberId)
                            {
                                i.SetValue(Regex.Match(e.message_text.Replace("!populate ", ""), @"<@(.+)>").Groups[1].Value);
                                memberDB.Save("PersonellDB.xml");
                                Console.WriteLine("succeeded");
                            }
                        }
                    }
                };
            };
                client.PrivateMessageReceived += (sender, e) =>
                {
                    Console.WriteLine(e.author);
                    if(e.message.StartsWith("!updateNews"))
                    {
                        bool hasPermission = false;
                        XDocument memberDB = XDocument.Load("PersonellDB.xml");
                        IEnumerable<XElement> permission = memberDB.Descendants("Rank").Where(x => x.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == e.author.Username));
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
                            string news = e.message.Replace("!updateNews ", "");
                            System.IO.File.WriteAllText("news.txt", DateTime.Now + " -- " + news);
                            e.author.SendMessage("News Updated: " + news);
                        }
                    }
                };
                try
                {
                    // Make sure that IF something goes wrong, the user will be notified.
                    // The SendLoginRequest should be called after the events are defined, to prevent issues.
                    client.SendLoginRequest();
                    client.Connect(); // Login request, and then connect using the discordclient i just made.
                }
                catch (Exception e)
                {
                    Console.WriteLine("Something went wrong!\n" + e.Message + "\nPress any key to close this window.");
                }

                Console.ReadKey();
        }
    }
}
