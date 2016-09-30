﻿using DiscordSharp;
using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Trileans;

namespace SwarmBot.XML
{
    
    public class Rankup
    {
        public Rank rank { get; internal set; }
        public DateTime date { get; set; }
        public string placeHolderDate { get; set; }
        public bool usingPlaceHolder { get; set; }

        public Rankup(Rank rank, string date = null, bool old = true, bool NaN = false)
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
        public Rank rank { get; internal set; }
        /// <summary>
        /// The Rankup history of the member consisting of <see cref="Rankup"/> objects
        /// </summary>
        private Rankup[] rankupHistory { get; set; }
        public string WFName { get; internal set; }
        public string SKName { get; internal set; }
        public string discordName { get; internal set; }
        public long discordId { get; internal set; }
        public string steamName { get; internal set; }
        private string steamIdAlpha { get; set; }
        private long steamIdNum { get; set; }
        public short forma { get; set; }
        /// <summary>
        /// The parent <see cref="XMLDocument"/> of the <see cref="XElement"/> (<see cref="xE"/>) of the member
        /// </summary>
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
            forma = short.Parse(xE.Descendants("FormaDonated").ToArray()[0].Value);
        }

        /// <summary>
        /// Returns the numerical or alphabetical Steam ID of the member.
        /// </summary>
        /// <returns><see cref="trilean"/>. Outer bool <see cref="true"/> if numerical. Embedded is the ID</returns>
        /// <remarks>Will throw an error if the parsing fails</remarks>
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
        public Rankup getRankup(Rank rank)
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
        public bool checkPermissions(Rank s)
        {
            try
            {
                return checkPermissions(x.getDefine(s, DefineType.Promotion));
            }
            catch
            {
                Console.WriteLine("Error: Invalid permissions level");
                throw new Exception("Erorr: Invalid Permissions level");
            }
        }
        public bool checkPermissions(int i)
        {
            return int.Parse(x.document.Descendants("Define").Where(y => y.Attribute("name").Value == rank).ToArray()[0].Value) >= i;
        }
        public trilean checkReadyForRankUp()
        {
            DateTime rankupDate = DateTime.Now;
            trilean isReady = false;
            double reqTime = 0;
            if(rank == Rank.GuildMaster)
            {
                trilean trilean = new trilean(false, true, XMLErrorCode.Maximum);
                return trilean;
            } else
            {
                reqTime = x.getDefine(rank + "LastRankUp", DefineType.LastRankUp);
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
                        isReady = new trilean(false, true, XMLErrorCode.Old);
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
        public trilean promote(DateTime date, XMLMember m, Rank forceRank = null)
        {
            Rank rank;
            int rankInt;
            if (forceRank != null)
            {
                rankInt = x.getDefine(forceRank, DefineType.Promotion);
                rank = forceRank;
            }
            else
            {
                try
                {
                    if (this.rank == Rank.GuildMaster)
                    {
                        return new trilean(false, true, XMLErrorCode.Maximum);
                    }
                    else
                    {
                        rankInt = x.getDefine(this.rank, DefineType.Promotion) + 1;
                        rank = x.getDefineName(rankInt, DefineType.Promotion);
                    }
                }
                catch (Exception exception)
                {
                    throw new Exception("Error: Invalid rank -- " + exception);
                }
            }
            if (x.getDefine(m.rank, DefineType.Promotion) > x.getDefine(rank, DefineType.Promotion) || m.rank == Rank.GuildMaster)
            {
                return x.promote(this, rank, date);
            } else
            {
                return new trilean(false, true, XMLErrorCode.Greater);
            }
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
                    new XElement("SteamId", new XAttribute("numerical", getSteamId().table[0]), getSteamId().embedded)),
                new XElement("FormaDonated", forma));
            return member;
                
        }
        public static implicit operator string(XMLMember x)
        {
            return x.name;
        }
        public trilean addForma(short formas)
        {
            return x.addForma(this, formas);
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
            }
        }
        public XMLMember getMemberById(ulong id)
        {
            return getMemberById(id.ToString());
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
        public int getDefine(string value, DefineType _for)
        {
            IEnumerable<XElement> defineNum = document.Descendants("Define").Where(x => x.Attribute("name").Value == value).Where(x => x.Attribute("for").Value == _for);
            XElement[] defineArray = defineNum.ToArray();
            Console.WriteLine(value);
            Console.WriteLine(defineArray[0]);
            if(defineArray.Length == 1)
            {
                return Int32.Parse(defineArray[0].Value);
            } else
            {
                throw new Exception("Error: Multiple defines were found: getDefine() ln 402");
            }
        }
        public string getDefineName(int value, DefineType _for)
        {
            IEnumerable<XElement> defineNum = document.Descendants("Define").Where(x => x.Value == value.ToString()).Where(x => x.Attribute("for").Value == _for);
            XElement[] defineArray = defineNum.ToArray();
            Console.WriteLine(defineArray.Length);
            if(defineArray.Length == 1)
            {
                return defineArray[0].Attribute("name").Value;
            } else
            {
                throw new Exception("Error: Multiple defines were found: getDefineName() ln 415");
            }
        }
        public trilean promote(XMLMember member, Rank rank, DateTime date)
        {
            string name = member.name;
            XElement[] xMemberArray = document.Descendants("Member").Where(x => x.Descendants("Name").ToArray()[0].Value == name).ToArray();
            if(xMemberArray.Length != 1)
            {
                return new trilean(false, true, XMLErrorCode.MultipleFound);
            } else
            {
                XElement xMember = xMemberArray[0];
                xMember.Descendants("Rank").ToArray()[0].SetValue(rank);
                xMember.Descendants("Rankup").Where(x => x.Attribute("name").Value == rank).ToArray()[0].SetValue(date.ToString());
                resetForma(member);
                Save(path);
                return new trilean(true, rank);
            }
        }
        public trilean addForma(XMLMember member, short formas)
        {
            XElement[] xMemberArray = document.Descendants("Member").Where(x => x.Descendants("Name").ToArray()[0].Value == member).ToArray();
            if(xMemberArray.Length != 1)
            {
                return new trilean(false, true, XMLErrorCode.MultipleFound);
            } else
            {
                XElement xMember = xMemberArray[0];
                xMember.Descendants("FormaDonated").ToArray()[0].SetValue(short.Parse(xMember.Descendants("FormaDonated").ToArray()[0].Value) + formas);
                Save(path);
                return new trilean(true);
            }
        }
        public trilean resetForma(XMLMember member)
        {
            XElement[] xMemberArray = document.Descendants("Member").Where(x => x.Descendants("Name").ToArray()[0].Value == member).ToArray();
            if (xMemberArray.Length != 1)
            {
                return new trilean(false, true, XMLErrorCode.MultipleFound);
            }
            else
            {
                XElement xMember = xMemberArray[0];
                xMember.Descendants("FormaDonated").ToArray()[0].SetValue(0);
                Save(path);
                return new trilean(true);
            }
        }
        public trilean checkRankMaxed(XMLMember member, Rank toRank = null)
        {
            if(toRank == null)
            {
                if (member.rank != Rank.GuildMaster) { toRank = getDefineName(getDefine(member.rank, DefineType.Promotion) + 1, DefineType.Promotion); } else { return true; }
            }
            try
            {
                short capacity = (short)getDefine(toRank, DefineType.RankCapacity);
                int memberCount = document.Descendants("Member").Where(x => x.Descendants("Rank").ToArray()[0].Value == toRank).ToArray().Length;
                Console.WriteLine(capacity + "|" + memberCount);
                if (memberCount >= capacity)
                {
                    return new trilean(false, memberCount);
                }
                else
                {
                    return new trilean(true, memberCount);
                }
            }
            catch
            {
                return new trilean(false, true, XMLErrorCode.Unknown);
            }
        }
    }
    public sealed class DefineType
    {
        private readonly string name;
        private readonly int value;

        public static readonly DefineType Promotion = new DefineType(0, "Promotion");
        public static readonly DefineType RankCapacity = new DefineType(1, "RankCapacity");
        public static readonly DefineType LastRankUp = new DefineType(2, "LastRankUp");

        private DefineType(int value, string name)
        {
            this.name = name;
            this.value = value;
        }
        public override string ToString()
        {
            return name;
        }
        public static implicit operator string(DefineType defineType)
        {
            return defineType.ToString();
        }
    }
    public sealed class Rank
    {
        private readonly string name;
        private readonly int value;
        
        private static readonly Dictionary<string, Rank> instance = new Dictionary<string, Rank>();

        public static readonly Rank Recruit = new Rank(1, "Recruit");
        public static readonly Rank Member = new Rank(2, "Member");
        public static readonly Rank MemberII = new Rank(3, "Member II");
        public static readonly Rank Veteran = new Rank(4, "Veteran");
        public static readonly Rank Officer = new Rank(5, "Officer");
        public static readonly Rank General = new Rank(6, "General");
        public static readonly Rank GuildMaster = new Rank(7, "Guild Master");

        private Rank(int value, string name)
        {
            this.name = name;
            this.value = value;
            instance[name] = this;
        }
        public override string ToString()
        {
            return name;
        }
        public static implicit operator Rank(string str)
        {
            Rank result;
            if (instance.TryGetValue(str, out result))
                return result;
            else
                throw new InvalidCastException();
        }
        public static implicit operator string(Rank rank)
        {
            return rank.ToString();
        }
    }
}
