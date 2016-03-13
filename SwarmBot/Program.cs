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
        public struct DateTimeSpan
        {
            private readonly int years;
            private readonly int months;
            private readonly int days;
            private readonly int hours;
            private readonly int minutes;
            private readonly int seconds;
            private readonly int milliseconds;

            public DateTimeSpan(int years, int months, int days, int hours, int minutes, int seconds, int milliseconds)
            {
                this.years = years;
                this.months = months;
                this.days = days;
                this.hours = hours;
                this.minutes = minutes;
                this.seconds = seconds;
                this.milliseconds = milliseconds;
            }

            public int Years { get { return years; } }
            public int Months { get { return months; } }
            public int Days { get { return days; } }
            public int Hours { get { return hours; } }
            public int Minutes { get { return minutes; } }
            public int Seconds { get { return seconds; } }
            public int Milliseconds { get { return milliseconds; } }

            enum Phase { Years, Months, Days, Done }

            public static DateTimeSpan CompareDates(DateTime date1, DateTime date2)
            {
                if (date2 < date1)
                {
                    var sub = date1;
                    date1 = date2;
                    date2 = sub;
                }

                DateTime current = date1;
                int years = 0;
                int months = 0;
                int days = 0;

                Phase phase = Phase.Years;
                DateTimeSpan span = new DateTimeSpan();

                while (phase != Phase.Done)
                {
                    switch (phase)
                    {
                        case Phase.Years:
                            if (current.AddYears(years + 1) > date2)
                            {
                                phase = Phase.Months;
                                current = current.AddYears(years);
                            }
                            else
                            {
                                years++;
                            }
                            break;
                        case Phase.Months:
                            if (current.AddMonths(months + 1) > date2)
                            {
                                phase = Phase.Days;
                                current = current.AddMonths(months);
                            }
                            else
                            {
                                months++;
                            }
                            break;
                        case Phase.Days:
                            if (current.AddDays(days + 1) > date2)
                            {
                                current = current.AddDays(days);
                                var timespan = date2 - current;
                                span = new DateTimeSpan(years, months, days, timespan.Hours, timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
                                phase = Phase.Done;
                            }
                            else
                            {
                                days++;
                            }
                            break;
                    }
                }

                return span;
            }
        }
        static void Main(string[] args)
        {
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
                        e.Channel.SendMessage("--   BETA Invite a group of players to play a game (!invite <Number of invitees> <Discord username 1>, [Discord username 2], [Discord username 3], [Discord username 4] <Game name>");
                        e.Channel.SendMessage("More functions will be added soon; feel free to pm @Mardan with suggestions!");
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
                            if(numInvitees == "1")
                            {
                                var matches = Regex.Match(e.message_text, @"!invite (?:[1-8]) (?:@)?(.+) (.+)");
                                string firstMatch = matches.Groups[1].Value;
                                string secondMatch = matches.Groups[2].Value;
                                /*Console.WriteLine(firstMatch);
                                Console.WriteLine(secondMatch);*/
                                // In the future, this will also dispatch a steam message.
                                try
                                {
                                    if (firstMatch.StartsWith("<@"))
                                    {
                                        e.Channel.parent.members.Find(x => x.ID == firstMatch.Replace("<", "").Replace("@", "").Replace(">", "")).SendMessage("@" + e.message.author.Username + " has invited you to play: " + secondMatch);
                                    }
                                    else {
                                        e.Channel.parent.members.Find(x => x.Username == firstMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + secondMatch);
                                    }
                                }
                                catch (Exception)
                                {
                                    e.Channel.SendMessage("Sorry, something went wrong. Perhaps your syntax is off?");
                                }
                            }
                            else if(numInvitees == "2")
                            {
                                var matches = Regex.Match(e.message_text, @"!invite (?:[1-8]) (?:@)?(.+) (?:@)?(.+) (.+)");
                                string firstMatch = matches.Groups[1].Value;
                                string secondMatch = matches.Groups[2].Value;
                                string thirdMatch = matches.Groups[3].Value;
                                try {
                                    if (firstMatch.StartsWith("<@"))
                                    {
                                        e.Channel.parent.members.Find(x => x.ID == firstMatch.Replace("<", "").Replace("@", "").Replace(">", "")).SendMessage("@" + e.message.author.Username + " has invited you to play: " + thirdMatch);
                                    }
                                    else {
                                        e.Channel.parent.members.Find(x => x.Username == firstMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + thirdMatch);
                                    }
                                    if (secondMatch.StartsWith("<@"))
                                    {
                                        e.Channel.parent.members.Find(x => x.ID == secondMatch.Replace("<", "").Replace("@", "").Replace(">", "")).SendMessage("@" + e.message.author.Username + " has invited you to play: " + thirdMatch);
                                    }
                                    else {
                                        e.Channel.parent.members.Find(x => x.Username == secondMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + thirdMatch);
                                    }
                                }
                                catch (Exception)
                                {
                                    e.Channel.SendMessage("Sorry, something went wrong. Perhaps your syntax is off?");
                                }
                            }
                            else if(numInvitees == "3")
                            {
                                var matches = Regex.Match(e.message_text, @"!invite (?:[1-8]) (?:@)?(.+) (?:@)?(.+) (?:@)?(.+) (.+)");
                                string firstMatch = matches.Groups[1].Value;
                                string secondMatch = matches.Groups[2].Value;
                                string thirdMatch = matches.Groups[3].Value;
                                string fourthMatch = matches.Groups[4].Value;
                                try {
                                    if (firstMatch.StartsWith("<@"))
                                    {
                                        e.Channel.parent.members.Find(x => x.ID == firstMatch.Replace("<", "").Replace("@", "").Replace(">", "")).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fourthMatch);
                                    }
                                    else {
                                        e.Channel.parent.members.Find(x => x.Username == firstMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fourthMatch);
                                    }
                                    if (firstMatch.StartsWith("<@"))
                                    {
                                        e.Channel.parent.members.Find(x => x.ID == secondMatch.Replace("<", "").Replace("@", "").Replace(">", "")).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fourthMatch);
                                    }
                                    else {
                                        e.Channel.parent.members.Find(x => x.Username == secondMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fourthMatch);
                                    }
                                    if (firstMatch.StartsWith("<@"))
                                    {
                                        e.Channel.parent.members.Find(x => x.ID == thirdMatch.Replace("<", "").Replace("@", "").Replace(">", "")).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fourthMatch);
                                    }
                                    else {
                                        e.Channel.parent.members.Find(x => x.Username == thirdMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fourthMatch);
                                    }
                                }
                                catch (Exception)
                                {
                                    e.Channel.SendMessage("Sorry, something went wrong. Perhaps your syntax is off?");
                                }
                            }
                            else if(numInvitees == "4")
                            {
                                var matches = Regex.Match(e.message_text, @"!invite (?:[1-8]) (?:@)?(.+) (?:@)?(.+) (?:@)?(.+) (?:@)?(.+) (.+)");
                                string firstMatch = matches.Groups[1].Value;
                                string secondMatch = matches.Groups[2].Value;
                                string thirdMatch = matches.Groups[3].Value;
                                string fourthMatch = matches.Groups[4].Value;
                                string fifthMatch = matches.Groups[5].Value;

                                try
                                {
                                    e.Channel.parent.members.Find(x => x.Username == firstMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fifthMatch);
                                    e.Channel.parent.members.Find(x => x.Username == secondMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fifthMatch);
                                    e.Channel.parent.members.Find(x => x.Username == thirdMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fifthMatch);
                                    e.Channel.parent.members.Find(x => x.Username == fourthMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fifthMatch);
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
                        } else
                        {
                            e.Channel.SendMessage("Error: Number of invitees not specified.");
                        };
                    }
                    else if(e.message_text.StartsWith("!getMember"))
                    {
                        Match cmd = Regex.Match(e.message_text, @"!getMember (?:<@(.+)>)?(?: )?(?:(-f))?(?: )?(?:(-h))?");
                        string member = e.Channel.parent.members.Find(x => x.ID == cmd.Groups[1].Value).Username;
                        bool force = cmd.Groups[2].Value.Equals("-f");
                        bool help = cmd.Groups[3].Value.Equals("-h");

                        if(help)
                        {
                            e.Channel.SendMessage("Help text");
                        } else {
                            if (force)
                            {
                                var destChannel = e.Channel;
                            }
                            try
                            {
                                Console.WriteLine(member + " " + force + " " + help);
                                XDocument memberDB = XDocument.Load("PersonellDB.xml");
                                var rank = memberDB.Descendants("Rank")
                                    .Where(x => x.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == member));
                                    foreach(var h in rank)
                                    {
                                        var hRank = h.Value;
                                        Console.WriteLine(member + " is a(n) " + hRank);
                                    e.Channel.SendMessage(member + " is a(n)" + hRank);
                                        var lastRankUp = memberDB.Descendants("RankupHistory").Descendants("Rankup")
                                            .Where(x => x.Parent.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == member))
                                            .Where(x => x.Attribute("name").Value == hRank);
                                    foreach (var i in lastRankUp)
                                    {
                                        var iLastRankUp = i.Value;
                                        Console.WriteLine(iLastRankUp);
                                        if (iLastRankUp == "Old")
                                        {
                                            e.Channel.SendMessage("We can't tell whether or not " + member + " is ready for an upgrade, but they probably are since our data dates back before this bot's conception.");
                                        }
                                        else {
                                            DateTime compareTo = DateTime.Parse(iLastRankUp);
                                            DateTime now = DateTime.Parse(DateTime.Now.ToString(new CultureInfo("en-us")));
                                            var dateSpan = DateTimeSpan.CompareDates(compareTo, now);
                                            int yrs = dateSpan.Years * 12;
                                            int mnths = dateSpan.Months * 30;
                                            Console.WriteLine(yrs + mnths);
                                            var isEligible = memberDB.Descendants("Define").Where(x => x.Attribute("name").Value == hRank + "LastRankUp");
                                            foreach (var j in isEligible)
                                            {
                                                bool jIsEligible = Int32.Parse(j.Value) <= yrs + mnths;
                                                if (jIsEligible)
                                                {
                                                    Console.WriteLine("He/she is eligible for a rank up.");
                                                    e.Channel.SendMessage("He/she is eligible for a rank up.");
                                                }
                                                else
                                                {
                                                    e.Channel.SendMessage("He/she is not eligible for a rank up.");
                                                }
                                            }
                                        }    
                                    }
                                }
                                

                            } catch (Exception)
                            {
                                Console.WriteLine("Error");
                            }
                        }
                    }
                    else if(e.message_text.StartsWith("!promote"))
                    {
                        Match cmd = Regex.Match(e.message_text, @"!promote <@(.+)>(?: -f \((.+)\))?(?: (-h))?");
                        var member = e.Channel.parent.members.Find(x => x.ID == cmd.Groups[1].Value);
                        string force = cmd.Groups[2].Value;
                            bool isForce = force != "";
                        bool help = cmd.Groups[3].Value.Equals("-h");

                        if(!help)
                        {
                            Console.WriteLine(member.Username + " " + force + " " + help);
                            XDocument memberDB = XDocument.Load("PersonellDB.xml");


                            var rank = memberDB.Descendants("Rank")
                                    .Where(x => x.Parent.Descendants("Names").Descendants("Discord").Any(y => y.Value == member.Username));
                            foreach (var h in rank)
                            {
                                var hRank = h.Value;
                                var hRankId = memberDB.Descendants("Define")
                                    .Where(x => x.Attribute("name").Value == hRank);
                                    foreach (var i in hRankId)
                                    {
                                        Console.WriteLine(i.Value);
                                        Console.WriteLine(Int32.Parse(i.Value));
                                        int iHRankId = Int32.Parse(i.Value);
                                        
                                    var iHRankIdProgressed = iHRankId + 1;
                                    var toRank = memberDB.Descendants("Define")
                                        .Where(x => x.Value == iHRankIdProgressed.ToString());
                                    foreach (var j in toRank)
                                    {
                                        string jToRank = force;
                                        if(jToRank == "")
                                        {
                                            jToRank = j.Attribute("name").Value;
                                        }
                                        Console.WriteLine(jToRank);
                                        e.Channel.parent.AssignRoleToMember(e.Channel.parent.roles.Find(x => x.name == jToRank), member);
                                        h.SetValue(jToRank);
                                        e.Channel.SendMessage("Successfully promoted " + member.Username + " to " + jToRank);
                                        
                                    }

                                }
                            }
                        }
                    }
                }

            };
            client.PrivateMessageReceived += (sender, e) =>
            {
                Console.WriteLine(e.author);
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
