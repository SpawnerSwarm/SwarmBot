using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using Trileans;

namespace SwarmBot.Chat
{
    class Events
    {
        private Event[] events { get; }
        private XDocument x;
        private string path;

        public Events(XDocument x, string path)
        {
            this.x = x;
            this.path = path;
            events = parseEvents();
        }
        public Events(string path)
        {
            x = XDocument.Load(path);
            this.path = path;
            events = parseEvents();
        }
        private Event[] parseEvents()
        {
            XElement[] xEs = x.Descendants("Event").ToArray();
            Event[] events = new Event[xEs.Length];
            for (int i = 0; i < xEs.Length; i++)
            {
                events[i] = new Event(xEs[i]);
            }
            return events;
        }
        public trilean getEvent(string reference)
        {
            Event[] events = this.events.Where(x => x.reference == reference).ToArray();
            if(events.Length > 1)
            {
                return new trilean(false, true, "Multiple");
            } else if (events.Length < 1)
            {
                return false;
            }
            return new trilean(true, events[0]);
        }
        public Event getLatestEvent()
        {
            return events.Last();
        }
        public string list(int page, XML.XMLDocument db)
        {
            if (page == 0) { page = 1; };
            string block = "Page " + page + ". To move to the next page, use \"!event list " + (page + 1) + "\". To view information about a specific event, use \"!event <event_ref>\".\n";
            for(int i = page * 5 - 1; i >= page * 5 - 5; i --)
            {
                if (i >= 0 && events.Length - 1 >= i)
                {
                    block += "\n" + events[i].icon + " " + events[i].name + " " + events[i].icon;
                    if (i == events.Length - 1)
                    {
                        block = block.Replace(events[i].name, "**" + events[i].name + "**");
                    }
                    block += " ‒‒ " + events[i].reference;
                    block += " ‒‒ ";
                    foreach(Reward reward in events[i].rewards)
                    {
                        block += reward.icon;
                    }
                }
            }
            if (block == "")
            {
                block = "http://i.imgur.com/zdMAeE9.png";
            }
            return block;
        }
    }
    class Event
    {
        public string icon { get; }
        public string name { get; }
        public string lotusText { get; }
        public string reference { get; }
        public Task[] tasks { get; }
        public Reward[] rewards { get; }

        public Event(XElement xE)
        {
            icon = xE.Descendants("Icon").ToArray()[0].Value;
            name = xE.Descendants("Name").ToArray()[0].Value;
            lotusText = xE.Descendants("LotusText").ToArray()[0].Value;
            reference = xE.Descendants("Reference").ToArray()[0].Value;
            XElement[] tasks = xE.Descendants("Task").ToArray();
            this.tasks = new Task[tasks.Length];
            for(int i = 0; i < tasks.Length; i++)
            {
                this.tasks[i] = new Task(tasks[i]);
            }
            XElement[] rewards = xE.Descendants("Reward").ToArray();
            this.rewards = new Reward[rewards.Length];
            for (int i = 0; i < rewards.Length; i++)
            {
                this.rewards[i] = new Reward(rewards[i]);
            }
        }
    }
    class Task
    {
        public string icon { get; }
        public string description { get; }
        public short numericCount { get; }
        public string target { get; }
        public string location { get; }

        public Task(XElement xE)
        {
            icon = xE.Descendants("Icon").ToArray()[0].Value;
            description = xE.Descendants("Description").ToArray()[0].Value;
            numericCount = short.Parse(xE.Descendants("NumericCount").ToArray()[0].Value);
            target = xE.Descendants("Target").ToArray()[0].Value;
            location = xE.Descendants("Location").ToArray()[0].Value;
        }
        public string getTask()
        {
            string block = "";
            block += icon;
            block += description + "\n";
            return block;
        }
    }
    class Reward
    {
        public string icon { get; }
        public string name { get; }
        public short numericCount { get; }

        public Reward(XElement xE)
        {
            icon = xE.Descendants("Icon").ToArray()[0].Value;
            name = xE.Descendants("Name").ToArray()[0].Value;
            numericCount = short.Parse(xE.Descendants("NumericCount").ToArray()[0].Value);
        }
        public string getReward()
        {
            string block = "";
            block += icon;
            block += name + "\n";
            return block;
        }
    }
}
