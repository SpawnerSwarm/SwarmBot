using System;
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
                    else if (e.Message.Text.StartsWith("!guildmail"))
                    {
                        string guildmail = "https://1drv.ms/b/s!AnyOF5dOdoX0v0iXHyVMBfggyOqy";
                        await e.Channel.SendMessage(guildmail);
                    }
                    if (Regex.IsMatch(e.Message.Text, "^!getMember", RegexOptions.IgnoreCase) && !e.Channel.IsPrivate)
                    {
                        Match cmd = Regex.Match(e.Message.RawText, @"!getMember (?:<@(.+)>)?(?: (--verbose|-v))?", RegexOptions.IgnoreCase);
                        if(cmd.Groups[1].Value == "") { await e.Channel.SendMessage("`Error: Incorrect Syntax`"); return; }
                        await Discord.getMember(new DiscordCommandArgs
                        {
                            e = e,
                            verbose = cmd.Groups[2].Value != "",
                            member = Discord.getDiscordMemberByID(cmd.Groups[1].Value, e.Server)
                        });
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
