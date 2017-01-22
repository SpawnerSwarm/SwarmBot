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
    [Group("e"), Alias("emotes", "emote")]
    class EmoteModule : ModuleBase
    {
        [Command, Summary("")]
        public async Task Default(string remainder = "")
        {
            string cmd = remainder.Replace(" ", "");
            if (cmd == "")
            {
                CommandInfoDB info = new CommandInfoDB(Config.CommandInfoDBPath);
                string message = "Avaliable commands are:\n";
                message += ((DBSubModule)info.getCommandByName("emotes")).getDescriptionForCommandUsed(Context.Message.Content);
                await ReplyAsync(message);
            }
            else
            {
                await Discord.getEmote(new DiscordCommandArgs
                {
                    e = Context.Message,
                    reference = cmd
                });
            }
        }

        [Command("list"), Summary("List all emotes")]
        public async Task list(short page = 1)
        {
            Emotes emotes = new Emotes(Config.EmoteDBPath);
            page = Math.Abs(page);
            try
            {
                string message = $"Page {(page == 0 ? 1 : page)}. To move to the next page, use **!e list {(page == 0 ? 2 : page + 1)}**. To view information about a specific emote, use **!e list <emote_ref>**.\n";
                List<Emote> eList = emotes.getBatchEmotes(page, 5);
                short i = 0;
                foreach (Emote emote in eList)
                {
                    IUser user = (Context.IsPrivate ? Context.Client.CurrentUser : Discord.getDiscordMemberByID(emote.creator, Context.Guild as SocketGuild));
                    await ReplyAsync((i == 0 ? message : ""), false, buildEmbed(emote.name, emote.reference, emote.requiredRank, emote.content, user));
                    /*message += "```xl\nName: " + emote.name.Replace('\'', 'ꞌ'); //Replaces the apostraphe with a Latin Small Letter Saltillo (U+A78C) so it won't break Discord formatting (as much).
                    message += "\nReference: " + emote.reference;
                    message += "\nRequired Rank: " + emote.requiredRank;
                    message += "\nCreator: " + emote.creator + "\n```\n";*/
                    i++;
                }
                if (i == 0) { await ReplyAsync("http://i.imgur.com/zdMAeE9.png"); }
            }
            catch (XMLException x)
            {
                if (x.errorCode == XMLErrorCode.NotFound) { await ReplyAsync("http://i.imgur.com/zdMAeE9.png"); return; }
            }
            //await ReplyAsync(message);
            return;
        }

        [Command("create"), Summary("Create a new emote"), Alias("new", "make"), RequireOwner]
        public async Task create(string name, string reference, short requiredRank, string content, IUser creator)
        {
            DiscordCommandArgs e = new DiscordCommandArgs
            {
                e = Context.Message,
                name = name,
                reference = reference,
                requiredRank = requiredRank, //Required Rank
                id = content, //Content
            };

            Emote emote = new Emote(e.name, e.reference, e.id, e.requiredRank, creator.Id.ToString());
            Emotes emotes = new Emotes(Config.EmoteDBPath);
            try { emotes.addEmote(emote); }
            catch (XMLException x)
            {
                await e.e.Channel.SendMessageAsync("`An error occurred while creating the emote. An emote with the same " + x.message + " already exists");
                Program.Log("Unable to create emote. An emote with the same " + x.message + " already exists");
                return;
            }
            await e.e.Channel.SendMessageAsync("Successfully created emote " + emote.name + "!");
        }

        public Embed buildEmbed(string name, string reference, Rank rank, string content, IUser author)
        {
            string url = (Regex.IsMatch(content, @"http(?:s)?:\/\/[^ \/]+\....") ? content : "http://i.imgur.com/88Cxeyw.png");

            EmbedAuthorBuilder authorBuilder = new EmbedAuthorBuilder()
                .WithIconUrl(author.AvatarUrl)
                .WithName(author.Username);

            EmbedBuilder builder = new EmbedBuilder()
                .WithThumbnailUrl(url.Replace(".gif", "h.jpg"))
                .WithTitle(name)
                .WithAuthor(authorBuilder)
                .AddField(x =>
                {
                    x.Name = "Reference";
                    x.Value = reference;
                    x.IsInline = true;
                })
                .AddField(x =>
                {
                    x.Name = "RequiredRank";
                    x.Value = rank.ToString();
                    x.IsInline = true;
                })
                .WithColor(rank.color);

            if(url == "http://i.imgur.com/88Cxeyw.png")
            {
                builder.WithDescription(content);
            }
            else
            {
                builder.Url = url;
            }

            return builder.Build();
        }
    }
}
