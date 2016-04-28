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
using SwarmBot;

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
                Discord.client.UpdateCurrentGame("Type !help for help");
            };

            Discord.client.MessageReceived += (sender, e) =>
            {
                if (e.Author.Username != "SwarmBot")
                {
                    if (e.MessageText == "!help")
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
                        string guildmail = "https://onedrive.live.com/redir?resid=F485764E97178E7C!5421&authkey=!AC4Oi92whZUO3xo&ithint=file%2cpdf";
                        e.Channel.SendMessage(guildmail);
                    }
                    else if (e.MessageText.StartsWith("!invite"))
                    {
                        Discord.invite(e);
                    }
                    else if (e.MessageText.StartsWith("!getMember"))
                    {
                        Match cmd = Regex.Match(e.MessageText, @"!getMember (?:<@(.+)>)?(?: )?(?:(-f))?(?: )?(?:(-h))?");
                        DiscordMember member = e.Channel.Parent.GetMemberByKey(cmd.Groups[1].Value);
                        bool force = cmd.Groups[2].Value.Equals("-f");
                        bool help = cmd.Groups[3].Value.Equals("-h");

                        Discord.getMember(member, e, force, help);
                    }
                    else if (e.MessageText.StartsWith("!promote"))
                    {
                        Match cmd = Regex.Match(e.MessageText, @"!promote <@(.+)>(?: -f \((.+)\))?(?: --date ([^ ]+)| -d ([^ ]+))?(?: (-h))?");
                        DiscordMember member = e.Channel.Parent.GetMemberByKey(cmd.Groups[1].Value);
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

                        Discord.promote(member, e, force, date, isForce, help);
                    }
                    else if (e.MessageText.StartsWith("!createMember"))
                    {
                        Match cmd = Regex.Match(e.MessageText, @"!createMember <@(.+)>(?: --date ([^ ]+)| -d ([^ ]+))?(?: --steam ([^ 0-9]+)| -s ([^ 0-9]+)| --steam ([^ ][0-9]+)| -s ([^ ][0-9]+))?( --populate| -p)?");
                        DiscordMember member = e.Channel.Parent.GetMemberByKey(cmd.Groups[1].Value);
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
                            DiscordMember member = e.Channel.Parent.GetMemberByKey(cmd.Groups[1].Value);
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
                        DiscordMember member = e.Channel.Parent.GetMemberByKey(Regex.Match(e.MessageText.Replace("!populate ", ""), @"<@(.+)>").Groups[1].Value);
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
                        }
                    }
                    else if(e.MessageText.StartsWith("!warframemarket") || e.MessageText.StartsWith("!wfmarket") || e.MessageText.StartsWith("!wfm"))
                    {
                        e.Channel.SendMessage("http://warframe.market");
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
