//Rename this file to Program.cs and replace the username and password with your own info.

//using DiscordSharp;
//using DiscordSharp.Objects;
using System;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using SwarmBot.Chat;
using Trileans;

namespace SwarmBot
{
    class Program
    {
        public static NexusStats nexus;
        public static System.Windows.Forms.NotifyIcon trayIcon;

        static void Main(string[] args)
        {
            Discord.client = new DiscordClient();
            Console.WriteLine("Initializing SwarmBot...");
            //Discord.Connect();

            nexus = new NexusStats();
            nexus.Connect();

            //Emotes
            string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SwarmBot\\");
            string emoteDir = Path.Combine(configDir, "emotes.xml");
            string eventDir = Path.Combine(configDir, "events.xml");
            string restarterPath = "";
            using (StreamReader sr = File.OpenText(Path.Combine(configDir, "config.txt")))
            {
                Match info = Regex.Match(sr.ReadToEnd(), @"(.+);(.+);(.+);(.+);(.+)");
                restarterPath = info.Groups[5].Value;
            }

            /*Discord.client.SocketClosed += (sender, e) =>
            {
                int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo = new System.Diagnostics.ProcessStartInfo(restarterPath, pid.ToString())
                {
                    UseShellExecute = false
                };
                p.Start();
            };*/
            Discord.client.Ready += (sender, e) =>
            {
                Console.WriteLine("Connected! User: " + Discord.client.CurrentUser.Name);
                Discord.client.SetGame("Type !help for help");

                trayIcon = new System.Windows.Forms.NotifyIcon();
                trayIcon.Text = "SwarmBot";
                trayIcon.Icon = new System.Drawing.Icon(Path.Combine(configDir, "Swarm.ico"), 40, 40);

                System.Windows.Forms.ContextMenu trayMenu = new System.Windows.Forms.ContextMenu();

                trayMenu.MenuItems.Add("Open Configuration Directory", (object sSender, EventArgs eE) =>
                {
                    System.Diagnostics.Process.Start(configDir);
                });
                trayMenu.MenuItems.Add("Exit", (object sSender, EventArgs eE) =>
                {
                    Discord.client.Disconnect();
                    trayIcon.Dispose();
                    Environment.Exit(0);
                });

                trayIcon.ContextMenu = trayMenu;
                trayIcon.Visible = true;
                System.Windows.Forms.Application.Run();
            };
            Discord.client.MessageReceived += async (sender, e) =>
            {
                if (!e.Message.IsAuthor)
                {
                    Discord.client.SetGame("Type !help for help");
                    if (e.Message.Text == "!help" || e.Message.Text == "!OHMAHGAWDHALPMEPLS" || e.Message.Text == "!ONOIAMNOTGOODWITHCOMPOOTERPLSTOHALP")
                    {
                        Discord.help(e);
                    }
                    else if (e.Message.Text.StartsWith("!wfwiki"))
                    {
                        Discord.wiki(e, "wf");
                    }
                    else if (e.Message.Text.StartsWith("!skwiki"))
                    {
                        Discord.wiki(e, "sk");
                    }
                    else if (e.Message.Text.StartsWith("!guildmail"))
                    {
                        string guildmail = "https://1drv.ms/b/s!AnyOF5dOdoX0v0iXHyVMBfggyOqy";
                        await e.Channel.SendMessage(guildmail);
                    }
                    else if (e.Message.Text.StartsWith("!invite") && !e.Channel.IsPrivate)
                    {
                        //Discord.invite(e);
                        MatchCollection cmd = Regex.Matches(e.Message.RawText, @"(?:!invite)? <@((?:!)?\d+)>( [^<@>]+)?");
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
                    else if (Regex.IsMatch(e.Message.Text, "^!getMember", RegexOptions.IgnoreCase) && !e.Channel.IsPrivate)
                    {
                        Match cmd = Regex.Match(e.Message.RawText, @"!getMember (?:<@(.+)>)?(?: (--verbose|-v))?", RegexOptions.IgnoreCase);
                        User member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, e.Server);
                        bool verbose = cmd.Groups[2].Value != "";

                        Discord.getMember(member, e, verbose);
                    }
                    else if (e.Message.Text.StartsWith("!promote") && !e.Channel.IsPrivate)
                    {
                        Match cmd = Regex.Match(e.Message.RawText, @"!promote <@([^ ]+)>(?: (?:(?:(?:--force |-f )\((.+)\))|(?:(?:--date |-d )([^ ]+))|(?:(-h))|((?:--ignore-capacity|--ignore-max-capacity|-i))))*");
                        if (cmd.Groups[1].Value != "")
                        {
                            User member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, e.Server);
                            string force = cmd.Groups[2].Value;
                            string date = cmd.Groups[3].Value;
                            bool isForce = force != "";
                            bool help = cmd.Groups[4].Value.Equals("-h");
                            bool ignoreCapacity = cmd.Groups[5].Value != "";

                            Discord.promote(member, e, force, date, isForce, help, ignoreCapacity);
                        } else
                        {
                            await e.Channel.SendMessage("Error: Incorrect syntax");
                        }
                    }
                    else if (e.Message.Text.StartsWith("!createMember") && !e.Channel.IsPrivate)
                    {
                        Match cmd = Regex.Match(e.Message.RawText, @"!createMember <@(.+)>(?: --date ([^ ]+)| -d ([^ ]+))?(?: --steam ([^ 0-9]+)| -s ([^ 0-9]+)| --steam ([^ ][0-9]+)| -s ([^ ][0-9]+))?( --populate| -p)?");
                        User member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, e.Server);
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
                    else if (e.Message.Text.StartsWith("!updateMember") && !e.Channel.IsPrivate)
                    {
                        try {
                            Match cmd = Regex.Match(e.Message.RawText, @"!updateMember <@(.+)>(?: ([^ .:]+)(?:\.([^ ]+) \((.+)\))?(?:\:([^ ]+))? (?:\((.+)\)))?");
                            User member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, e.Server);
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
                            await e.Channel.SendMessage("Something went wrong. Make sure you're tagging the member and using correct syntax.");
                        }
                    }
                    else if (e.Message.Text == "!news")
                    {
                        Discord.news(e);
                    }
                    else if (e.Message.Text.StartsWith("!updateNews"))
                    {
                        Match cmd = Regex.Match(e.Message.RawText, @"!updateNews(?: (?:(--silent|-s)|(?:(?:--force|-f) (?:#)?([^ ]+))))* (.+)", RegexOptions.IgnoreCase);
                        bool silent = cmd.Groups[1].Value != "";
                        string force = cmd.Groups[2].Value;
                        if (force == "") { force = "general"; }
                        Console.WriteLine(force);
                        string news = cmd.Groups[3].Value;
                        Discord.updateNews(e, news, silent, force);
                    }
                    else if(e.Message.Text.StartsWith("!lenny"))
                    {
                        if(e.User.Name == "FoxTale")
                        {
                            await e.Channel.SendMessage("http://i.imgur.com/MXeL1Jh.gifv");
                        }
                        else if(e.User.Name == "Mardan")
                        {
                            await e.Channel.SendMessage("http://i.imgur.com/hLvfgHe.gif");
                        }
                        else
                        {
                            await e.Channel.SendMessage("( ͡° ͜ʖ ͡°)");
                        }                        
                    }
                    else if(e.Message.Text.StartsWith("!brutal"))
                    {
                        if(e.User.Name == "FoxTale")
                        {
                            await e.Channel.SendMessage("http://i.imgur.com/Vw20PUI.png");
                        } else
                        {
                            await e.Channel.SendMessage("http://i.imgur.com/0eMrMLd.jpg");
                        }
                    }
                    else if(e.Message.Text.StartsWith("(╯°□°）╯︵ ┻━┻"))
                    {
                        await e.Channel.SendMessage("┬─┬﻿ ノ( ゜-゜ノ)");
                    }
                    else if(e.Message.Text.StartsWith("!warframemarket") || e.Message.Text.StartsWith("!wfmarket") || e.Message.Text.StartsWith("!wfm"))
                    {
                        await e.Channel.SendMessage("http://warframe.market");
                    }
                    /*else if(e.Message.Text.StartsWith("!createChannel"))
                    {
                        Match cmd = Regex.Match(e.Message.RawText, @"!createChannel ([^< ]+)(?: <@([^>]+)>)?(?: <@([^>]+)>)?(?: <@([^>]+)>)?");
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
                    }*/
                    else if(e.Message.Text.StartsWith("!emotes") || e.Message.Text.StartsWith("!e ") || e.Message.Text == "!e")
                    {
                        Emotes emotes = new Emotes(emoteDir);
                        string cmd = e.Message.Text.Replace("!emotes", "").Replace("!e", "").Replace(" ", "");
                        if (cmd == "")
                        {
                            await e.Channel.SendMessage("Welcome to the Spawner Swarm Emotes system (beta)! To send an emote, send a message like this '!e <emote_ref>'. To see a list of emotes, send '!emotes list' or '!e list'");
                        }
                        else
                        {
                            Discord.getEmote(e, emotes, cmd);
                        }
                    }
                    else if(e.Message.Text.StartsWith("!newEmote") && !e.Channel.IsPrivate)
                    {
                        Emotes emotes = new Emotes(emoteDir);
                        Match cmd = Regex.Match(e.Message.RawText, @"!newEmote \((.+)\) ([^ ]+) ([^ ]+) ([^ ]+)(?: <@([^ ]+)>)?");
                        string name = cmd.Groups[1].Value;
                        string reference = cmd.Groups[2].Value;
                        short reqRank = short.Parse(cmd.Groups[3].Value);
                        string URL = cmd.Groups[4].Value;
                        if(cmd.Groups[5].Value != "")
                        {
                            //string author = e.Server.GetMemberByKey(cmd.Groups[5].Value).Username;
                            string author = Discord.getDiscordMemberByID(cmd.Groups[5].Value, e.Server).Name;
                            Discord.createEmote(e, emotes, name, URL, reference, reqRank, author);
                        }
                        else
                        {
                            Discord.createEmote(e, emotes, name, URL, reference, reqRank);
                        }
                        
                    }
                    else if(e.Message.Text.StartsWith("!addForma") && !e.Channel.IsPrivate)
                    {
                        Match cmd = Regex.Match(e.Message.RawText, @"!addForma <@(.+)> ([1-9])");
                        if (cmd.Groups[1].Value != "" && cmd.Groups[2].Value != "")
                        {
                            User member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, e.Server);
                            short formas = short.Parse(cmd.Groups[2].Value);
                            if (formas == 0)
                            {
                                await e.Channel.SendMessage("Must donate between 0 and 10 formas at a time (not inclusive)");
                            }
                            else
                            {
                                Discord.addForma(e, member, formas);
                            }
                        } else
                        {
                            await e.Channel.SendMessage("Error: Incorrect syntax");
                        }
                    }
                    else if(e.Message.Text.StartsWith("!info"))
                    {
                        string member = Regex.Match(e.Message.RawText, @"!info <@(.+)>").Groups[1].Value;
                        //User m = e.Server.GetMemberByKey(member);
                        User m = Discord.getDiscordMemberByID(member, e.Server);
                        Console.WriteLine(m.Id);
                        Console.WriteLine(m.Name);
                        Console.WriteLine(m.Roles);
                    }
                    
