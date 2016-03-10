//Rename this file to Program.cs and replace the username and password with your own info.

using DiscordSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SwarmBot
{
    class Program
    {
        static void Main(string[] args)
        {
            DiscordClient client = new DiscordClient();
            client.ClientPrivateInformation.email = "EMAIL";
            client.ClientPrivateInformation.password = "PWORD";

            client.Connected += (sender, e) =>
            {
                Console.WriteLine("Connected! User: " + e.user.Username);
                client.UpdateCurrentGame("Type !help for help");
            };

            client.MessageReceived += (sender, e) =>
            {
                if (e.author.Username != "SwarmBot")
                {
                    if (e.message_text == "!help")
                    {
                        e.Channel.SendMessage("I am the SwarmBot created by @Mardan. You can ask me to search the Warframe Wiki (!wfwiki <page name>), or read the Spiral Knights Wiki (!skwiki <page name>). More functions will be added soon; feel free to pm @Mardan with suggestions!");
                        Console.WriteLine("Sent help because of this message: " + e.message_text);
                    }
                    else if (e.message_text.Contains("!wfwiki"))
                    {
                        string target = Regex.Match(e.message_text,
                            @"!wfwiki (.+)",
                            RegexOptions.Singleline)
                        .Groups[1].Value;
                        e.Channel.SendMessage("@" + e.author.Username + " http://warframe.wikia.com/wiki/" + target.Replace(" ", "_"));
                        Console.WriteLine("Sent " + target.Replace(" ", "_") + " to " + e.author.Username + " because of this message: " + e.message_text);
                    };
                }
                
            };
            try
            {
                // Make sure that IF something goes wrong, the user will be notified.
                // The SendLoginRequest should be called after the events are defined, to prevent issues.
                client.SendLoginRequest();
                client.Connect(); // Login request, and then connect using the discordclient i just made.
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong!\n" + e.Message + "\nPress any key to close this window.");
            }

            Console.ReadKey();
        }
    }
}
