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

            switch(memberArr.Length)
            {
                case 1: return new XMLMember(memberArr[0], this);
                case 2: return null;//insert exception here
                case 3: return null;//insert exception here
                default: throw new Exception();
            }
        }
        public XMLMember getMemberById(ulong id)
        {
            return getMemberById(id.ToString());
        }

        public int getDefine(string value, DefineType _for)
        {
            List<XElement> defines = document.Elements("Define").Where(x => x.Attribute("name").Value == value).Where(x => x.Attribute("for").Value == _for).ToList();
            if(defines.Count == 1) { return Int32.Parse(defines[0].Value); }
            else { throw new XMLException((defines.Count > 1 ? XMLErrorCode.MultipleFound : XMLErrorCode.NotFound)); }
        }
        public string getDefineName(string value, DefineType _for)
        {
            List<XElement> defines = document.Elements("Define").Where(x => x.Value == value.ToString()).Where(x => x.Attribute("for").Value == _for).ToList();
            if(defines.Count == 1) { return defines[0].Attribute("name").Value; }
            else { throw new XMLException((defines.Count > 1 ? XMLErrorCode.MultipleFound : XMLErrorCode.NotFound)); }
        }
    }
}
