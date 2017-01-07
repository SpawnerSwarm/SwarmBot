using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Xml.Linq;
using Discord.WebSocket;
using Discord.Commands;
using System.Text.RegularExpressions;

namespace SwarmBot.Warframe
{
    public partial class WarframeAlertsModule : ModuleBase
    {
        public static async Task newAlertReceived(SocketMessage e)
        {
            AlertsDB db = new AlertsDB(Config.AlertsDBPath);
            string[] msgParts = e.Content.Split(new char[] { ';' });
            string alertText = msgParts[0];
            string link = msgParts[1];

            string[] keywords = alertText.Replace("(", "").Replace(")", "").Replace(":", "").Replace("- ", "").Split(new char[] { ' ' });
            foreach(string keyword in keywords)
            {
                List<XElement> kList = db.keywords.Where(x => Regex.IsMatch(x.Attribute("key").Value, keyword, RegexOptions.IgnoreCase)).ToList();
                
                if(kList.Count == 1)
                {
                    List<XElement> members = kList[0].Elements("Member").ToList();
                    foreach (XElement member in members)
                    {
                        await Discord.getDiscordMemberByID(member.Value, Discord.client.GetGuild(ulong.Parse(Config.discordSwarmServerId))).CreateDMChannelAsync().GetAwaiter().GetResult().SendMessageAsync(alertText);
                    }
                }
            }
        } 
    }

    class AlertsDB
    {
        public XDocument x;
        internal List<XElement> keywords;
        private List<XElement> members;
        public string path;

        public AlertsDB(string path)
        {
            x = XDocument.Load(path);
            this.path = path;

            keywords = x.Element("Database").Element("Keywords").Elements("Keyword").ToList();
            members = x.Element("Database").Element("Members").Elements("Member").ToList();
        }

        public async Task<Keyword> getOrCreateKeyword(string key)
        {
            IEnumerable<XElement> xEs = keywords.Where(x => x.Attribute("key")?.Value == key);
            if(xEs?.Count() != 0)
            {
                return new Keyword(xEs.First().Attribute("key").Value, this);
            }
            else
            {
                x.Element("Database").Element("Keywords").Add(new XElement("Keyword", new XAttribute("key", key)));
                Save(Config.AlertsDBPath);
                return new Keyword(key, this);
            }
        }

        public void Save(string path)
        {
            x.Save(path);

            keywords = x.Element("Database").Element("Keywords").Elements("Keyword").ToList();
            members = x.Element("Database").Element("Members").Elements("Member").ToList();
        }

        public async Task<Keyword> getKeyword(string key)
        {
            return new Keyword(key, this);
        }

        public async Task<List<Keyword>> getKeywordsForMember(ulong memberID)
        {
            List<XElement> xEs = members.Where(x => x.Element("id").Value == memberID.ToString()).First().Elements("Keyword").ToList();
            List<Keyword> returns = new List<Keyword>();
            foreach(XElement xE in xEs)
            {
                returns.Add(new Keyword(xE.Attribute("key").Value, this));
            }
            return returns;
        }

        public async Task addKeywordToMember(ulong memberID, Keyword keyword)
        {
            XElement member = members.Where(x => x.Element("id").Value == memberID.ToString()).First();
            member.Add(new XElement("Keyword", new XAttribute("key", keyword.key)));
            x.Save(Config.AlertsDBPath);
        }

        public async Task removeKeywordFromMember(ulong memberID, Keyword keyword)
        {
            members.Where(x => x.Element("id").Value == memberID.ToString()).Elements("Keyword").Where(x => x.Attribute("key").Value == keyword.key).Remove();
            x.Save(Config.AlertsDBPath);
        }

        public async Task createMemberIfNull(IUser member)
        {
            if(members.Where(x => x?.Element("id")?.Value == member.Id.ToString())?.Count() == 0)
            {
                x.Element("Database").Element("Members").Add(new XElement("Member", new XAttribute("name", member.Username), new XElement("id", member.Id)));
                x.Save(Config.AlertsDBPath);
            }
        }

        public async Task removeKeywordIfEmpty(Keyword keyword)
        {
            XElement xE = keywords.Where(x => x.Attribute("key").Value == keyword.key).First();
            if (xE.HasElements) { return; }
            xE.Remove();
            Save(Config.AlertsDBPath);
        }

        public async Task<bool> memberExists(ulong id)
        {
            return members.Where(x => x?.Element("id")?.Value == id.ToString())?.Count() != 0;
        }
    }

    class Keyword
    {
        public List<ulong> ids;
        public string key;

        public Keyword(string key, AlertsDB db)
        {
            this.key = key;
            ids = new List<ulong>();
            IEnumerable<XElement> xEs = db.keywords.Where(x => x.Attribute("key")?.Value == key).Elements("Member");
            foreach(XElement xE in xEs)
            {
                ids.Add(ulong.Parse(xE.Value));
            }
        }

        public async Task addMemberToTracking(IUser member, AlertsDB db)
        {
            db.keywords.Where(x => x.Attribute("key")?.Value == key).First().Add(new XElement("Member", new XAttribute("name", member.Username), member.Id));
            db.Save(Config.AlertsDBPath);
        }

        public async Task removeMemberFromTracking(IUser member, AlertsDB db)
        {
            db.keywords.Where(x => x.Attribute("key")?.Value == key).First().Elements("Member").Where(x => x.Value == member.Id.ToString()).Remove();
            db.Save(Config.AlertsDBPath);
        }
    }
}
