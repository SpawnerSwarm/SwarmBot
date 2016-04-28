using DiscordSharp;
using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.IO;
using System.Globalization;
using SwarmBot;
using Trileans;

namespace SwarmBot
{
    
    public class Rankup
    {
        public string rank { get; internal set; }
        public DateTime date { get; set; }
        public string placeHolderDate { get; set; }
        public bool usingPlaceHolder { get; set; }

        public Rankup(string rank, string date = null, bool old = true, bool NaN = false)
        {
            this.rank = rank;
            usingPlaceHolder = true;
            if(old)
            {
                placeHolderDate = "Old";
                this.date = new DateTime();
            } else if(NaN)
            {
                placeHolderDate = "NaN";
                this.date = new DateTime();
            } else
            {
                try
                {
                    this.date = DateTime.Parse(date);
                    usingPlaceHolder = false;
                    placeHolderDate = null;
                } catch
                {
                    Console.WriteLine("Error: incorrect date format");
                    this.date = new DateTime();
                    usingPlaceHolder = true;
                    placeHolderDate = "Old";
                }
            }
        }
        internal Rankup(XElement rankup)
        {
            rank = rankup.Attribute("name").Value;
            string date = rankup.Value;
            if(date == "Old" || date == "NaN")
            {
                this.date = new DateTime();
                usingPlaceHolder = true;
                placeHolderDate = date;
            } else
            {
                this.date = DateTime.Parse(date);
                usingPlaceHolder = false;
                placeHolderDate = null;
            }
        }
        public string getDate()
        {
            if(usingPlaceHolder)
            {
                return placeHolderDate;
            } else
            {
                return date.ToString();
            }
        }
        public XElement toXElement()
        {
            XElement rankup = new XElement("Rankup", new XAttribute("name", rank), Regex.Match(getDate(), @"(.+) [0-9]+:[0-9]+:[0-9]+ .M").Groups[1].Value);
            if(usingPlaceHolder)
            {
                rankup.SetValue(placeHolderDate);
            }
            return rankup;
        }
        public static implicit operator Rankup(XElement xE)
        {
            return new Rankup(xE);
        }
    }

    public class XMLMember
    {
        public string name { get; internal set; }
        public string rank { get; internal set; }
        private Rankup[] rankupHistory { get; set; }
        public string WFName { get; internal set; }
        public string SKName { get; internal set; }
        public string discordName { get; internal set; }
        public long discordId { get; internal set; }
        public string steamName { get; internal set; }
        private string steamIdAlpha { get; set; }
        private long steamIdNum { get; set; }
        internal XMLDocument x { get; set; }
        internal XElement xE { get; set; }

        internal XMLMember(XElement xE, XMLDocument x)
        {
            name = xE.Descendants("Name").ToArray()[0].Value;
            Console.WriteLine(name);
            rank = xE.Descendants("Rank").ToArray()[0].Value;
            Console.WriteLine(rank);
            this.xE = xE;
            this.x = x;
            List<XElement> rankups = new List<XElement>();
            rankups = xE.Descendants("Rankup").ToList();
            Console.WriteLine(rankups.ToArray()[0]);
            /*for(int i=0;i<rankups.ToArray().Length;i++)
            {
                //rankupHistory[0] = new Rankup(rankups[0]);
                rankupHistory[0] = rankups.ToArray()[0];
            }*/
            int i = 0;
            rankupHistory = new Rankup[rankups.Count];
            foreach(XElement rankup in rankups)
            {
                rankupHistory[i] = rankup;
                i = i + 1;
            }
            WFName = xE.Descendants("Warframe").ToArray()[0].Value;
            SKName = xE.Descendants("SpiralKnights").ToArray()[0].Value;
            discordName = xE.Descendants("Discord").ToArray()[0].Value;
            discordId = Int64.Parse(xE.Descendants("DiscordId").ToArray()[0].Value);
            steamName = xE.Descendants("Steam").ToArray()[0].Value;
            string steamId = xE.Descendants("Steam").ToArray()[0].Value;
            bool isNumerical = true;
            foreach(char c in steamId.ToCharArray())
            {
                if(!Char.IsDigit(c) || steamId == "")
                {
                    isNumerical = false;
                }
            }
            if(isNumerical && steamId != "")
            {
                steamIdNum = Int64.Parse(steamId);
                steamIdAlpha = null;
            } else
            {
                steamIdAlpha = steamId;
                steamIdNum = 0;
            }
        }

