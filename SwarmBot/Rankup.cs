using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwarmBot
{
    class Rankup
    {
        public Rank rank;
        public DateTime date;
        public string placeHolderDate = null;

        public Rankup(Rank rank, string date = null, bool old = true, bool NaN = false)
        {
            this.rank = rank;
            if(old || NaN)
            {
                placeHolderDate = old ? "Old" : "NaN";
            }
            else
            {
                try
                {
                    this.date = DateTime.Parse(date);
                }
                catch
                {
                    this.date = new DateTime();
                    placeHolderDate = "Old";
                }
            }
        }
        internal Rankup(XElement rankup)
        {
            rank = rankup.Attribute("name").Value;
            string date = rankup.Value;
            if (date == "Old" || date == "NaN")
            {
                this.date = new DateTime();
                this.placeHolderDate = date;
            }
            else { this.date = DateTime.Parse(date); }
        }
        public string getDate()
        {
            return (placeHolderDate != null) ? placeHolderDate : date.ToString();
        }
    }
}
