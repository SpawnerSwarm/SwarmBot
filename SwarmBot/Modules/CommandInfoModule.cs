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
using System.Xml.Linq;

namespace SwarmBot.Modules
{
    class CommandInfoModule : ModuleBase
    {
        [Command("help")]
        public async Task help()
        {
            CommandInfoDB infoDB = new CommandInfoDB(Config.CommandInfoDBPath);
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                .WithText("() = Required argument. [] = Optional argument.");
            EmbedBuilder builder = new EmbedBuilder()
                .WithFooter(footer)
                .WithColor(Rank.GuildMaster.color);
            foreach (DBGroup group in infoDB.groups)
            {
                builder.AddField(x =>
                {
                    x.Name = group.name;
                    string str = "";
                    foreach (IDBCommand command in group.commands)
                    {
                        str += $"{command.syntax}\n";
                    }
                    x.Value = str;
                });
            }
            await ReplyAsync("", false, builder.Build());
        }
        [Command("help")]
        public async Task help(string commandName)
        {
            CommandInfoDB infoDB = new CommandInfoDB(Config.CommandInfoDBPath);
            commandName = commandName.Replace("!", "");
            IDBCommand _command = infoDB.getCommandByName(commandName);
            EmbedBuilder builder;
            switch (_command.isSubModule)
            {
                case true:
                    builder = new EmbedBuilder()
                        .WithDescription($"**{_command.name}** -- **!{commandName}**")
                        .AddField(x =>
                        {
                            x.Name = "Available commands";
                            x.Value = ((DBSubModule)_command).getDescriptionForCommandUsed(commandName);
                        });
                    await ReplyAsync("", embed: builder.Build());
                    break;
                case false:
                    DBCommand command = (DBCommand)_command;
                    builder = new EmbedBuilder()
                        .WithColor(command.requiredRank.color)
                        .WithDescription($"**{command.name}** -- {command.syntax.Replace(command.name, commandName)}{(command.description == "" ? "" : $"\n\n{command.description}")}\n\nRequired Rank: {command.requiredRank}");
                    foreach (DBParameter parameter in command.parameters)
                    {
                        EmbedFieldBuilder fBuilder = new EmbedFieldBuilder()
                            .WithIsInline(true)
                            .WithName(parameter.name);
                        string s = $"{parameter.description}\n\nOptional: {parameter.optional.ToString()}";
                        if (parameter.prefix != null)
                        {
                            s += $"\n\nPossible Prefixes:";
                            foreach (string prefix in parameter.prefix.Split('|'))
                            {
                                s += $" {prefix} ";
                            }
                        }
                        s += $"\n\nType: {(parameter.hasContent ? parameter.valueType : "Empty")}";
                        builder.AddField(x =>
                        {
                            x.IsInline = true;
                            x.Name = parameter.name;
                            x.Value = s;
                        });
                        //fBuilder.WithValue(s);
                        //builder.AddField(x => x = fBuilder);
                    }
                    await ReplyAsync("", embed: builder.Build());
                    break;
            }
        }
    }
}
namespace SwarmBot.XML {

    public class CommandInfoDB
    {
        private XDocument _document;
        public XElement document { get
            {
                return _document.Element("Database");
            } }
        public string path { get; internal set; }
        public List<DBGroup> groups => getGroups();

        public CommandInfoDB(string path)
        {
            _document = XDocument.Load(path);
            this.path = path;
            /*groups = new List<Group>();
            foreach(XElement xE in document.Elements("Group"))
            {
                groups.Add(new Group(xE));
            }*/
        }

        public DBGroup getGroupByName(string groupName)
        {
            IEnumerable<XElement> xEs = document.Elements("Group").Where(x => x.Attribute("name").Value == groupName);
            if(xEs.Count() == 1)
            {
                return new DBGroup(xEs.First());
            } else
            {
                throw new Exception("No groups found by that name");
            }
        }

        public List<DBGroup> getGroups()
        {
            List<DBGroup> groups = new List<DBGroup>();
            foreach(XElement xE in document.Elements("Group"))
            {
                groups.Add(new DBGroup(xE));
            }
            return groups;
        }

        public IDBCommand getCommandByName(string commandName, bool searchAliases = true)
        {
            /*IEnumerable<XElement> xEs = document.Elements("Group").Elements("Command").Where(x => x.Element("Name").Value == commandName || x.Element("Aliases")?.Elements("Alias")?.Where(y => y.Value == commandName)?.Count() != 0);
            if (xEs.Count() == 1)
            {
                return new DBCommand(xEs.First());
            }
            else
            {
                xEs = document.Elements("Group").Elements("SubModule").Where(x => x.Element("Name").Value == commandName || x.Element("Aliases")?.Elements("Alias")?.Where(y => y.Value == commandName)?.Count() != 0);
                if(xEs.Count() == 1)
                {
                    return new DBSubModule(xEs.First());
                }
                else
                {
                    throw new Exception("No commands found by that name");
                }
            }*/
            IEnumerable<IDBCommand> commands = getCommands().Where(x => x.name == commandName || x.aliases.Contains(commandName));
            if(commands.Count() == 1)
            {
                return commands.First();
            }
            else if(commands.Count() < 1)
            {
                throw new Exception("No commands found by that name");
            }
            else
            {
                throw new Exception("Multiple commands found by that name");
            }
        }

