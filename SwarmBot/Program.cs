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
        public static DateTime startTime;

        public static void Log(string text)
        {
            if (Application.OpenForms.Count < 1) { return; }
            logHandler?.Invoke(text);
        }

        [STAThread]
        static void Main()
        {
            if (System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1) { return; }
            startTime = DateTime.Now;
            Config.ExePath = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory;
            using (StreamReader sr = File.OpenText(Path.Combine(Config.ExePath.FullName, "config.txt")))
            {
                Config.AppDataPath = sr.ReadLine();
                Config.MemberDBPath = Path.Combine(Config.AppDataPath, sr.ReadLine());
                Config.EmoteDBPath = Path.Combine(Config.AppDataPath, sr.ReadLine());
                Config.AlertsDBPath = Path.Combine(Config.AppDataPath, sr.ReadLine());
                Config.CommandInfoDBPath = Path.Combine(Config.AppDataPath, sr.ReadLine());
                Config.discordToken = sr.ReadLine();
                Config.discordArchiveServerId = sr.ReadLine();
                Config.discordSwarmServerId = sr.ReadLine();
                Config.guildmailURL = sr.ReadLine();
                Config.wfWorldStateURL = sr.ReadLine();
                Config.wfJSPath = sr.ReadLine();
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
                else if(e.Channel.Id == 137991656547811328)
                {
                    IGuildUser user = await (e.Channel as IGuildChannel).GetUserAsync(e.Author.Id);
                    if(user.RoleIds.Contains<ulong>(280864289420738561) && e.Attachments.Count >= 1)
                    {
                        await (e as IUserMessage).AddReactionAsync(Emoji.Parse("<:Weeb:230529609475686400>"));
                    }
                    
                }
            };
            Discord.client.UserJoined += async (e) =>
            {
                if (e.Guild.Id == 137991656547811328)
                {
                    await (e.Guild.GetChannel(137991656547811328) as IMessageChannel).SendMessageAsync($"Greetings {e.Username}! Welcome to the Swarm!\nPlease read the guild mail at https://1drv.ms/b/s!AnyOF5dOdoX0v0iXHyVMBfggyOqy and ask a Veteran or above if you have any questions!");
                }
            };
            Discord.client.Log += async (e) =>
            {
                Log($"Discord.Net Log.\n\tException: {e.Exception}\n\tMessage: {e.Message}\n\tSource: {e.Source}");
            };

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
        public static string CommandInfoDBPath;
        internal static string oldMemberDBPath, oldEmoteDBPath, oldAlertsDBPath;
        public static DirectoryInfo ExePath;
        public static string discordToken;
        public static string discordArchiveServerId;
        public static string discordSwarmServerId;
        public static string guildmailURL;
        public static string wfWorldStateURL;
        public static string wfJSPath;
        public static bool debugModeActive;
    }
}
