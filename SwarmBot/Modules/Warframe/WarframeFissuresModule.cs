using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SwarmBot.XML;
using System.Xml.Linq;
using Trileans;
using System.Text.RegularExpressions;

namespace SwarmBot.Warframe
{
    [Group("warframe fissures"), Alias("warframefissures", "wf fissures", "wffissures", "fissures", "wf")]
    public partial class WarframeFissuresModule : ModuleBase
    {
        [Command(), Alias("list")]
        public async Task Default()
        {
            List<Fissure> fissures = WorldState.Fissures;
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle("Fissures");
            foreach (Fissure fissure in fissures)
            {
                TimeSpan rem = fissure.remainingTime;
                builder.AddField(x =>
                {
                    x.Name = $"{Regex.Replace(fissure.tier, @"\(.+\)", "Fissure")} - {fissure.enemy} {fissure.missionType} on {fissure.node}";
                    x.Value = $"{rem.Days}d {rem.Hours}h {rem.Minutes}m {rem.Seconds}s remaining";
                });
            }
            await ReplyAsync("", embed: builder.Build());
        }
    }
}