        public List<IDBCommand> getCommands()
        {
            List<IDBCommand> commands = new List<IDBCommand>();
            foreach (XElement xE in document.Elements("Group").Elements("Command"))
            {
                commands.Add(new DBCommand(xE));
            }
            foreach(XElement xE in document.Elements("Group").Elements("SubModule"))
            {
                commands.Add(new DBSubModule(xE));
            }
            return commands;
        }

        public CommandInfoDB(XDocument document, string path)
        {
            this._document = document;
            this.path = path;
        }

        public void Save(string path)
        {
            _document.Save(path);
        }
    }

    public class DBGroup
    {
        internal string name;
        internal List<IDBCommand> commands;

        internal DBGroup(XElement x)
        {
            name = x.Attribute("name").Value;
            commands = new List<IDBCommand>();
            foreach(XElement xE in x.Elements("Command"))
            {
                commands.Add(new DBCommand(xE));
            }
            foreach(XElement xE in x.Elements("SubModule"))
            {
                commands.Add(new DBSubModule(xE));
            }
        }

        internal IDBCommand getCommandByName(string commandName)
        {
            IEnumerable<IDBCommand> commands = this.commands.Where(x => x.name == commandName || x.aliases.Contains(commandName));
            if(commands.Count() == 1)
            {
                return commands.First();
            }
            else
            {
                throw new Exception("No commands found by that name");
            }
        }
    }

    public class DBCommand : IDBCommand
    {
        public string name { get; set; }
        public string syntax {
            get
            {
                string str = $"**!{name}**";
                foreach(DBParameter parameter in parameters)
                {
                    char st = '|', en = '|';
                    switch(parameter.optional)
                    {
                        case false:
                            st = '(';
                            en = ')';
                            break;
                        case true:
                            st = '[';
                            en = ']';
                            break;
                    }
                    str += $" {st}{(parameter.prefix == null ? "" : $"*{parameter.prefix.Replace("|", "/")}* ")}{(parameter.hasContent ? parameter.name : "")}{en}";
                }
                return str;
            } set { } }
        public List<string> aliases { get; set; }
        public string description { get; set; }
        internal List<DBParameter> parameters;

        internal bool SwarmServerRequired;
        internal Rank requiredRank;

        public bool isSubModule { get; set; }

        internal DBCommand(XElement x)
        {
            name = x.Element("Name").Value;
            //syntax = x.Element("Syntax").Value;
            aliases = new List<string>();
            foreach(XElement xE in x.Element("Aliases")?.Elements("Alias"))
            {
                aliases.Add(xE.Value);
            }
            description = x.Element("Description").Value;
            parameters = new List<DBParameter>();
            foreach(XElement xE in x.Element("Parameters").Elements("Parameter"))
            {
                parameters.Add(new DBParameter(xE));
            }

            SwarmServerRequired = x.Element("Requirements").Element("SwarmServerRequired") != null;
            if(x.Element("Requirements").Element("RequiredRank") != null)
            {
                requiredRank = x.Element("Requirements").Element("RequiredRank").Value;
            }
            else
            {
                requiredRank = Rank.Recruit;
            }
            isSubModule = false;
        }
    }

    public class DBParameter
    {
        internal string name;
        internal bool optional;
        internal string description;
        internal string prefix;
        internal bool hasContent;
        internal string valueType;

        internal DBParameter(XElement x)
        {
            name = x.Attribute("name").Value;
            optional = x.Attribute("optional") != null;
            description = x.Value;
            prefix = x.Attribute("prefix")?.Value;
            hasContent = !bool.TryParse(x.Attribute("hasContent")?.Value, out hasContent);
            valueType = x.Attribute("type").Value;
        }
    }

    public class DBSubModule : IDBCommand
    {
        public string name { get; set; }
        public List<string> aliases { get; set; }
        public string syntax
        {
            get
            {
                return $"**!{name}** -- More information available on {name} commands";
            }
            set
            {
                
            }
        }
        public bool isSubModule { get; set; }
        public string description { get; set; }

        internal DBSubModule(XElement x)
        {
            name = x.Element("Name").Value;
            aliases = new List<string>();
            foreach(XElement xE in x.Element("Aliases")?.Elements("Alias"))
            {
                aliases.Add(xE.Value);
            }
            isSubModule = true;
            description = x.Element("Description").Value.Replace("\\t", "\t");
        }

        public string getDescriptionForCommandUsed(string command)
        {
            if(command.StartsWith("!"))
            {
                command = command.Substring(1);
            }
            return description.Replace("{initialCommandUsed}", command);
        }
    }

    public interface IDBCommand
    {
        string name { get; set; }
        List<string> aliases { get; set; }
        string syntax { get; set; }
        bool isSubModule { get; set; }
        string description { get; set; }
    }
}