        public trilean getSteamId()
        {
            if(steamIdAlpha == null)
            {
                return new trilean(true, steamIdNum.ToString());
            } else if(steamIdNum == 0)
            {
                return new trilean(false, steamIdAlpha);
            } else
            {
                throw new Exception("Error: Steam Id parsing failed");
            }
        }
        public Rankup getRankup(string rank)
        {
            bool hasReturned = false;
            int hasRNum = -1;
            for(int i=0;i<rankupHistory.Length;i++)
            {
                if(rankupHistory[i].rank == rank)
                {
                    hasReturned = true;
                    hasRNum = i;
                    break;
                }
            }
            if(!hasReturned)
            {
                throw new Exception("Error: Rankup not found");
            } else
            {
                return rankupHistory[hasRNum];
            }
        }
        public bool checkPermissions(int i)
        {
            return Int32.Parse(x.document.Descendants("Define").Where(y => y.Attribute("name").Value == rank).ToArray()[0].Value) >= i;
        }
        public bool checkPermissions(string s)
        {
            try
            {
                return Int32.Parse(x.document.Descendants("Define").Where(y => y.Attribute("name").Value == rank).ToArray()[0].Value) >= x.getDefine(s);
            }
            catch
            {
                Console.WriteLine("Error: Invalid permissions level");
                throw new Exception("Erorr: Invalid Permissions level");
            }
        }
        public trilean checkReadyForRankUp()
        {
            DateTime rankupDate = DateTime.Now;
            trilean isReady = 2;
            double reqTime = 0;
            if(rank == "Guild Master")
            {
                trilean trilean = new trilean(false, false);
                trilean.embedded = "Max";
                return trilean;
            } else
            {
                reqTime = x.getDefine(rank + "LastRankUp");
            }
            /*for(int i=0;i<rankupHistory.Length;i++)
            {
                if(rankupHistory[i].getDate() != "Old" || rankupHistory[i].getDate() != "NaN")
                {
                    break;
                } else
                {
                    rankupDate = DateTime.Parse(rankupHistory[i].getDate());
                }
            }*/
            foreach(Rankup rankup in rankupHistory)
            {
                if(rankup.rank == rank)
                {
                    string s = rankup.getDate();

                    if(s != "Old")
                    {
                        rankupDate = DateTime.Parse(s);
                        double t = (DateTime.Now - rankupDate).TotalDays;
                        isReady = reqTime <= t;
                        isReady.embedded = t.ToString();
                    } else
                    {
                        isReady = new trilean(false, true);
                    }
                    break;
                }
            }
            /*if(isOldDate == "true")
            {
                isReady = new trilean(false, true);
            } else
            {
                double t = (DateTime.Now - rankupDate).TotalDays;
                isReady = reqTime <= t;
                isReady.embedded = t.ToString();
            }*/
            return isReady;
        }
        public trilean promote(DateTime date, string forceRank = null)
        {
            string rank;
            int rankInt;
            if (forceRank != null)
            {
                rankInt = x.getDefine(forceRank);
                rank = forceRank;
            }
            else
            {
                try
                {
                    if (this.rank == "Guild Master")
                    {
                        return new trilean(false, true, "Max");
                    }
                    else
                    {
                        rankInt = x.getDefine(this.rank) + 1;
                        rank = x.getRankName(rankInt);
                    }
                }
                catch (Exception exception)
                {
                    throw new Exception("Error: Invalid rank -- " + exception);
                }
            }
            return x.promote(this, rank, date);
            
        }
        public XElement toXElement()
        {
            XElement rankupHistory = new XElement("RankupHistory");
            for(int i=0;i<this.rankupHistory.Length;i++)
            {
                XElement rankup = this.rankupHistory[i].toXElement();
                rankupHistory.Add(rankup);
            }
            XElement member = new XElement("Member",
                new XElement("Name", name),
                new XElement("Rank", rank),
                rankupHistory,
                new XElement("Names",
                    new XElement("Warframe", WFName),
                    new XElement("SpiralKnights", SKName),
                    new XElement("Discord", discordName),
                    new XElement("DiscordId", discordId),
                    new XElement("Steam", steamName),
                    new XElement("SteamId", new XAttribute("numerical", getSteamId().table[0]), getSteamId().embedded)));
            return member;
                
        }
        public static implicit operator string(XMLMember x)
        {
            return x.name;
        }
    }
    public class XMLDocument
    {
        public XDocument document { get; internal set; }
        public string path { get; internal set; }

