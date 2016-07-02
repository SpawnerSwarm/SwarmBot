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
using SwarmBot.XML;
using Trileans;

namespace SwarmBot.Chat
{
    public class Emotes
    {
        public Emote[] emotes { get; set; }
        private XDocument x { get; set; }
        private string path { get; set; }

        public Emotes(XDocument x, string path) 
        {
            this.x = x;
            this.path = path;
            emotes = parseEmotes();
        }
        public Emotes(string path)
        {
            x = XDocument.Load(path);
            this.path = path;
            emotes = parseEmotes();
        }
        private Emote[] parseEmotes()
        {
            XElement[] xEs = x.Descendants("Emote").ToArray();
            Emote[] emotes = new Emote[xEs.Length];
            for(int i = 0; i < xEs.Length; i++)
            {
                emotes[i] = new Emote(xEs[i]);
            }
            Console.WriteLine(emotes[0]);
            return emotes;
        }
        public trilean getEmote(string reference) 
        {
            Emote[] emotes = this.emotes.Where(x => x.reference == reference).ToArray();
            if(emotes.Length > 1)
            {
                return new trilean(false, true, "Multiple");
            } else if(emotes.Length < 1)
            {
                return false;
            }
            return new trilean(true, emotes[0]);
        }
        public trilean getEligibleForEmote(XMLMember member, string reference)
        {
            Emote emote = (Emote)getEmote(reference).embedded;
            if(emote == null)
            {
                return new trilean(false, true, "multiple");
            }
            return getEligibleForEmote(member, emote);
        }
        public static trilean getEligibleForEmote(XMLMember member, Emote emote)
        {
            return new trilean(member.checkPermissions(emote.requiredRank), false);
        }
        public trilean newEmote(Emote emote)
        {
            if(x.Descendants("Emote").Where(x => x.Descendants("Name").ToArray()[0].Value == emote.name).ToArray().Length > 0)
            {
                return new trilean(false, true, "name");
            }
            else if(x.Descendants("Emote").Where(x => x.Descendants("URL").ToArray()[0].Value == emote.URL).ToArray().Length > 0)
            {
                return new trilean(false, true, "URL");
            }
            else if(x.Descendants("Emote").Where(x => x.Descendants("ref").ToArray()[0].Value == emote.reference).ToArray().Length > 0)
            {
                return new trilean(false, true, "ref");
            }
            x.Descendants("Emotes").ToArray()[0].Add(emote.toXElement());
            x.Save(path);
            return true;
        }
    }
    public class Emote
    {
        public string URL { get; internal set; }
        public string name { get; internal set; }
        public string reference { get; internal set; }
        public short requiredRank { get; internal set; }
        private string creator { get; set; }

        public Emote(XElement xE)
        {
            URL = xE.Descendants("URL").ToArray()[0].Value;
            name = xE.Descendants("Name").ToArray()[0].Value;
            reference = xE.Descendants("ref").ToArray()[0].Value;
            requiredRank = short.Parse(xE.Descendants("Rank").ToArray()[0].Value);
            creator = xE.Descendants("Creator").ToArray()[0].Value;
        }

        public Emote(string URL, string name, string reference, short requiredRank, XMLMember creator)
        {
            this.URL = URL;
            this.name = name;
            this.reference = reference;
            this.requiredRank = requiredRank;
            this.creator = creator.name;
        }

        public trilean getEligible(XMLMember member)
        {
            return Emotes.getEligibleForEmote(member, this);
        }

        public XElement toXElement()
        {
            XElement xE = new XElement("Emote",
                new XElement("Name", name),
                new XElement("URL", URL),
                new XElement("ref", reference),
                new XElement("Rank", requiredRank),
                new XElement("Creator", creator));
            return xE;
        }
    }
}
