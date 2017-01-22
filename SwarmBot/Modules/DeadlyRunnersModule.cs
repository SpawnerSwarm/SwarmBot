using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Modules;
using Discord.Net;
using Discord;
using Discord.Commands;
using SwarmBot.XML;
using System.Xml.Linq;
using SwarmBot.Modules;

namespace SwarmBot.Modules
{
    public partial class BasicMemberDBModule : ModuleBase
    {
        [Group("runners"), Alias("deadlyrunners", "wfmastery")]
        public class DeadlyRunnersModule : ModuleBase
        {
            [Command]
            public async Task Default()
            {
                CommandInfoDB db = new CommandInfoDB(Config.CommandInfoDBPath);
                string str = "Available commands are:\n";
                str += ((DBSubModule)db.getCommandByName("deadlyRunners")).getDescriptionForCommandUsed(Context.Message.Content);
                await ReplyAsync(str);
            }

            [Command("list"), Alias("all"), Summary("Show all weapons and members who have mastered them.")]
            public async Task List()
            {
                XMLDocument memberDB = new XMLDocument(Config.MemberDBPath);
                List<Weapon> weapons = memberDB.getAllWeapons();
                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle("Deadly Runners")
                    .WithColor(Rank.General.color);
                foreach (Weapon weapon in weapons)
                {
                    builder.AddField(x =>
                    {
                        x.Name = weapon.name;
                        x.Value = weapon.listMembers();
                    });
                }
                await ReplyAsync("", embed: builder.Build());
            }

            [Command("viewUser"), Alias("user", "showUser", "infoUser"), Summary("Show the mastery for a specific user"), RequireContext(ContextType.Guild)]
            public async Task ViewUser(IUser user)
            {
                XMLDocument memberDB = new XMLDocument(Config.MemberDBPath);
                XMLMember member = memberDB.getMemberById(user);
                EmbedBuilder builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(user.AvatarUrl)
                        .WithName(member.name))
                    .WithColor(Rank.General.color);
                foreach (Weapon weapon in member.getMastery())
                {
                    builder.Description += $"{weapon.name}\n";
                }
                await ReplyAsync("", embed: builder.Build());
            }

            [Command("viewWeapon"), Alias("weapon", "showWeapon", "infoWeapon"), Summary("Show the users who have mastery over a specific weapon")]
            public async Task ViewWeapon(string str)
            {
                XMLDocument memberDB = new XMLDocument(Config.MemberDBPath);
                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle(str)
                    .WithColor(Rank.General.color)
                    .WithDescription(memberDB.getWeapon(str).listMembers());
                await ReplyAsync("", embed: builder.Build());
            }

            [Command("update"), Alias("grant", "giveMastery", "master"), Summary("Update the mastery of a specific user"), RequireContext(ContextType.Guild), RequireUserId(156962731084349442)]
            public async Task Update(IUser user, string str)
            {
                XMLDocument memberDB = new XMLDocument(Config.MemberDBPath);
                XMLMember member = memberDB.getMemberById(user);
                Weapon weapon = memberDB.getWeapon(memberDB.getOrCreateWeapon(str));
                if (weapon._members.Contains(member.discordId.ToString()))
                {
                    memberDB.getOrCreateWeapon(str).Elements("Member").Where(x => x.Value == member.discordId.ToString()).First().Remove();
                    memberDB.Save(memberDB.path);
                    await ReplyAsync($"Removed mastery of {str} from {member.name}");
                    return;
                }
                member.updateMastery(weapon.name);
                await ReplyAsync($"Updated mastery for {member.name}");
            }

            public class Weapon
            {
                public string name;
                public List<string> _members;
                public List<XMLMember> members
                {
                    get
                    {
                        List<XMLMember> members = new List<XMLMember>();
                        foreach (string str in _members)
                        {
                            members.Add(document.getMemberById(str));
                        }
                        return members;
                    }
                    set { }
                }
                XMLDocument document;

                public Weapon(string name, List<string> members, XMLDocument document)
                {
                    this.name = name;
                    this._members = members;
                    this.document = document;
                }

                public string listMembers()
                {
                    string str = "";
                    foreach (XMLMember member in members)
                    {
                        str += $"{member.name}\n";
                    }
                    if (str == "") { str = "None"; }
                    return str;
                }
            }
        }
    }
}

namespace SwarmBot.XML
{
    public partial class XMLMember
    {
        public IEnumerable<Modules.BasicMemberDBModule.DeadlyRunnersModule.Weapon> getMastery()
        {
            return x.getWeaponsForMember(discordId.ToString());
        }

        public void updateMastery(string name)
        {
            XElement weapon = x.getOrCreateWeapon(name);
            weapon.Add(new XElement("Member", discordId));
            x.Save(x.path);
        }
    }

    public partial class XMLDocument
    {
        public BasicMemberDBModule.DeadlyRunnersModule.Weapon getWeapon(XElement weapon)
        {
            if (weapon == null) { throw new Exception("No weapon found by that name"); }
            List<string> strList = new List<string>();
            foreach (XElement member in weapon.Elements("Member"))
            {
                strList.Add(member.Value);
            }
            return new BasicMemberDBModule.DeadlyRunnersModule.Weapon(weapon.Attribute("name").Value, strList, this);
        }

        public BasicMemberDBModule.DeadlyRunnersModule.Weapon getWeapon(string name)
        {
            try
            {
                XElement weapon = document.Element("Database").Element("DeadlyRunners").Elements("Weapon").Where(x => x.Attribute("name").Value == name).First();
                return getWeapon(weapon);
            }
            catch
            {
                throw new Exception("No weapon found by that name");
            }
        }

        public List<BasicMemberDBModule.DeadlyRunnersModule.Weapon> getAllWeapons()
        {
            IEnumerable<XElement> xEs = document.Element("Database").Element("DeadlyRunners").Elements("Weapon");
            List<BasicMemberDBModule.DeadlyRunnersModule.Weapon> weapons = new List<BasicMemberDBModule.DeadlyRunnersModule.Weapon>();
            foreach (XElement weapon in xEs)
            {
                weapons.Add(getWeapon(weapon));
            }
            return weapons;
        }

        public IEnumerable<BasicMemberDBModule.DeadlyRunnersModule.Weapon> getWeaponsForMember(string id)
        {
            return getAllWeapons().Where(x => x._members.Contains(id));
        }

        public XElement getOrCreateWeapon(string name)
        {
            XElement runners = document.Element("Database").Element("DeadlyRunners");
            XElement[] xEs = runners.Elements("Weapon").Where(x => x.Attribute("name").Value == name).ToArray();
            if (xEs.Length == 1)
            {
                return xEs.First();
            }
            else
            {
                runners.Add(new XElement("Weapon", new XAttribute("name", name)));
                Save(path);
                return runners.Elements("Weapon").Where(x => x.Attribute("name").Value == name).First();
            }
        }
    }
}
