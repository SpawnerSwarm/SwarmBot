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
                .WithFooter(footer);
            foreach(Group group in infoDB.groups)
            {
                builder.AddField(x =>
                {
                    x.Name = group.name;
                    string str = "";
                    foreach (Command command in group.commands)
                    {
                        str += $"{command.syntax}\n";
                    }
                    x.Value = str;
                });
            }
            await ReplyAsync("", false, builder.Build());
        }
    }

    internal class CommandInfoDB
    {
        public XDocument document { get; internal set; }
        public string path { get; internal set; }
        public List<Group> groups;

        public CommandInfoDB(string path)
        {
            document = XDocument.Load(path);
            this.path = path;
            groups = new List<Group>();
            foreach(XElement xE in document.Element("Database").Elements("Group"))
            {
                groups.Add(new Group(xE));
            }
        }

        public CommandInfoDB(XDocument document, string path)
        {
            this.document = document;
            this.path = path;
        }

        public void Save(string path)
        {
            document.Save(path);
        }
    }

    internal class Group
    {
        internal string name;
        internal List<Command> commands;

        internal Group(XElement x)
        {
            name = x.Attribute("name").Value;
            commands = new List<Command>();
            foreach(XElement xE in x.Elements("Command"))
            {
                commands.Add(new Command(xE));
            }
        }
    }

    internal class Command
    {
        internal string name;
        internal string syntax;
        internal List<string> aliases;
        internal string description;
        internal List<Parameter> parameters;

        internal bool SwarmServerRequired;
        internal Rank requiredRank;

        internal Command(XElement x)
        {
            name = x.Element("Name").Value;
            syntax = x.Element("Syntax").Value;
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
        }
    }

    internal class Parameter
    {
        internal string name;
        internal bool optional;
        internal string description;

        internal Parameter(XElement x)
        {
            name = x.Attribute("name").Value;
            optional = x.Attribute("optional") != null;
            description = x.Value;
        }
    }
}
