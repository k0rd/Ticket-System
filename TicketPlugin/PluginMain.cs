using System;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;
using Terraria;
using Hooks;
using TShockAPI;
using TShockAPI.DB;
using System.ComponentModel;
using System.IO;

namespace TicketPlugin
{
    [APIVersion(1, 11)]
    public class TicketPlugin : TerrariaPlugin
    {
        public static List<Player> Players = new List<Player>();
        public override string Name
        {
            get { return "TicketSystem"; }
        }

        public override string Author
        {
            get { return "Spectrewiz"; }
        }

        public override string Description
        {
            get { return "This plugin allows users in game to file tickets that admins and moderators can access."; }
        }

        public override Version Version
        {
            get { return new Version("0.9.7"); }
        }

        public override void Initialize()
        {
            GameHooks.Update += OnUpdate;
            GameHooks.Initialize += OnInitialize;
            NetHooks.GreetPlayer += OnGreetPlayer;
            ServerHooks.Leave += OnLeave;
            ServerHooks.Chat += OnChat;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GameHooks.Update -= OnUpdate;
                GameHooks.Initialize -= OnInitialize;
                NetHooks.GreetPlayer -= OnGreetPlayer;
                ServerHooks.Leave -= OnLeave;
                ServerHooks.Chat -= OnChat;
            }
            base.Dispose(disposing);
        }

        public TicketPlugin(Main game)
            : base(game)
        {
            Order = 10;
        }

        public void OnInitialize()
        {
            bool tic = false;

            foreach (Group group in TShock.Groups.groups)
            {
                if (group.Name != "superadmin")
                {
                    if (group.HasPermission("TicketList"))
                        tic = true;
                }
            }

            List<string> permlist = new List<string>();
            if (!tic)
            {
                permlist.Add("TicketList");
                permlist.Add("TicketClear");
            }

            TShock.Groups.AddPermissions("trustedadmin", permlist);

            Commands.ChatCommands.Add(new Command(Hlpme, "hlpme", "ticket"));
            Commands.ChatCommands.Add(new Command("TicketList", TicketList, "ticketlist", "ticlist"));
            Commands.ChatCommands.Add(new Command("TicketClear", TicketClear, "ticketclear", "ticketsclear", "ticclear", "ticsclear"));
        }

        public void OnUpdate()
        {
        }

        public void OnGreetPlayer(int who, HandledEventArgs e)
        {
            lock (Players)
                Players.Add(new Player(who));
            string name = TShock.Players[who].Name;
            int count = NumberOfTickets(name);
            if (!TShock.Players[who].Group.HasPermission("TicketList"))
            {
                TShock.Players[who].SendMessage("To write a Complaint, use /hlpme ''<Message>''", Color.DarkCyan);
            }
            else if (TShock.Players[who].Group.HasPermission("TicketList"))
            {
                TShock.Players[who].SendMessage("There are " + count + " tickets submitted, use /ticketlist to view them.", Color.Cyan);
            }
            else if (TShock.Players[who].Group.Name == "superadmin")
            {
                TShock.Players[who].SendMessage("There are " + count + " tickets submitted, use /ticketlist to view them.", Color.Cyan);
            }
        }

        public void OnLeave(int ply)
        {
            lock (Players)
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    if (Players[i].Index == ply)
                    {
                        Players.RemoveAt(i);
                        break; //Found the player, break.
                    }
                }
            }
        }

        public void OnChat(messageBuffer msg, int ply, string text, HandledEventArgs e)
        {
        }

        public static int NumberOfTickets(string name)
        {
            if (name != null && File.Exists("Tickets.txt"))
            {
                int count = 0;
                StreamReader sr = new StreamReader("Tickets.txt", true);
                while (sr.Peek() >= 0)
                {
                    sr.ReadLine();
                    count++;
                }
                sr.Close();
                return count;
            }
            return 0;
        }

        public static void Hlpme(CommandArgs args)
        {
            if ((args.Parameters.Count == 1) && (args.Parameters[0].ToLower() == "help"))
            {
                args.Player.SendMessage("To file a complaint about a bug or just a general issue that you have, do /hlpme <message>", Color.Cyan);
            }
            else if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("You must enter a message!", Color.Red);
            }
            else if ((args.Parameters.Count >= 1) || (args.Parameters.Count == 1 && args.Parameters[0].ToLower() != "help"))
            {
                try
                {
                    string text = "";
                    foreach (string word in args.Parameters)
                    {
                        text = text + word + " ";
                    }
                    string username = args.Player.Name;
                    args.Player.SendMessage("Your Ticket has been sent!", Color.DarkCyan);
                    StreamWriter tw = new StreamWriter("Tickets.txt", true);
                    tw.WriteLine(string.Format("{0} - {1}: {2}", DateTime.Now, username, text));
                    tw.Close();
                    foreach (Player player in TicketPlugin.Players)
                    {
                        if (player.TSPlayer.Group.HasPermission(""))
                        {
                            player.TSPlayer.SendMessage(string.Format("{0} just submitted a ticket: {1}", args.Player.Name, text), Color.Cyan);
                        }
                    }
                }
                catch (Exception e)
                {
                    args.Player.SendMessage("Your ticket could not be sent, contact an administrator.", Color.Red);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                }
            }
        }

        public static int linenumber = 1;
        public static void TicketList(CommandArgs args)
        {
            try
            {
                StreamReader sr = new StreamReader("Tickets.txt", true);
                while (sr.Peek() >= 0)
                {
                    args.Player.SendMessage(linenumber+". " +sr.ReadLine(), Color.Cyan);
                    linenumber++;
                }
                sr.Close();
                linenumber = 1;
            }
            catch (Exception e)
            {
                // Let the console know what went wrong, and tell the player that the file could not be read.
                args.Player.SendMessage("The file could not be read, or it doesnt exist.", Color.Red);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
            }
        }

        public static void TicketClear(CommandArgs args)
        {
            switch (args.Parameters[0].ToLower())
            {
                case "all":
                    try
                    {
                        File.Delete("Tickets.txt");
                        args.Player.SendMessage("All of the Tickets were cleared!", Color.DarkCyan);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(string.Format("{0} has cleared all of the tickets.", args.Player.Name));
                        Console.ResetColor();
                    }
                    catch (Exception e)
                    {
                        // Let the console know what went wrong, and tell the player that there was an error.
                        args.Player.SendMessage("All the tickets are already cleared!", Color.Red);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.Message);
                        Console.ResetColor();
                    }
                    break;
                case "id":
                    if (args.Parameters.Count > 0)
                    {
                        try
                        {
                            int lineToDelete = (Convert.ToInt32(args.Parameters[1]) - 1);
                            var file = new List<string>(System.IO.File.ReadAllLines("Tickets.txt"));
                            file.RemoveAt(lineToDelete);
                            File.WriteAllLines("Tickets.txt", file.ToArray());
                            args.Player.SendMessage(string.Format("Ticket ID {0} was cleared!", args.Parameters[1]), Color.DarkCyan);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(string.Format("{0} has cleared ticket ID: {1}", args.Player.Name, args.Parameters[1]));
                            Console.ResetColor();
                        }
                        catch (Exception e)
                        {
                            args.Player.SendMessage("Not a valid ID.", Color.Red);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(e.Message);
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        args.Player.SendMessage("You have to state a ticket id! Syntax: /ticclear id <ticid>", Color.Red);
                    }
                    break;
                default:
                    args.Player.SendMessage("Syntax: /ticclear <all/id> <id>", Color.Red);
                    break;
            }
        }
    }
}