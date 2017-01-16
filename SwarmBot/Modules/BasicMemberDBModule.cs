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
    public class BasicMemberDBModule : ModuleBase
    {
        [Command("getMember"), Summary("Gets information about a member from the Database"), RequireContext(ContextType.Guild), Alias("member", "get")]
        public async Task getMember([Summary("The user to get information about")] IGuildUser user, [Summary("If true, additional information will be provided")] string _verbose = "")
        {
            bool verbose = Regex.IsMatch(_verbose, @"^-v\z|--verbose\z", RegexOptions.IgnoreCase);

            try
            {
                XMLDocument memberDB = new XMLDocument(Config.MemberDBPath);
                XMLMember member = memberDB.getMemberById(user.Id);
                trilean isReady = member.checkReadyForRankup();
                //string message = "```xl\n";
                //message += member.name + " is a(n) " + member.rank + "\n";
                EmbedAuthorBuilder authorBuilder = new EmbedAuthorBuilder()
                    .WithIconUrl(user.AvatarUrl)
                    .WithName(member.name);
                EmbedBuilder builder = new EmbedBuilder()
                    .WithAuthor(authorBuilder)
                    .WithColor(member.rank.color)
                    .AddField(x =>
                    {
                        x.Name = "Rank";
                        x.Value = member.rank.ToString();
                        x.IsInline = true;
                    });

                if (isReady.value == TrileanValue.Tri)
                {
                    builder.AddField(x =>
                    {
                        x.Name = "Last Rankup";
                        x.Value = "Unknown";
                        x.IsInline = true;
                    });
                    switch ((XMLErrorCode)isReady.embedded)
                    {
                        case XMLErrorCode.Old: builder.WithDescription($"We can't tell whether or not {member.name} is ready for an upgrade, but they probably are since our data dates back before this bot's conception."); break; //message += "We can't tell whether or not " + member.name + " is ready for an upgrade, but they probably are since our data dates back before this bot's conception.\n"; break;
                        case XMLErrorCode.Maximum: builder.WithDescription("He/She has reached the maximum possible rank."); break;//message += "He/She has reached the maximum possible rank.\n"; break;
                    }
                }
                else if (isReady.value == TrileanValue.False)
                {
                    //message += "He/she is not eligible for a rankup at this time.\n";
                    //message += "It has been " + Regex.Match((string)isReady.embedded, @"(.+)\.(?:.+)?").Groups[1].Value + " days since their last rankup.\n";
                    builder.AddField(x =>
                    {
                        x.Name = "Last Rankup";
                        x.Value = member.rankupHistory[member.rank - 1].date.ToShortDateString();
                        x.IsInline = true;
                    })
                    .WithDescription($"{member.name} is not eligible for a rankup at this time.\nIt has been {Regex.Match((string)isReady.embedded, @"(.+)\.(?:.+)?").Groups[1].Value} days since their last rankup.");
                }
                else if (isReady.value == TrileanValue.True)
                {
                    //message += "He/she is eligible for a rankup.\n";
                    //message += "It has been " + Regex.Match((string)isReady.embedded, @"(.+)\.(?:.+)?").Groups[1].Value + " days since their last rankup.\n";
                    builder.AddField(x =>
                    {
                        x.Name = "Last Rankup";
                        x.Value = member.rankupHistory[member.rank - 1].date.ToShortDateString();
                        x.IsInline = true;
                    })
                    .WithDescription($"{member.name} is eligible for a rankup.\nIt has been {Regex.Match((string)isReady.embedded, @"(.+)\.(?:.+)?").Groups[1].Value} days since their last rankup.");
                }
                //message += member.name + " has donated " + member.forma + " Forma\n";
                builder.AddField(x => {
                    x.Name = "Forma Donated";
                    x.Value = member.forma.ToString();
                    x.IsInline = true;
                });
                if (verbose)
                {
                    if (member.WFName != "" && member.WFName != "NaN") {
                        builder.AddField(x =>
                        {
                            x.Name = "Warframe";
                            x.Value = member.WFName;
                            x.IsInline = true;
                        });
                    }//message += "\nWarframe name is " + member.WFName; }
                    if (member.SKName != "" && member.SKName != "NaN")
                    {
                        builder.AddField(x =>
                        {
                            x.Name = "Spiral Knights";
                            x.Value = member.SKName;
                            x.IsInline = true;
                        });
                    }//message += "\nSpiral Knights name is " + member.SKName; }
                    if (member.steamName != "" && member.steamName != "NaN")
                    {
                        builder.AddField(x =>
                        {
                            x.Name = "Steam";
                            x.Value = member.steamName;
                            x.IsInline = true;
                        });
                    }//message += "\nSteam name is " + member.steamName; }
                }
                //message += "```";
                await Context.Channel.SendMessageAsync("", false, builder);
            }
            catch (XMLException x)
            {
                switch (x.errorCode)
                {
                    case XMLErrorCode.MultipleFound:
                        await Context.Channel.SendMessageAsync("`An error occured: Multiple members were found`"); break;
                    case XMLErrorCode.NotFound:
                        await Context.Channel.SendMessageAsync("`An error occured: No members were found`"); break;
                    case XMLErrorCode.Unknown:
                        await Context.Channel.SendMessageAsync("`An unknown error occured`"); break;
                }
                Program.Log("An error occured while retreiving data for member " + user.Username + ", " + x.message + ": " + x.errorCode);
                return;
            }
        }
        [Command("createMember"), Summary("Create a new member in the database"), RequireContext(ContextType.Guild)]
        public async Task createMember([Summary("The Discord user to create an entry for")] IGuildUser user, [Summary("Optional Arguments"), Remainder] string remainder = "")
        {
            Match cmd = Regex.Match(remainder, @"(?:(?:--date |-d )([^ ]+))", RegexOptions.IgnoreCase);
            DiscordCommandArgs e = new DiscordCommandArgs
            {
                e = Context.Message,
                member = user,
                date = cmd.Groups[1].Value
            };

            try
            {
                XMLDocument memberDB = new XMLDocument(Config.MemberDBPath);
                XMLMember author = memberDB.getMemberById(e.e.Author.Id);
                if (!author.checkPermissions(Rank.Veteran)) { await ReplyAsync("```xl\nSorry, you don't have the permissions to do that\n```"); return; }

                try { memberDB.getMemberById(e.member.Id); }
                catch (XMLException x)
                {
                    if (x.errorCode == XMLErrorCode.MultipleFound) { await ReplyAsync("`An error occured. Multiple members were found for the provided ID.`"); return; }
                    else if (x.errorCode != XMLErrorCode.NotFound) { await ReplyAsync("`An unexpected error occured. " + x.message + "`"); return; }
                }

                DateTime date;
                if (e.date != "") { try { date = DateTime.Parse(e.date); } catch { await ReplyAsync("Invalid Date"); return; } }
                else { date = DateTime.Today; }

                /*long steamId;
                if (e.steam != "") { try { steamId = Int64.Parse(e.steam); } catch { await ReplyAsync("Steam ID must be numeric"); return; } }
                else { steamId = 0; }*/

                Program.Log("Creating member " + e.member.Username + "...");
                trilean t = memberDB.createMember(e.member.Username, date, e.member.Id);
                if (t.value == TrileanValue.False) { await ReplyAsync("An unexpected error occured. Could not create member."); return; }
                else
                {
                    await ReplyAsync("Success! Created member " + e.member.Username + "!");
                    Program.Log("Successfully created member " + e.member.Username);
                }
                memberDB.Save(Config.MemberDBPath);
            }
            catch (XMLException x)
            {
                switch (x.errorCode)
                {
                    case XMLErrorCode.MultipleFound:
                        await ReplyAsync("`An error occured: Multiple members were found`"); break;
                    case XMLErrorCode.NotFound:
                        await ReplyAsync("`An error occured: No members were found`"); break;
                    case XMLErrorCode.Unknown:
                        await ReplyAsync("`An unknown error occured`"); break;
                }
                Program.Log("An error occured while creating member " + e.member.Username + ", " + x.message + ": " + x.errorCode);
                return;
            }
        }
        [Command("promote"), Summary("Promote a member"), RequireContext(ContextType.Guild)]
        public async Task promote([Summary("The Discord user to promote")] IGuildUser user, [Summary("Optional Arguments"), Remainder] string remainder = "")
        {
            Match cmd = Regex.Match(remainder, @"(?:(?:(?:--force |-f )\((.+)\))|(?:(?:--date |-d )([^ ]+))|(?:(-h))|((?:--ignore-capacity|--ignore-max-capacity|-i)))*", RegexOptions.IgnoreCase);
            DiscordCommandArgs e = new DiscordCommandArgs
            {
                e = Context.Message,
                member = user,
                force = cmd.Groups[1].Value,
                date = cmd.Groups[2].Value,
                ignore = cmd.Groups[4].Value != ""
            };

            try
            {
                XMLDocument memberDB = new XMLDocument(Config.MemberDBPath);
                XMLMember member = memberDB.getMemberById(e.member.Id);
                XMLMember author = memberDB.getMemberById(e.e.Author.Id);
                if (!author.checkPermissions(Rank.Officer)) { await ReplyAsync("```xl\nSorry, you don't have the permissions to do that\n```"); return; }
                if (e.e.Author.Id == (ulong)member.discordId && !author.checkPermissions(Rank.GuildMaster)) { await ReplyAsync("```xl\nSorry, you can't promote yourself!\n```"); return; }
                if (member.rank == Rank.GuildMaster && e.force == "") { await ReplyAsync("```xl\nCan't promote " + member.name + " because they are already at maximum rank.\n```"); return; }

                Rank targetRank = e.force == "" ? (Rank)(member.rank + 1) : (Rank)e.force;
                if (targetRank >= author.rank && author.rank != Rank.GuildMaster) { await ReplyAsync("```xl\nCan't promote " + member.name + " because the destination rank is higher than your rank!\n```"); return; }

                trilean isRankMaxed;
                try
                {
                    isRankMaxed = memberDB.checkRankMaxed(targetRank);
                    if (isRankMaxed && (!e.ignore || !author.checkPermissions(Rank.GuildMaster))) { await ReplyAsync("```xl\nError: The Rank you have requested to promote to is currently at maximum capacity.\nPlease contact a Guild Master if you believe this is in error"); return; }

                    DateTime targetDate;
                    if (e.date == "") { targetDate = DateTime.Today; }
                    else
                    {
                        try { targetDate = DateTime.Parse(e.date); }
                        catch { await ReplyAsync("```xl\nError: Invalid Date. Must be in MM/DD/YYYY format.\n```"); return; }
                    }

                    try
                    {
                        trilean trilean = member.Promote(targetDate, targetRank);
                        //await (e.member as IGuildUser).AddRolesAsync(((e.e.Channel as IGuildChannel)?.Guild as SocketGuild).Roles.Where(x => x.Name == (Rank)trilean.embedded).First());
                        await Discord.applyRoleToMember(e.member, e.e, (Rank)trilean.embedded);
                        await ReplyAsync("```xl\nSuccessfully promoted " + member.name + " to " + trilean.embedded + "\n```");
                    }
                    catch (XMLException x)
                    {
                        switch (x.errorCode)
                        {
                            case XMLErrorCode.MultipleFound: await ReplyAsync("```xl\nError: Multiple Members found\n```"); break;
                            case XMLErrorCode.Maximum: await ReplyAsync("```xl\nCan't promote " + member.name + " because they are already at maximum rank.\n```"); break;
                            case XMLErrorCode.Greater: await ReplyAsync("```xl\nCan't promote " + member.name + " because the destination rank is higher than your rank!\n```"); break;
                            case XMLErrorCode.Unknown: await ReplyAsync("```xl\nAn error occured\n```"); break;
                        }
                    }
                }
                catch (Exception x)
                {
                    await ReplyAsync("An error occured: " + x.Message);
                    Program.Log("ERROR: " + author.name + " tried to promote " + member.name + " to " + targetRank + " but encountered an exception. " + x.Message);
                }
            }
            catch (XMLException x)
            {
                switch (x.errorCode)
                {
                    case XMLErrorCode.MultipleFound:
                        await ReplyAsync("`An error occured: Multiple members were found`"); break;
                    case XMLErrorCode.NotFound:
                        await ReplyAsync("`An error occured: No members were found`"); break;
                    case XMLErrorCode.Unknown:
                        await ReplyAsync("`An unknown error occured`"); break;
                }
                Program.Log("An error occured while promoting member " + e.member.Username + ", " + x.message + ": " + x.errorCode);
                return;
            }
        }

        [Command("memberList"), Alias("listMembers", "memberCount"), Summary("Lists the number of members in each rank")]
        public async Task memberList()
        {
            XMLDocument memberDB = new XMLDocument(Config.MemberDBPath);
            string message = "```xl\n";
            for (short i = 1; i <= 7; i++)
            {
                Rank rank = i;
                int memberCount = memberDB.document.Element("Database").Elements("Member").Where(x => x.Element("Rank").Value == rank.ToString()).Count();
                message += rank.ToString() + ": " + memberCount + " out of " + memberDB.getDefine(rank.ToString(), DefineType.RankCapacity) + "\n";
            }
            message += "\n```";
            await ReplyAsync(message);
        }
    }
}
