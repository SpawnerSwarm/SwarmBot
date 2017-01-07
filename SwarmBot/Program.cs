using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using SwarmBot.UI;
using SwarmBot.Warframe;
using Discord;
using Discord.Commands;
using Discord.Modules;
using Discord.Net;
using Discord.WebSocket;
using Discord.Audio;
using System.Text.RegularExpressions;

namespace SwarmBot
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public delegate void StringDelegate(string string1);
        public static event StringDelegate logHandler;

        public static void Log(string text)
        {
            if (Application.OpenForms.Count < 1) { return; }
            logHandler?.Invoke(text);
        }

        [STAThread]
        static void Main()
        {
            if (System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1) { return; }
            Config.ExePath = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory;
            using (StreamReader sr = File.OpenText(Path.Combine(Config.ExePath.FullName, "config.txt")))
            {
                Config.AppDataPath = sr.ReadLine();
                Config.MemberDBPath = Path.Combine(Config.AppDataPath, sr.ReadLine());
                Config.EmoteDBPath = Path.Combine(Config.AppDataPath, sr.ReadLine());
                Config.AlertsDBPath = Path.Combine(Config.AppDataPath, sr.ReadLine());
                Config.discordToken = sr.ReadLine();
                Config.discordArchiveServerId = sr.ReadLine();
                Config.discordSwarmServerId = sr.ReadLine();
                Config.debugModeActive = false;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormUI());
        }

        public static async void onUIReadyCallback()
        {
            await Task.Run(() => Log("SwarmBot UI Loaded"));
            
            Discord.client = new DiscordSocketClient();
            Discord.client.Ready += async () =>
            {
                Log("Connected to Discord! User: " + Discord.client.CurrentUser.Username);
                await Discord.client.SetGameAsync("Type !help for help");
            };
            Discord.client.MessageReceived += async (e) =>
            {
                if (e.Channel.Id == 264287324710371328)
                {
                    if (!Config.debugModeActive) { await Discord.client.SetGameAsync("Type !help for help"); }
                    else { await Discord.client.SetGameAsync("DEBUG MODE"); }
                    await WarframeAlertsModule.newAlertReceived(e);
                }
            };
            /*
                if (!e.Author.IsBot)
                {
                    if (!Config.debugModeActive) { await Discord.client.SetGameAsync("Type !help for help"); }
                    else { await Discord.client.SetGameAsync("DEBUG MODE"); }
                    if (e.Content == "!help") { await Discord.help(new DiscordCommandArgs { e = e }); }
                    if (e.Content.StartsWith("!wfwiki") || e.Content.StartsWith("!skwiki")) { await Discord.wiki(new DiscordCommandArgs { e = e }); }
                    else if (Regex.IsMatch(e.Content, "^!guildmail", RegexOptions.IgnoreCase)) { await e.Channel.SendMessageAsync("https://1drv.ms/b/s!AnyOF5dOdoX0v0iXHyVMBfggyOqy"); }
                    if (Regex.IsMatch(e.Content, "^!getMember", RegexOptions.IgnoreCase) && !e.Channel.IsPrivate)
                    {
                        Match cmd = Regex.Match(e.Content, @"!getMember (?:<@(.+)>)?(?: (--verbose|-v))?", RegexOptions.IgnoreCase);
                        if (cmd.Groups[1].Value == "") { await e.Channel.SendMessageAsync("`Error: Incorrect Syntax`"); return; }
                        await Discord.getMember(new DiscordCommandArgs
                        {
                            e = e,
                            verbose = cmd.Groups[2].Value != "",
                            member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, e.Channel.)
                        });
                    }
                    else if (Regex.IsMatch(e.Content, "^!promote", RegexOptions.IgnoreCase) && !e.Channel.IsPrivate)
                    {
                        Match cmd = Regex.Match(e.Content, @"!promote <@([^ ]+)>(?: (?:(?:(?:--force |-f )\((.+)\))|(?:(?:--date |-d )([^ ]+))|(?:(-h))|((?:--ignore-capacity|--ignore-max-capacity|-i))))*", RegexOptions.IgnoreCase);
                        if (cmd.Groups[1].Value == "") { await e.Channel.SendMessageAsync("`Error: Incorrect Syntax`"); return; }
                        await Discord.promote(new DiscordCommandArgs
                        {
                            e = e,
                            member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, (e.Channel as IGuildChannel)?.Guild as SocketGuild),
                            force = cmd.Groups[2].Value,
                            date = cmd.Groups[3].Value,
                            ignore = cmd.Groups[5].Value != ""
                        });
                    }
                    else if (Regex.IsMatch(e.Content, "^!createMember", RegexOptions.IgnoreCase) && !e.Channel.IsPrivate)
                    {
                        Match cmd = Regex.Match(e.Content, @"!createMember <@([^ ]+)>(?: (?:(?:(?:--date |-d )([^ ]+))|(?:(?:--steam-id |-s |--steam )(\d+))))*", RegexOptions.IgnoreCase);
                        if (cmd.Groups[1].Value == "") { await e.Channel.SendMessageAsync("`Error: Incorrect Syntax`"); return; }
                        if (Regex.IsMatch(e.Content, "!createMember <@([^ ]+)> (?:(?:--steam-id |-s )((?:.+)?[^0-9]+(?:.+)?))", RegexOptions.IgnoreCase)) { await e.Channel.SendMessageAsync("Steam ID must be numeric"); return; }
                        await Discord.createMember(new DiscordCommandArgs
                        {
                            e = e,
                            member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, (e.Channel as IGuildChannel)?.Guild as SocketGuild),
                            date = cmd.Groups[2].Value,
                            steam = cmd.Groups[3].Value
                        });
                    }
                    else if (Regex.IsMatch(e.Content, "^!updateMember", RegexOptions.IgnoreCase) && !e.Channel.IsPrivate)
                    {
                        await e.Channel.SendMessageAsync("!updateMember to be re-added at a later date. Please contact Mardan to manually update database entries");
                    }

                    //Text Stuff
                    else if (e.Content.StartsWith("!lenny"))
                    {
                        switch (e.User.Name)
                        {
                            case "FoxTale": await e.Channel.SendMessageAsync("http://i.imgur.com/MXeL1Jh.gifv"); break;
                            case "Mardan": await e.Channel.SendMessageAsync("http://i.imgur.com/H0aG47G.gifv"); break;
                            case "Quantum-Nova": await e.Channel.SendMessageAsync("http://i.imgur.com/LUfk3HX.gifv"); break;
                            default: await e.Channel.SendMessageAsync("( ͡° ͜ʖ ͡°)"); break;
                        }
                    }
                    else if (e.Content.StartsWith("!holdup")) { if(e.User.Name == "Quantum-Nova") { await e.Channel.SendMessageAsync("http://i.imgur.com/ACoUhAW.gifv"); } }
                    else if (e.Content.StartsWith("!brutal")) { await e.Channel.SendMessageAsync("http://i.imgur.com/1xzQkSo.png"); }
                    else if (e.Content.StartsWith("(╯°□°）╯︵ ┻━┻")) { await e.Channel.SendMessageAsync("┬─┬﻿ ノ( ゜-゜ノ)"); }
                    else if (e.Content.StartsWith("!warframemarket") || e.Content.StartsWith("!wfmarket") || e.Content.StartsWith("!wfm")) { await e.Channel.SendMessageAsync("http://warframe.market"); }

                    //Emotes
                    else if (e.Content.StartsWith("!emote") || e.Content.StartsWith("!e ") || e.Content == "!e" || e.Content == "!emote")
                    {
                        string cmd = e.Content.Replace("!emotes", "").Replace("!e", "").Replace(" ", "");
                        if(cmd == "")
                        {
                            await e.Channel.SendMessageAsync("Welcome to the Spawner Swarm Emotes system (beta)! To send an emote, send a message like this '!e <emote_ref>'. To see a list of emotes, send '!emotes list' or '!e list'");
                        }
                        else
                        {
                            await Discord.getEmote(new DiscordCommandArgs
                            {
                                e = e,
                                reference = cmd
                            });
                        }
                    }
                    else if(e.Content.StartsWith("!newEmote") && !e.Channel.IsPrivate)
                    {
                        Match cmd = Regex.Match(e.Content, @"!newEmote \((.+)\) ([^ ]+) ([^ ]+) ([^ ]+)");
                        await Discord.createEmote(new DiscordCommandArgs
                        {
                            e = e,
                            name = cmd.Groups[1].Value,
                            reference = cmd.Groups[2].Value,
                            force = cmd.Groups[3].Value, //Required Rank
                            id = cmd.Groups[4].Value, //Content
                        });
                    }
                    else if(e.Content.StartsWith("!tagme") && !e.Channel.IsPrivate)
                    {
                        Match cmd = Regex.Match(e.Content, @"!tagme (.+)", RegexOptions.IgnoreCase);
                        if(cmd.Groups[1].Value == "") { await e.Channel.SendMessageAsync("Error: Argument blank. Correct format is \"!tagme (overwatch/warframe/sk/bot/rank)\""); return; }
                        if(cmd.Groups[1].Value != "overwatch" && cmd.Groups[1].Value != "warframe" && cmd.Groups[1].Value != "sk" && cmd.Groups[1].Value != "rank" && cmd.Groups[1].Value != "bot") { await e.Channel.SendMessageAsync("Error: Argument invalid. Correct format is \"!tagme (overwatch/warframe/sk/bot/rank)\""); return; }
                        await Discord.tagMember(new DiscordCommandArgs
                        {
                            e = e,
                            member = e.User,
                            force = cmd.Groups[1].Value
                        });
                    }
                    else if (e.Content.StartsWith("!untagme") && !e.Channel.IsPrivate)
                    {
                        Match cmd = Regex.Match(e.Content, @"!untagme (.+)", RegexOptions.IgnoreCase);
                        if (cmd.Groups[1].Value == "") { await e.Channel.SendMessageAsync("Error: Argument blank. Correct format is \"!untagme (overwatch/warframe/sk/bot)\""); return; }
                        if (cmd.Groups[1].Value != "overwatch" && cmd.Groups[1].Value != "warframe" && cmd.Groups[1].Value != "sk" && cmd.Groups[1].Value != "bot") { await e.Channel.SendMessageAsync("Error: Argument invalid. Correct format is \"!untagme (overwatch/warframe/sk/bot)\""); return; }
                        await Discord.untagMember(new DiscordCommandArgs
                        {
                            e = e,
                            member = e.User,
                            force = cmd.Groups[1].Value
                        });
                    }
                    else if (Regex.IsMatch(e.Content, @"^!memberList", RegexOptions.IgnoreCase))
                    {
                        await Discord.getMemberCount(new DiscordCommandArgs
                        {
                            e = e
                        });
                    }
                }
            };*/

            Discord.initializeDiscordClient();
        }

        public static void toggleDebug()
        {
            if(!Config.debugModeActive)
            {
                Config.debugModeActive = true;

                string memberDestPath = Path.Combine(Config.AppDataPath, new FileInfo(Config.MemberDBPath).Name.Replace(".xml", ".debug.xml"));
                string emoteDestPath = Path.Combine(Config.AppDataPath, new FileInfo(Config.EmoteDBPath).Name.Replace(".xml", ".debug.xml"));
                string alertsDestPath = Path.Combine(Config.AppDataPath, new FileInfo(Config.AlertsDBPath).Name.Replace(".xml", ".debug.xml"));

                if (File.Exists(memberDestPath)) { File.Delete(memberDestPath); }
                if (File.Exists(emoteDestPath)) { File.Delete(emoteDestPath); }
                if (File.Exists(alertsDestPath)) { File.Delete(alertsDestPath); }

                File.Copy(Config.MemberDBPath, memberDestPath);
                File.Copy(Config.EmoteDBPath, emoteDestPath);
                File.Copy(Config.AlertsDBPath, alertsDestPath);

                Config.oldMemberDBPath = Config.MemberDBPath;
                Config.MemberDBPath = memberDestPath;

                Config.oldEmoteDBPath = Config.EmoteDBPath;
                Config.EmoteDBPath = emoteDestPath;

                Config.oldAlertsDBPath = Config.AlertsDBPath;
                Config.AlertsDBPath = alertsDestPath;

                Discord.client.SetGameAsync("DEBUG MODE");
                Log("Enabled Debug Mode");
            }
            else
            {
                Config.debugModeActive = false;

                File.Delete(Config.MemberDBPath);
                File.Delete(Config.EmoteDBPath);
                File.Delete(Config.AlertsDBPath);

                Config.MemberDBPath = Config.oldMemberDBPath;
                Config.EmoteDBPath = Config.oldEmoteDBPath;
                Config.AlertsDBPath = Config.oldAlertsDBPath;

                Config.oldMemberDBPath = null;
                Config.oldEmoteDBPath = null;
                Config.oldAlertsDBPath = null;

                Discord.client.SetGameAsync("Type !help for help.");
                Log("Disabled Debug Mode");
            }
        }
    }

    struct Config
    {
        public static string AppDataPath;
        public static string MemberDBPath;
        public static string EmoteDBPath;
        public static string AlertsDBPath;
        internal static string oldMemberDBPath, oldEmoteDBPath, oldAlertsDBPath;
        public static DirectoryInfo ExePath;
        public static string discordToken;
        public static string discordArchiveServerId;
        public static string discordSwarmServerId;
        public static bool debugModeActive;
    }
}
