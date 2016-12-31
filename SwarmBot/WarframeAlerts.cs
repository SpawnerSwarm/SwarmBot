using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Xml.Linq;

namespace SwarmBot.Warframe
{
    class WarframeAlerts
    {
        public static void newAlertReceived(MessageEventArgs e)
        {
            AlertsDB db = new AlertsDB(Config.AlertsDBPath);
            string[] msgParts = e.Message.RawText.Split(new char[] { ';' });
            string alertText = msgParts[0];
            string link = msgParts[1];

            string[] keywords = alertText.Replace("(", "").Replace(")", "").Replace(":", "").Replace("- ", "").Split(new char[] { ' ' });
            foreach(string keyword in keywords)
            {
                List<XElement> kList = db.keywords.Where(x => x.Attribute("key").Value == keyword).ToList();
                
                if(kList.Count == 1)
                {
                    List<XElement> members = kList[0].Elements("Member").ToList();
                    foreach (XElement member in members)
                    {
                        Discord.getDiscordMemberByID(member.Value, Discord.client.GetServer(ulong.Parse(Config.discordSwarmServerId))).SendMessage(alertText);
                    }
                }
            }
        } 
    }

    class AlertsDB
    {
        public XDocument x;
        public List<XElement> keywords;
        public string path;

        public AlertsDB(string path)
        {
            x = XDocument.Load(path);
            this.path = path;

            keywords = x.Element("Database").Elements("Keyword").ToList();
        }
    }
}
