//Rename this file to Program.cs and replace the username and password with your own info.

using DiscordSharp;
using DiscordSharp.Objects;
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
            client.ClientPrivateInformation.password = "PASSWORD";

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
                        e.Channel.SendMessage("I am the SwarmBot created by @Mardan. View my source code: https://github.com/SpawnerSwarm/SwarmBot. I can:");
                        e.Channel.SendMessage("--   Search the Warframe Wiki (!wfwiki <page name>)");
                        e.Channel.SendMessage("--   Search the Spiral Knights Wiki (!skwiki <page name>)");
                        e.Channel.SendMessage("--   Link you to the Guild Mail (!guildmail)");
                        e.Channel.SendMessage("--   BETA Invite a group of players to play a game (!invite <Number of invitees> <Discord username 1>, [Discord username 2], [Discord username 3], [Discord username 4] <Game name>");
                        e.Channel.SendMessage("More functions will be added soon; feel free to pm @Mardan with suggestions!");
                        Console.WriteLine("Sent help because of this message: " + e.message_text);
                    }
                    else if (e.message_text.StartsWith("!wfwiki"))
                    {
                        string target = Regex.Match(e.message_text,
                            @"!wfwiki (.+)",
                            RegexOptions.Singleline)
                        .Groups[1].Value;
                        e.Channel.SendMessage("@" + e.author.Username + " http://warframe.wikia.com/wiki/" + target.Replace(" ", "_"));
                        Console.WriteLine("Sent " + target.Replace(" ", "_") + " to " + e.author.Username + " because of this message: " + e.message_text);
                    }
                    else if (e.message_text.StartsWith("!skwiki"))
                    {
                        string target = Regex.Match(e.message_text,
                            @"!skwiki (.+)",
                            RegexOptions.Singleline)
                            .Groups[1].Value;
                        e.Channel.SendMessage("@" + e.author.Username + " http://wiki.spiralknights.com/" + target.Replace(" ", "_"));
                        Console.WriteLine("Sent " + target.Replace(" ", "_") + " to " + e.author.Username + " because of this message: " + e.message_text);
                        
                    }
                    else if (e.message_text.StartsWith("!guildmail"))
                    {
                        string guildmail = "https://onedrive.live.com/redir?resid=F485764E97178E7C!5421&authkey=!AC4Oi92whZUO3xo&ithint=file%2cpdf";
                        e.Channel.SendMessage(guildmail);
                    }
                    else if (e.message_text.StartsWith("!invite"))
                    {
                        string numInvitees = Regex.Match(e.message_text, @"([1-8])", RegexOptions.Singleline).Groups[1].Value;
                        if (numInvitees != "")
                        {
                            if(numInvitees == "1")
                            {
                                var matches = Regex.Match(e.message_text, @"!invite (?:[1-8]) (?:@)?(.+) (.+)");
                                string firstMatch = matches.Groups[1].Value;
                                string secondMatch = matches.Groups[2].Value;
                                /*Console.WriteLine(firstMatch);
                                Console.WriteLine(secondMatch);*/
                                // In the future, this will also dispatch a steam message.
                                try {
                                    e.Channel.parent.members.Find(x => x.Username == firstMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + secondMatch);
                                }
                                catch (Exception f)
                                {
                                    e.Channel.SendMessage("Sorry, something went wrong. Perhaps your syntax is off? Make sure you're putting commas inbetween the names!");
                                }
                            }
                            else if(numInvitees == "2")
                            {
                                var matches = Regex.Match(e.message_text, @"!invite (?:[1-8]) (?:@)?(.+), (?:@)?(.+) (.+)");
                                string firstMatch = matches.Groups[1].Value;
                                string secondMatch = matches.Groups[2].Value;
                                string thirdMatch = matches.Groups[3].Value;
                                try {
                                    e.Channel.parent.members.Find(x => x.Username == firstMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + thirdMatch);
                                    e.Channel.parent.members.Find(x => x.Username == secondMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + thirdMatch);
                                }
                                catch (Exception f)
                                {
                                    e.Channel.SendMessage("Sorry, something went wrong. Perhaps your syntax is off? Make sure you're putting commas inbetween the names!");
                                }
                            }
                            else if(numInvitees == "3")
                            {
                                var matches = Regex.Match(e.message_text, @"!invite (?:[1-8]) (?:@)?(.+), (?:@)?(.+), (?:@)?(.+) (.+)");
                                string firstMatch = matches.Groups[1].Value;
                                string secondMatch = matches.Groups[2].Value;
                                string thirdMatch = matches.Groups[3].Value;
                                string fourthMatch = matches.Groups[4].Value;
                                try {
                                    e.Channel.parent.members.Find(x => x.Username == firstMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fourthMatch);
                                    e.Channel.parent.members.Find(x => x.Username == secondMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fourthMatch);
                                    e.Channel.parent.members.Find(x => x.Username == thirdMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fourthMatch);
                                }
                                catch (Exception f)
                                {
                                    e.Channel.SendMessage("Sorry, something went wrong. Perhaps your syntax is off? Make sure you're putting commas inbetween the names!");
                                }
                            }
                            else if(numInvitees == "4")
                            {
                                var matches = Regex.Match(e.message_text, @"!invite (?:[1-8]) (?:@)?(.+), (?:@)?(.+), (?:@)?(.+), (?:@)?(.+) (.+)");
                                string firstMatch = matches.Groups[1].Value;
                                string secondMatch = matches.Groups[2].Value;
                                string thirdMatch = matches.Groups[3].Value;
                                string fourthMatch = matches.Groups[4].Value;
                                string fifthMatch = matches.Groups[5].Value;

                                try
                                {
                                    e.Channel.parent.members.Find(x => x.Username == firstMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fifthMatch);
                                    e.Channel.parent.members.Find(x => x.Username == secondMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fifthMatch);
                                    e.Channel.parent.members.Find(x => x.Username == thirdMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fifthMatch);
                                    e.Channel.parent.members.Find(x => x.Username == fourthMatch).SendMessage("@" + e.message.author.Username + " has invited you to play: " + fifthMatch);
                                }
                                catch (Exception f)
                                {
                                    e.Channel.SendMessage("Sorry, something went wrong. Perhaps your syntax is off? Make sure you're putting commas inbetween the names!");
                                }

                            }
                            else
                            {
                                e.Channel.SendMessage("Sorry, only 1-4 invitees are supported with one command!");
                            };
                        } else
                        {
                            e.Channel.SendMessage("Error: Number of invitees not specified.");
                        };
                    }
                }

            };
            client.PrivateMessageReceived += (sender, e) =>
            {
                Console.WriteLine(e.author);
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
