﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using SwarmBot.UI;
using Discord;
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

        public static async Task Log(string text)
        {
            if (Application.OpenForms.Count < 1) { return; }
            logHandler?.Invoke(text);
        }

        [STAThread]
        static void Main()
        {
            Config.ExePath = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory;
            using (StreamReader sr = File.OpenText(Path.Combine(Config.ExePath.FullName, "config.txt")))
            {
                Config.AppDataPath = sr.ReadLine();
                Config.MemberDBPath = Path.Combine(Config.AppDataPath, sr.ReadLine());
                Config.EmoteDBPath = Path.Combine(Config.AppDataPath, sr.ReadLine());
                Config.discordToken = sr.ReadLine();
                Config.discordArchiveServerId = sr.ReadLine();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormUI());
        }

        public static async void onUIReadyCallback()
        {
            await Task.Run(() => Log("SwarmBot UI Loaded"));
            Discord.client = new DiscordClient();
            Discord.client.Ready += async (sender, e) =>
            {
                await Log("Connected to Discord! User: " + Discord.client.CurrentUser.Name);
                Discord.client.SetGame("Type !help for help");
            };
            Discord.client.MessageReceived += async (sender, e) =>
            {
                if (!e.Message.IsAuthor)
                {
                    Discord.client.SetGame("Type !help for help");
                    if (e.Message.Text == "!help") { await Discord.help(new DiscordCommandArgs { e = e }); }
                    if (e.Message.Text.StartsWith("!wfwiki") || e.Message.Text.StartsWith("!skwiki")) { await Discord.wiki(new DiscordCommandArgs { e = e }); }
                    else if (Regex.IsMatch(e.Message.Text, "^!guildmail", RegexOptions.IgnoreCase)) { await e.Channel.SendMessage("https://1drv.ms/b/s!AnyOF5dOdoX0v0iXHyVMBfggyOqy"); }
                    if (Regex.IsMatch(e.Message.Text, "^!getMember", RegexOptions.IgnoreCase) && !e.Channel.IsPrivate)
                    {
                        Match cmd = Regex.Match(e.Message.RawText, @"!getMember (?:<@(.+)>)?(?: (--verbose|-v))?", RegexOptions.IgnoreCase);
                        if (cmd.Groups[1].Value == "") { await e.Channel.SendMessage("`Error: Incorrect Syntax`"); return; }
                        await Discord.getMember(new DiscordCommandArgs
                        {
                            e = e,
                            verbose = cmd.Groups[2].Value != "",
                            member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, e.Server)
                        });
                    }
                    else if (Regex.IsMatch(e.Message.Text, "^!promote", RegexOptions.IgnoreCase) && !e.Channel.IsPrivate)
                    {
                        Match cmd = Regex.Match(e.Message.RawText, @"!promote <@([^ ]+)>(?: (?:(?:(?:--force |-f )\((.+)\))|(?:(?:--date |-d )([^ ]+))|(?:(-h))|((?:--ignore-capacity|--ignore-max-capacity|-i))))*", RegexOptions.IgnoreCase);
                        if (cmd.Groups[1].Value == "") { await e.Channel.SendMessage("`Error: Incorrect Syntax`"); return; }
                        await Discord.promote(new DiscordCommandArgs
                        {
                            e = e,
                            member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, e.Server),
                            force = cmd.Groups[2].Value,
                            date = cmd.Groups[3].Value,
                            ignore = cmd.Groups[5].Value != ""
                        });
                    }
                    else if (Regex.IsMatch(e.Message.Text, "^!createMember", RegexOptions.IgnoreCase) && !e.Channel.IsPrivate)
                    {
                        Match cmd = Regex.Match(e.Message.RawText, @"!createMember <@([^ ]+)>(?: (?:(?:(?:--date |-d )([^ ]+))|(?:(?:--steam-id |-s |--steam )(\d+))))*", RegexOptions.IgnoreCase);
                        if (cmd.Groups[1].Value == "") { await e.Channel.SendMessage("`Error: Incorrect Syntax`"); return; }
                        if (Regex.IsMatch(e.Message.RawText, "!createMember <@([^ ]+)> (?:(?:--steam-id |-s )((?:.+)?[^0-9]+(?:.+)?))", RegexOptions.IgnoreCase)) { await e.Channel.SendMessage("Steam ID must be numeric"); return; }
                        await Discord.createMember(new DiscordCommandArgs
                        {
                            e = e,
                            member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, e.Server),
                            date = cmd.Groups[2].Value,
                            steam = cmd.Groups[3].Value
                        });
                    }
                    else if (Regex.IsMatch(e.Message.Text, "^!updateMember", RegexOptions.IgnoreCase) && !e.Channel.IsPrivate)
                    {
                        await e.Channel.SendMessage("!updateMember to be re-added at a later date. Please contact Mardan to manually update database entries");
                    }

                    //Text Stuff
                    else if (e.Message.Text.StartsWith("!lenny"))
                    {
                        switch (e.User.Name)
                        {
                            case "FoxTale": await e.Channel.SendMessage("http://i.imgur.com/MXeL1Jh.gifv"); break;
                            case "Mardan": await e.Channel.SendMessage("http://i.imgur.com/hLvfgHe.gif"); break;
                            default: await e.Channel.SendMessage("( ͡° ͜ʖ ͡°)"); break;
                        }
                    }
                    else if (e.Message.Text.StartsWith("!brutal")) { await e.Channel.SendMessage("http://i.imgur.com/1xzQkSo.png"); }
                    else if (e.Message.Text.StartsWith("(╯°□°）╯︵ ┻━┻")) { await e.Channel.SendMessage("┬─┬﻿ ノ( ゜-゜ノ)"); }
                    else if (e.Message.Text.StartsWith("!warframemarket") || e.Message.Text.StartsWith("!wfmarket") || e.Message.Text.StartsWith("!wfm")) { await e.Channel.SendMessage("http://warframe.market"); }

                    //Emotes
                    else if (e.Message.Text.StartsWith("!emote") || e.Message.Text.StartsWith("!e ") || e.Message.Text == "!e" || e.Message.Text == "!emote")
                    {
                        string cmd = e.Message.Text.Replace("!emotes", "").Replace("!e", "").Replace(" ", "");
                        if(cmd == "")
                        {
                            await e.Channel.SendMessage("Welcome to the Spawner Swarm Emotes system (beta)! To send an emote, send a message like this '!e <emote_ref>'. To see a list of emotes, send '!emotes list' or '!e list'");
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
                }
            };

            Discord.initializeDiscordClient();
        }
    }

    struct Config
    {
        public static string AppDataPath;
        public static string MemberDBPath;
        public static string EmoteDBPath;
        public static DirectoryInfo ExePath;
        public static string discordToken;
        public static string discordArchiveServerId;
    }
}
