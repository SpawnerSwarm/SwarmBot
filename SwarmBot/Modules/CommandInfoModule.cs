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
            foreach(Group group in infoDB.groups)
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
            IDBCommand _command = infoDB.getCommandByName(commandName.Replace("!", ""));
            EmbedBuilder builder;
            switch(_command.isSubModule)
            {
                case true:
                    builder = new EmbedBuilder()
                        .WithColor(Rank.GuildMaster.color)
                        .WithDescription(_command.syntax);
                    await ReplyAsync("", embed: builder.Build());
                    break;
                case false:
                    Command command = (Command)_command;
                    builder = new EmbedBuilder()
                        .WithColor(command.requiredRank.color)
                        .WithDescription($"{command.syntax}{(command.description == "" ? "" : $"\n\n{command.description}")}\n\nRequired Rank: {command.requiredRank}");
                    foreach(Parameter parameter in command.parameters)
                    {
                        EmbedFieldBuilder fBuilder = new EmbedFieldBuilder()
                            .WithIsInline(true)
                            .WithName(parameter.name);
                        string s = $"{parameter.description}\n\nOptional: {parameter.optional.ToString()}";
                        if(parameter.prefix != null)
                        {
                            s += $"\n\nPossible Prefixes:";
                            foreach(string prefix in parameter.prefix.Split('|'))
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

    internal class CommandInfoDB
    {
        private XDocument _document;
        public XElement document { get
            {
                return _document.Element("Database");
            } }
        public string path { get; internal set; }
        public List<Group> groups => getGroups();

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

        public Group getGroupByName(string groupName)
        {
            IEnumerable<XElement> xEs = document.Elements("Group").Where(x => x.Attribute("name").Value == groupName);
            if(xEs.Count() == 1)
            {
                return new Group(xEs.First());
            } else
            {
                throw new Exception("No groups found by that name");
            }
        }

        public List<Group> getGroups()
        {
            List<Group> groups = new List<Group>();
            foreach(XElement xE in document.Elements("Group"))
            {
                groups.Add(new Group(xE));
            }
            return groups;
        }

        public IDBCommand getCommandByName(string commandName)
        {
            IEnumerable<XElement> xEs = document.Elements("Group").Elements("Command").Where(x => x.Element("Name").Value == commandName);
            if (xEs.Count() == 1)
            {
                return new Command(xEs.First());
            }
            else
            {
                xEs = document.Elements("Group").Elements("SubModule").Where(x => x.Element("Name").Value == commandName);
                if(xEs.Count() == 1)
                {
                    return new SubModule(xEs.First());
                }
                else
                {
                    throw new Exception("No commands found by that name");
                }
            }
        }

        public List<IDBCommand> getCommands()
        {
            List<IDBCommand> commands = new List<IDBCommand>();
            foreach (XElement xE in document.Elements("Group").Elements("Command"))
            {
                commands.Add(new Command(xE));
            }
            foreach(XElement xE in document.Elements("Group").Elements("SubModule"))
            {
                commands.Add(new SubModule(xE));
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

    internal class Group
    {
        internal string name;
        internal List<IDBCommand> commands;

        internal Group(XElement x)
        {
            name = x.Attribute("name").Value;
            commands = new List<IDBCommand>();
            foreach(XElement xE in x.Elements("Command"))
            {
                commands.Add(new Command(xE));
            }
            foreach(XElement xE in x.Elements("SubModule"))
            {
                commands.Add(new SubModule(xE));
            }
        }

        internal IDBCommand getCommandByName(string commandName)
        {
            IEnumerable<IDBCommand> commands = this.commands.Where(x => x.name == commandName);
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

    internal class Command : IDBCommand
    {
        public string name { get; set; }
        public string syntax {
            get
            {
                string str = $"**!{name}**";
                foreach(Parameter parameter in parameters)
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
        internal string description;
        internal List<Parameter> parameters;

        internal bool SwarmServerRequired;
        internal Rank requiredRank;

        public bool isSubModule { get; set; }

        internal Command(XElement x)
        {
            name = x.Element("Name").Value;
            //syntax = x.Element("Syntax").Value;
            aliases = new List<string>();
            foreach(XElement xE in x.Element("Aliases")?.Elements("Alias"))
            {
                aliases.Add(xE.Value);
            }
            description = x.Element("Description").Value;
            parameters = new List<Parameter>();
            foreach(XElement xE in x.Element("Parameters").Elements("Parameter"))
            {
                parameters.Add(new Parameter(xE));
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

    internal class Parameter
    {
        internal string name;
        internal bool optional;
        internal string description;
        internal string prefix;
        internal bool hasContent;
        internal string valueType;

        internal Parameter(XElement x)
        {
            name = x.Attribute("name").Value;
            optional = x.Attribute("optional") != null;
            description = x.Value;
            prefix = x.Attribute("prefix")?.Value;
            hasContent = !bool.TryParse(x.Attribute("hasContent")?.Value, out hasContent);
            valueType = x.Attribute("type").Value;
        }
    }

    internal class SubModule : IDBCommand
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

        internal SubModule(XElement x)
        {
            name = x.Element("Name").Value;
            aliases = new List<string>();
            foreach(XElement xE in x.Element("Aliases")?.Elements("Alias"))
            {
                aliases.Add(xE.Value);
            }
            isSubModule = true;
        }
    }

    internal interface IDBCommand
    {
        string name { get; set; }
        List<string> aliases { get; set; }
        string syntax { get; set; }
        bool isSubModule { get; set; }
    }
}
