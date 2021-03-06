﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trileans;
using SwarmBot.XML;

namespace SwarmBot
{
    class Emotes
    {
        private XDocument x { get; set; }
        private string path { get; set; }

        public Emotes(XDocument x, string path)
        {
            this.x = x;
            this.path = path;
        }
        public Emotes(string path)
        {
            x = XDocument.Load(path);
            this.path = path;
        }
        public Emote getEmote(string reference)
        {
            List<XElement> xEList = x.Element("Emotes").Elements("Emote").Where(x => x.Element("Reference").Value == reference).ToList();
            if(xEList.Count == 1) { return new Emote(xEList.First()); }
            else if(xEList.Count > 1) { throw new XMLException(XMLErrorCode.MultipleFound, "Found multiple Emotes"); }
            else { throw new XMLException(XMLErrorCode.NotFound, "Could not find Emote"); }
        }
        public static bool getEligibleForEmote(XMLMember member, Emote emote) { return member.checkPermissions(emote.requiredRank); }
        public List<Emote> getBatchEmotes(short page, short count)
        {
            short index = (short)(page == 0 ? page : (page - 1) * 5);
            List<XElement> xEList;
            try { xEList = x.Element("Emotes").Elements("Emote").ToList();
                xEList = xEList.GetRange(index, (xEList.Count - index - count >= 0 ? count : xEList.Count - index)); } catch { throw new XMLException(XMLErrorCode.NotFound); }
            if(xEList.Count == 0) { throw new XMLException(XMLErrorCode.NotFound); }
            return xEList.ConvertAll(new Converter<XElement, Emote>(Emote.XElementToEmote));
        }
        public void addEmote(Emote emote)
        {
            XElement xE = x.Element("Emotes");
            if(xE.Elements("Emote").Where(x => x.Element("Name").Value == emote.name).Count() > 0) { throw new XMLException(XMLErrorCode.MultipleFound, "name"); }
            if(xE.Elements("Emote").Where(x => x.Element("Reference").Value == emote.reference).Count() > 0) { throw new XMLException(XMLErrorCode.MultipleFound, "reference"); }
            if(xE.Elements("Emote").Where(x => x.Element("Content").Value == emote.content).Count() > 0) { throw new XMLException(XMLErrorCode.MultipleFound, "content"); }

            xE.Add(emote.EmoteToXElement());
            x.Save(path);
        }
    }

    class Emote
    {
        public string name { get; internal set; }
        public string content { get; internal set; }
        public string reference { get; internal set; }
        public Rank requiredRank { get; internal set; }
        public string creator { get; internal set; }

        public Emote(XElement xE)
        {
            name = xE.Element("Name").Value;
            content = xE.Element("Content").Value;
            reference = xE.Element("Reference").Value;
            requiredRank = Int16.Parse(xE.Element("Rank").Value);
            creator = xE.Element("Creator").Value;
        }

        public Emote(string name, string reference, string content, Rank requiredRank, string creator = "Empty")
        {
            this.name = name;
            this.content = content;
            this.reference = reference;
            this.requiredRank = requiredRank;
            this.creator = creator;
        }

        public bool getEligible(XMLMember member)
        {
            return member.checkPermissions(requiredRank);
        }

        public static Emote XElementToEmote(XElement xE)
        {
            return new Emote(xE);
        }

        public XElement EmoteToXElement()
        {
            XElement xE = new XElement("Emote",
                new XElement("Name", name),
                new XElement("Content", content),
                new XElement("Reference", reference),
                new XElement("Rank", (short)requiredRank),
                new XElement("Creator", creator));
            return xE;
        }
    }
}
