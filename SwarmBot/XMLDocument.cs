using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Trileans;

namespace SwarmBot.XML
{
    class XMLDocument
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
            XElement[] memberArr = document.Descendants("Member").Where(x => x.Element("Names").Element("DiscordId").Value == id).ToArray();
            
            if(memberArr.Length == 1) { return new XMLMember(memberArr[0], this); }
            else if(memberArr.Length < 1) { throw new XMLException(XMLErrorCode.NotFound); }
            else if(memberArr.Length > 1) { throw new XMLException(XMLErrorCode.MultipleFound); }
            else { throw new Exception(); }
        }
        public XMLMember getMemberById(ulong id)
        {
            return getMemberById(id.ToString());
        }

        public int getDefine(string value, DefineType _for)
        {
            List<XElement> defines = document.Element("Database").Elements("Define").Where(x => x.Attribute("name").Value == value).Where(x => x.Attribute("for").Value == _for).ToList();
            if(defines.Count == 1) { return Int32.Parse(defines[0].Value); }
            else { throw new XMLException((defines.Count > 1 ? XMLErrorCode.MultipleFound : XMLErrorCode.NotFound)); }
        }
        public string getDefineName(string value, DefineType _for)
        {
            List<XElement> defines = document.Element("Database").Elements("Define").Where(x => x.Value == value.ToString()).Where(x => x.Attribute("for").Value == _for).ToList();
            if(defines.Count == 1) { return defines[0].Attribute("name").Value; }
            else { throw new XMLException((defines.Count > 1 ? XMLErrorCode.MultipleFound : XMLErrorCode.NotFound)); }
        }
        public trilean checkRankMaxed(Rank rank)
        {
            try
            {
                short memberCount = (short)document.Elements("Member").Where(x => x.Element("Rank").Value == rank).Count();
                return new trilean(memberCount >= (short)getDefine(rank, DefineType.RankCapacity), memberCount);
            }
            catch { throw new XMLException(XMLErrorCode.Unknown, "An error occured while checking availible member capacity."); }
        }
        public async Task<trilean> createMember(string name, DateTime date, long steamId, ulong discordId)
        {
            try
            {
                XElement xE = new XElement("Member",
                    new XElement("Name", name),
                    new XElement("Rank", Rank.Recruit.ToString()),
                    new XElement("RankupHistory",
                        new XElement("Rankup", new XAttribute("name", "Recruit"), date),
                        new XElement("Rankup", new XAttribute("name", "Member"), "NaN"),
                        new XElement("Rankup", new XAttribute("name", "Member II"), "NaN"),
                        new XElement("Rankup", new XAttribute("name", "Veteran"), "NaN"),
                        new XElement("Rankup", new XAttribute("name", "Officer"), "NaN"),
                        new XElement("Rankup", new XAttribute("name", "General"), "NaN"),
                        new XElement("Rankup", new XAttribute("name", "Guild Master"), "NaN")
                    ),
                    new XElement("Names",
                        new XElement("Warframe", ""),
                        new XElement("SpiralKnights", ""),
                        new XElement("Discord", name),
                        new XElement("DiscordId", discordId),
                        new XElement("Steam"),
                        new XElement("SteamId", steamId)
                    ),
                    new XElement("FailedTrial", false),
                    new XElement("FormaDonated", 0)
                );
                document.Element("Database").Add(xE);
            }
            catch (Exception x)
            {
                await Program.Log("An error occured in XMLDocument.cs, unable to create member. \n\n" + x.Message);
                return new trilean(false, x);
            }
            return new trilean(true);
        }
    }
}
