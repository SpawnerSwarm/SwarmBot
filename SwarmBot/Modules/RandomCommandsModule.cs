using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SwarmBot.XML;
using Trileans;
using System.Text.RegularExpressions;

namespace SwarmBot.Modules
{
    class RandomCommandsModule : ModuleBase
    {
        [Command("holdup"), Summary(""), RequireUserId(139109512744402944)]
        public async Task holdup()
        {
            await ReplyAsync("http://i.imgur.com/ACoUhAW.gifv");
        }

        [Command("brutal"), RequireUserId(138043934143152128)]
        public async Task brutal()
        {
            await ReplyAsync("http://i.imgur.com/1xzQkSo.png");
        }

        [Command("warframemarket"), Alias("wfm", "market")]
        public async Task warframemarket()
        {
            await ReplyAsync("http://warframe.market");
        }

        [Command("lenny"), Alias("( ͡° ͜ʖ ͡°)")]
        public async Task lenny()
        {
            switch(Context.User.Id)
            {
                case 138043934143152128: await ReplyAsync("http://i.imgur.com/MXeL1Jh.gifv"); break; //Fox
                case 137976237292388353: await ReplyAsync("http://i.imgur.com/6AMJZaD.gifv"); break; //Mardan
                case 139109512744402944: await ReplyAsync("http://i.imgur.com/LUfk3HX.gifv"); break; //Quantum
                default: await ReplyAsync("( ͡° ͜ʖ ͡°)"); break;
            }
        }
        [Command("source"), Alias("code", "sourcecode", "github")]
        public async Task source()
        {
            await ReplyAsync("https://github.com/SpawnerSwarm/SwarmBot");
        }
        [Command("guildmail"), Alias("gm", "mail")]
        public async Task guildmail()
        {
            await ReplyAsync("https://1drv.ms/b/s!AnyOF5dOdoX0v0iXHyVMBfggyOqy");
        }
        [Command("uptime"), Alias("time")]
        public async Task uptime()
        {
            TimeSpan time = DateTime.Now - Program.startTime;
            await ReplyAsync($"SwarmBot has been online continuously for **{time.Days}** days **{time.Hours}** hours **{time.Minutes}** minutes **{time.Seconds}** seconds");
        }
    }
}