        public XMLDocument(string path)
        {
            document = XDocument.Load(path);
            this.path = path;
        }

        public XMLDocument(XDocument document, string path)
        {
            this.document = document;
            this.path = path;
        }

        public void Save(string path)
        {
            document.Save(path);
        }

        public XMLMember getMemberById(string id)
        {
            IEnumerable<XElement> memberNum = document.Descendants("Member").Where(x => x.Descendants("DiscordId").ToArray()[0].Value == id);
            XElement[] memberArray = memberNum.ToArray();
            if(memberArray.Length == 1)
            {
                XElement member = memberArray[0];
                return new XMLMember(member, this);
            } else
            {
                throw new Exception("Error: Multiple members were found");
                Console.WriteLine(memberArray.Length);
            }
        }
        public XMLMember getMemberByUsername(string username)
        {
            IEnumerable<XElement> memberNum = document.Descendants("Member").Where(x => x.Descendants("Discord").ToArray()[0].Value == username);
            XElement[] memberArray = memberNum.ToArray();
            if(memberArray.Length == 1)
            {
                XElement member = memberArray[0];
                return new XMLMember(member, this);
            } else
            {
                throw new Exception("Error: Multiple members were found");
            }
        }
        public int getDefine(string value)
        {
            IEnumerable<XElement> defineNum = document.Descendants("Define").Where(x => x.Attribute("name").Value == value);
            XElement[] defineArray = defineNum.ToArray();
            Console.WriteLine(value);
            Console.WriteLine(defineArray[0]);
            if(defineArray.Length == 1)
            {
                return Int32.Parse(defineArray[0].Value);
            } else
            {
                throw new Exception("Error: Multiple defines were found");
            }
        }
        public string getRankName(int value)
        {
            IEnumerable<XElement> defineNum = document.Descendants("Define").Where(x => x.Value == value.ToString()).Where(x => x.Attribute("for").Value == "Promotion");
            XElement[] defineArray = defineNum.ToArray();
            Console.WriteLine(defineArray.Length);
            if(defineArray.Length == 1)
            {
                return defineArray[0].Attribute("name").Value;
            } else
            {
                throw new Exception("Error: Multiple defines were found");
            }
        }
        public trilean promote(XMLMember member, string rank, DateTime date)
        {
            string name = member.name;
            XElement[] xMemberArray = document.Descendants("Member").Where(x => x.Descendants("Name").ToArray()[0].Value == name).ToArray();
            if(xMemberArray.Length != 1)
            {
                return new trilean(false, true, "Multiple");
            } else
            {
                XElement xMember = xMemberArray[0];
                xMember.Descendants("Rank").ToArray()[0].SetValue(rank);
                xMember.Descendants("Rankup").Where(x => x.Attribute("name").Value == rank).ToArray()[0].SetValue(date.ToString());
                Save(path);
                return new trilean(true, rank);
            }
        }
    }
}