                    //-------------------------------------NEXUS-----------------------------------------

                    else if(Regex.IsMatch(e.Message.Text, @"^!p(rice)?\s?c(heck)?", RegexOptions.IgnoreCase))
                    {
                        string id = Regex.Match(e.Message.RawText, @"^!p(rice)?\s?c(heck)? (.+)", RegexOptions.IgnoreCase).Groups[3].Value;
                        Discord.priceCheck(e, id);
                    }

                    //-------------------------------------END NEXUS-----------------------------------------

                    else if(Regex.IsMatch(e.Message.Text, @"^!memberList", RegexOptions.IgnoreCase))
                    {
                        Discord.getMemberCount(e);
                    }
                    else if(Regex.IsMatch(e.Message.Text, @"^!event(?:s)?", RegexOptions.IgnoreCase))
                    {
                        Events events = new Events(eventDir);
                        string cmd = e.Message.Text.Replace("!events", "").Replace("!event", "").Replace(" ", "");//Regex.Match(e.Message.RawText, @"!event(?:s)?(.+)").Groups[1].Value;
                        if (cmd.StartsWith("list"))
                        {
                            if (cmd == "list")
                            {
                                Discord.listEvents(e, events);
                            }
                            else
                            {
                                try
                                {
                                    Discord.listEvents(e, events, int.Parse(cmd.Replace("list", "").Replace("-", "")));
                                }
                                catch
                                {
                                    await e.Channel.SendMessage("```xl\nError: Invalid page\n```");
                                }
                            }
                        } else if(cmd == "")
                        {
                            Discord.displayEvent(e, events);
                        } else
                        {
                            Discord.displayEvent(e, events, cmd);
                        }
                    }
                    else if(Regex.IsMatch(e.Message.Text, @"^!archive", RegexOptions.IgnoreCase) && !e.Channel.IsPrivate)
                    {
                        Match cmd = Regex.Match(e.Message.Text, @"!archive #(.+)", RegexOptions.IgnoreCase);
                        string channel = cmd.Groups[1].Value;
                        await Discord.archive(e, channel);
                    }
                };
            };
            /*Discord.client.PrivateMessageReceived += (sender, e) =>
            {
                Console.WriteLine(e.User.Name + ": " + e.Message);
                if(e.Message.StartsWith("!updateNews"))
                {
                    Match cmd = Regex.Match(e.Message, @"!updateNews(?: (?:(--silent|-s)|(?:(?:--force|-f) (?:#)?([^ ]+))))* (.+)", RegexOptions.IgnoreCase);
                    bool silent = cmd.Groups[1].Value != "";
                    string force = cmd.Groups[2].Value;
                    if(force == "") { force = "general"; }
                    Console.WriteLine(force);
                    string news = cmd.Groups[3].Value;
                    Discord.pmUpdateNews(e, news, silent, force);
                }
            };*/
            Discord.Connect();
                Console.ReadKey();
        }
    }
}
