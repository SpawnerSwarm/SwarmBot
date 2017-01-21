using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Modules;
using Discord.Commands;
using Discord.WebSocket;
using SwarmBot.UI;
using SwarmBot.XML;
using System.Text.RegularExpressions;
using Trileans;
using System.Reflection;

namespace SwarmBot.TypeReaders
{
    public class BooleanTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> Read(ICommandContext context, string input)
        {
            bool result;
            if (bool.TryParse(input, out result))
                return Task.FromResult(TypeReaderResult.FromSuccess(result));

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as a boolean."));
        }
    }

    public class RankTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> Read(ICommandContext context, string input)
        {
            Rank result;
            try
            {
                result = (Rank)input;
                return Task.FromResult(TypeReaderResult.FromSuccess(result));
            }
            catch(Exception e)
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, e.Message));
            }
        }
    }
}

namespace SwarmBot
{
    class Discord
    {
        public static DiscordSocketClient client;
        public static CommandHandler handler;

        public static async Task applyRoleToMember(IUser member, IUserMessage e, string roleName)
        {
            await (member as IGuildUser).AddRolesAsync(((e.Channel as IGuildChannel)?.Guild as IGuild).Roles.Where(x => x.Name == roleName).First());
        }

        public static async Task removeRoleFromMember(IUser member, IUserMessage e, string roleName)
        {
            await (member as IGuildUser).RemoveRolesAsync(((e.Channel as IGuildChannel)?.Guild as IGuild).Roles.Where(x => x.Name == roleName).First());
        }

        public static async void initializeDiscordClient()
        {
            Program.Log("Initializing SwarmBot Discord...");
            try
            {
                /*client.ExecuteAndWait(async () =>
                {
                    await Task.Run(() => client.Connect(Config.discordToken, TokenType.Bot));
                });*/
                await client.LoginAsync(TokenType.Bot, Config.discordToken);
                await Task.Run(() => client.ConnectAsync());

                var map = new DependencyMap();
                map.Add(client);

                handler = new CommandHandler();
                await handler.Install(map);
            }
            catch(Exception e)
            {
                Program.Log("Discord connection failed." + e.Message + " | " + e.Source);
            }
        }
        public static IUser getDiscordMemberByID(string ID, SocketGuild server)
        {
            return server.GetUser(ulong.Parse(ID.Replace("!", "")));
        }
        public static async Task getEmote(DiscordCommandArgs e)
        {
            Emotes emotes = new Emotes(Config.EmoteDBPath);
            /*if(e.reference.StartsWith("list"))
            {
                short page;
                try { page = (e.reference == "list" ? (short)0 : Int16.Parse(e.reference.Replace("list", "").Replace("-", ""))); }
                catch(Exception x)
                {
                    Program.Log("Expected emote list page number. Received " + e.reference + " instead: " + x.Message);
                    await e.e.Channel.SendMessageAsync("Error: Invalid page number");
                    return;
                }

                string message = "";
                try
                {
                    message += "Page " + (page == 0 ? 1 : page) + ". To move to the next page, use \"!e list " + (page == 0 ? 2 : page + 1) + "\". To view information about a specific emote, use \"!e list <emote_ref>\".\n";
                    List<Emote> eList = emotes.getBatchEmotes(page, 5);
                    foreach(Emote emote in eList)
                    {
                        message += "```xl\nName: " + emote.name.Replace('\'', 'ꞌ'); //Replaces the apostraphe with a Latin Small Letter Saltillo (U+A78C) so it won't break Discord formatting (as much).
                        message += "\nReference: " + emote.reference;
                        message += "\nRequired Rank: " + emote.requiredRank;
                        message += "\nCreator: " + emote.creator + "\n```\n";
                    }
                    if(!message.Contains("Name")) { message = "http://i.imgur.com/zdMAeE9.png"; }
                }
                catch(XMLException x)
                {
                    if(x.errorCode == XMLErrorCode.NotFound) { await e.e.Channel.SendMessageAsync("http://i.imgur.com/zdMAeE9.png"); return; }
                }
                await e.e.Channel.SendMessageAsync(message);
                return;
            }
            else
            {*/
                Emote emote;
                try { emote = emotes.getEmote(e.reference); }
                catch(XMLException x)
                {
                    switch(x.errorCode)
                    {
                        case XMLErrorCode.NotFound: await e.e.Channel.SendMessageAsync("Error: Could not find requested emote."); Program.Log("Error: No emotes found for ref " + e.reference); break;
                        case XMLErrorCode.MultipleFound: await e.e.Channel.SendMessageAsync("Error: Found multiple emotes."); Program.Log("Error: Multiple emotes found for ref " + e.reference); break;
                    }
                    return;
                }
                await e.e.Channel.SendMessageAsync(emote.content);
                return;
            //}
        }
        
    }

    public class DiscordCommandArgs
    {
        public IUserMessage e;
        public IUser member;
        public string name;
        public string steam;
        public string discord;
        public string id;
        public string reference;
        public bool verbose;
        public string date;
        public string force;
        public bool ignore;
        public Rank requiredRank;

        /*public DiscordCommandArgs(MessageEventArgs e, User member = null, string name = null, string steam = null, string discord = null, string id = null, string reference = null, bool verbose = false, string date = null, string force = null, bool isForce = false, bool ignore = false)
        {
            this.e = e;
            this.member = member;
            this.name = name;
            this.steam = steam;
            this.discord = discord;
            this.id = id;
            this.reference = reference;
            this.verbose = verbose;
            this.date = date;
            this.force = force;
            this.isForce = isForce;
            this.ignore = ignore;
        }*/
    }

    public class CommandHandler
    {
        private CommandService commands;
        private DiscordSocketClient client;
        private IDependencyMap map;

        public async Task Install(IDependencyMap _map)
        {
            client = _map.Get<DiscordSocketClient>();
            commands = new CommandService();
            _map.Add(commands);
            map = _map;

            commands.AddTypeReader<Rank>(new TypeReaders.RankTypeReader());
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
            await commands.AddModuleAsync<Modules.EmoteModule>();
            await commands.AddModuleAsync<Modules.RandomCommandsModule>();
            await commands.AddModuleAsync<Modules.TaggingModule>();
            await commands.AddModuleAsync<Modules.CommandInfoModule>();
            await commands.AddModuleAsync<Modules.NexusStatsModule>();
            //await commands.AddModuleAsync<Warframe.WarframeAlertsModule>();
            //await commands.AddTypeReader<bool>(new TypeReaders.BooleanTypeReader);

            client.MessageReceived += HandleCommand;
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;
            if (!(message.HasMentionPrefix(client.CurrentUser, ref argPos) || message.HasCharPrefix('!', ref argPos))) return;

            var context = new CommandContext(client, message);
            var result = await commands.ExecuteAsync(context, argPos, map);

            if(!result.IsSuccess)
            {
                await message.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");
            }
        }
    }
}
