//Rename this file to Program.cs and replace the username and password with your own info.

using DiscordSharp;
using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using SwarmBot.Chat;

namespace SwarmBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing SwarmBot...");
            Discord.Connect();

            Discord.client.Connected += (sender, e) =>
            {
                Console.WriteLine("Connected! User: " + e.User.Username);
                Discord.client.UpdateCurrentGame("Type !help for help", true, "https://github.com/SpawnerSwarm/SwarmBot");
            };

            //Emotes
            string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SwarmBot\\");
            string emoteDir = Path.Combine(configDir, "emotes.xml");
            string restarterPath = "";
            using (StreamReader sr = File.OpenText(Path.Combine(configDir, "config.txt")))
            {
                Match info = Regex.Match(sr.ReadToEnd(), @"(.+);(.+);(.+);(.+);(.+)");
                restarterPath = info.Groups[5].Value;
            }

            Discord.client.SocketClosed += (sender, e) =>
            {
                int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo = new System.Diagnostics.ProcessStartInfo(restarterPath, pid.ToString())
                {
                    UseShellExecute = false
                };
                p.Start();
            };

            Discord.client.MessageReceived += (sender, e) =>
            {
                if (e.Author.Username != "SwarmBot")
                {
                    if (e.MessageText == "!help" || e.MessageText == "!OHMAHGAWDHALPMEPLS" || e.MessageText == "!ONOIAMNOTGOODWITHCOMPOOTERPLSTOHALP")
                    {
                        Discord.help(e);
                    }
                    else if (e.MessageText.StartsWith("!wfwiki"))
                    {
                        Discord.wiki(e, "wf");
                    }
                    else if (e.MessageText.StartsWith("!skwiki"))
                    {
                        Discord.wiki(e, "sk");
                    }
                    else if (e.MessageText.StartsWith("!guildmail"))
                    {
                        string guildmail = "https://onedrive.live.com/redir?resid=EB28C1A942749087!7909&authkey=!AEcoJaL5hNrQgXE&ithint=file%2cpdf";
                        e.Channel.SendMessage(guildmail);
                    }
                    else if (e.MessageText.StartsWith("!invite"))
                    {
                        //Discord.invite(e);
                        MatchCollection cmd = Regex.Matches(e.MessageText, @"(?:!invite)? <@((?:!)?\d+)>( [^<@>]+)?");
                        Console.WriteLine(cmd[0].Groups[1].Value);
                        Console.WriteLine(cmd[cmd.Count - 1].Groups[2].Value);
                        string inviteSubject = Regex.Match(cmd[cmd.Count - 1].Groups[2].Value, @" (.+)").Groups[1].Value;
                        string[] memberKeys = new string[cmd.Count];
                        for(int i = 0; i < cmd.Count; i++)
                        {
                            memberKeys[i] = cmd[i].Groups[1].Value;
                        }
                        Discord.invite(e, cmd, inviteSubject, memberKeys);
                    }
                    else if (e.MessageText.StartsWith("!getMember"))
                    {
                        Match cmd = Regex.Match(e.MessageText, @"!getMember (?:<@(.+)>)?(?: )?(?:(-f))?(?: )?(?:(-h))?");
                        //DiscordMember member = e.Channel.Parent.GetMemberByKey(cmd.Groups[1].Value.Replace("!", ""));
                        DiscordMember member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, e.Channel.Parent);
                        bool force = cmd.Groups[2].Value.Equals("-f");
                        bool help = cmd.Groups[3].Value.Equals("-h");

                        Discord.getMember(member, e, force, help);
                    }
                    else if (e.MessageText.StartsWith("!promote"))
                    {
                        Match cmd = Regex.Match(e.MessageText, @"!promote <@([^ ]+)>(?:(?: --force | -f )\((.+)\))?(?:(?: --date | -d )([^ ]+))?(?: (-h))?");
                        if (cmd.Groups[1].Value != "")
                        {
                            DiscordMember member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, e.Channel.Parent);
                            string force = cmd.Groups[2].Value;
                            string date = cmd.Groups[3].Value;
                            bool isForce = force != "";
                            bool help = cmd.Groups[3].Value.Equals("-h");

                            Discord.promote(member, e, force, date, isForce, help);
                        } else
                        {
                            e.Channel.SendMessage("Error: Incorrect syntax");
                        }
                    }
                    else if (e.MessageText.StartsWith("!createMember"))
                    {
                        Match cmd = Regex.Match(e.MessageText, @"!createMember <@(.+)>(?: --date ([^ ]+)| -d ([^ ]+))?(?: --steam ([^ 0-9]+)| -s ([^ 0-9]+)| --steam ([^ ][0-9]+)| -s ([^ ][0-9]+))?( --populate| -p)?");
                        DiscordMember member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, e.Channel.Parent);
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

                        Discord.createMember(member, e, isSettingSteam, isSteamNumerical, isSettingDate, date, steamId);
                    }
                    else if (e.MessageText.StartsWith("!updateMember"))
                    {
                        try {
                            Match cmd = Regex.Match(e.MessageText, @"!updateMember <@(.+)>(?: ([^ .:]+)(?:\.([^ ]+) \((.+)\))?(?:\:([^ ]+))? (?:\((.+)\)))?");
                            DiscordMember member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, e.Channel.Parent);
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

                            Discord.updateMember(member, e, node, targetValue, attribute, attributeValue, isSettingAttribute, isGettingByAttribute);
                        } catch (Exception)
                        {
                            e.Channel.SendMessage("Something went wrong. Make sure you're tagging the member and using correct syntax.");
                        }
                    }
                    else if (e.MessageText == "!news")
                    {
                        Discord.news(e);
                    }
                    else if (e.MessageText.StartsWith("!updateNews"))
                    {
                        Discord.updateNews(e);
                    }
                    else if (e.MessageText.StartsWith("!populate"))
                    {
                        //DiscordMember member = e.Channel.Parent.GetMemberByKey(Regex.Match(e.MessageText.Replace("!populate ", ""), @"<@(.+)>").Groups[1].Value);
                        DiscordMember member = Discord.getDiscordMemberByID(Regex.Match(e.MessageText.Replace("!populate ", ""), @"<@(.+)>").Groups[1].Value, e.Channel.Parent);
                        Discord.populate(member, e);
                    }
                    else if(e.MessageText.StartsWith("!lenny"))
                    {
                        if(e.Author.Username == "FoxTale")
                        {
                            e.Channel.SendMessage("http://i.imgur.com/fAEYhFH.gifv");
                        }
                        else if(e.Author.Username == "Mardan")
                        {
                            e.Channel.SendMessage("( ͡◉ ͜ʖ ͡◉)﻿");
                        }
                        else
                        {
                            e.Channel.SendMessage("( ͡° ͜ʖ ͡°)");
                        }                        
                    }
                    else if(e.MessageText.StartsWith("!brutal"))
                    {
                        if(e.Author.Username == "FoxTale")
                        {
                            e.Channel.SendMessage("http://i.imgur.com/Vw20PUI.png");
                        } else
                        {
                            e.Channel.SendMessage("http://i.imgur.com/0eMrMLd.jpg");
                        }
                    }
                    else if(e.MessageText.StartsWith("(╯°□°）╯︵ ┻━┻"))
                    {
                        e.Channel.SendMessage("┬─┬﻿ ノ( ゜-゜ノ)");
                    }
                    else if(e.MessageText.StartsWith("!warframemarket") || e.MessageText.StartsWith("!wfmarket") || e.MessageText.StartsWith("!wfm"))
                    {
                        e.Channel.SendMessage("http://warframe.market");
                    }
                    else if(e.MessageText.StartsWith("!createChannel"))
                    {
                        Match cmd = Regex.Match(e.MessageText, @"!createChannel ([^< ]+)(?: <@([^>]+)>)?(?: <@([^>]+)>)?(?: <@([^>]+)>)?");
                        string channelName = cmd.Groups[1].Value;
                        string[] invitees = new string[3];
                        int numInvitees = 0;
                        for(int i = 3; i < 6; i++)
                        {
                            if(cmd.Groups[i].Value != "")
                            {
                                numInvitees++;
                                invitees[i - 3] = cmd.Groups[i].Value;
                            }
                        }
                        Discord.createChannel(e, channelName, numInvitees, invitees);
                    }
                    else if(e.MessageText.StartsWith("!emotes") || e.MessageText.StartsWith("!e ") || e.MessageText == "!e")
                    {
                        Emotes emotes = new Emotes(emoteDir);
                        string cmd = e.MessageText.Replace("!emotes", "").Replace("!e", "").Replace(" ", "");
                        if (cmd == "")
                        {
                            e.Channel.SendMessage("Welcome to the Spawner Swarm Emotes system (beta)! To send an emote, send a message like this '!e <emote_ref>'. To see a list of emotes, send '!emotes list' or '!e list'");
                        }
                        else
                        {
                            Discord.getEmote(e, emotes, cmd);
                        }
                    }
                    else if(e.MessageText.StartsWith("!newEmote"))
                    {
                        Emotes emotes = new Emotes(emoteDir);
                        Match cmd = Regex.Match(e.MessageText, @"!newEmote \((.+)\) ([^ ]+) ([^ ]+) ([^ ]+)(?: <@([^ ]+)>)?");
                        string name = cmd.Groups[1].Value;
                        string reference = cmd.Groups[2].Value;
                        short reqRank = short.Parse(cmd.Groups[3].Value);
                        string URL = cmd.Groups[4].Value;
                        if(cmd.Groups[5].Value != "")
                        {
                            //string author = e.Channel.Parent.GetMemberByKey(cmd.Groups[5].Value).Username;
                            string author = Discord.getDiscordMemberByID(cmd.Groups[5].Value, e.Channel.Parent).Username;
                            Discord.createEmote(e, emotes, name, URL, reference, reqRank, author);
                        }
                        else
                        {
                            Discord.createEmote(e, emotes, name, URL, reference, reqRank);
                        }
                        
                    }
                    else if(e.MessageText.StartsWith("!addForma"))
                    {
                        Match cmd = Regex.Match(e.MessageText, @"!addForma <@(.+)> ([1-9])");
                        if (cmd.Groups[1].Value != "" && cmd.Groups[2].Value != "")
                        {
                            DiscordMember member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, e.Channel.Parent);
                            short formas = short.Parse(cmd.Groups[2].Value);
                            if (formas == 0)
                            {
                                e.Channel.SendMessage("Must donate between 0 and 10 formas at a time (not inclusive)");
                            }
                            else
                            {
                                Discord.addForma(e, member, formas);
                            }
                        } else
                        {
                            e.Channel.SendMessage("Error: Incorrect syntax");
                        }
                    }
                    else if(e.MessageText.StartsWith("!info"))
                    {
                        string member = Regex.Match(e.MessageText, @"!info <@(.+)>").Groups[1].Value;
                        //DiscordMember m = e.Channel.Parent.GetMemberByKey(member);
                        DiscordMember m = Discord.getDiscordMemberByID(member, e.Channel.Parent);
                        Console.WriteLine(m.ID);
                        Console.WriteLine(m.Username);
                        Console.WriteLine(m.Roles);
                    }
                };
            };
                Discord.client.PrivateMessageReceived += (sender, e) =>
                {
                    Console.WriteLine(e.Author);
                    if(e.Message.StartsWith("!updateNews"))
                    {
                        Discord.pmUpdateNews(e.Author, e);
                    }
                };

                Console.ReadKey();
        }
    }
}
