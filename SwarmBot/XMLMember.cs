using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Trileans;

namespace SwarmBot.XML
{
    class XMLMember
    {
        public string name, WFName, SKName, discordName, steamName;
        public long discordId;
        public short forma;
        //private long steamId;

        public Rank rank;
        public List<Rankup> rankupHistory = new List<Rankup>();

        internal XMLDocument x;
        internal XElement xE;

        internal XMLMember(XElement xE, XMLDocument x)
        {
            name = xE.Element("Name").Value;
            rank = xE.Element("Rank").Value;
            this.xE = xE;
            this.x = x;

            foreach(XElement rankup in xE.Element("RankupHistory").Descendants("Rankup").ToList())
            {
                rankupHistory.Add(new Rankup(rankup));
            }

            XElement names = xE.Element("Names");
            WFName = names.Element("Warframe").Value;
            SKName = names.Element("SpiralKnights").Value;
            discordName = names.Element("Discord").Value;
            discordId = Int64.Parse(names.Element("DiscordId").Value);
            steamName = names.Element("Steam").Value;

            /*string steamId = names.Element("SteamId").Value;
            bool isNumerical = true;
            foreach(char c in steamId.ToCharArray())
            {
                if(!Char.IsDigit(c) || steamId == "")
                {
                    isNumerical = false;
                }
            }
            if(steamId == "") { isNumerical = false; }
            switch(isNumerical)
            {
                case true:
                    steamIdNum = Int64.Parse(steamId);
                    steamIdAlpha = null;
                    break;
                case false:
                    steamIdAlpha = steamId;
                    steamIdNum = 0;
                    break;
            }*/
            //string steamIdStr = names.Element("SteamId").Value;
            //steamId = steamIdStr == "" ? 0 : Int64.Parse(steamIdStr);

            forma = short.Parse(xE.Element("FormaDonated").Value);
        }

        public trilean checkReadyForRankup()
        {
            double reqTime = 0;
            if(rank == Rank.GuildMaster) { return new trilean(false, true, XMLErrorCode.Maximum); }
            else { reqTime = x.getDefine(rank + "LastRankUp", DefineType.LastRankUp); }

            string s = rankupHistory.Where(x => x.rank == rank).First().getDate();
            if(s != "Old" && s != "NaN")
            {
                double t = (DateTime.Now - DateTime.Parse(s)).TotalDays;
                return new trilean(reqTime <= t, t.ToString());
            }
            else { return new trilean(false, true, XMLErrorCode.Old); }
        }

        public bool checkPermissions(Rank s)
        {
            try
            { return x.getDefine(rank, DefineType.Promotion) >= x.getDefine(s, DefineType.Promotion); }
            catch(Exception) { throw new XMLException(XMLErrorCode.Unknown, "Unknown rank"); }
        }

        public trilean Promote(DateTime date, Rank rank)
        {
            xE.Element("Rank").SetValue(rank);
            xE.Element("RankupHistory").Elements().Where(x => x.Attribute("name").Value == rank).First().SetValue(date.ToString());
            xE.Element("FormaDonated").SetValue(0);
            x.Save(x.path);
            return new trilean(true, rank);
        }
    }
}
