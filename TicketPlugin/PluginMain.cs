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
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
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
            TShock.Players[who].SendMessage("To write a Complaint, use /hlpme ''<Message>''", Color.DarkCyan);
        }

        public void OnLeave(int ply)
        {
        }

        public void OnChat(messageBuffer msg, int ply, string text, HandledEventArgs e)
        {
        }

        public static void Hlpme(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("You must enter a message!", Color.Red);
            }
            else if (args.Parameters.Count > 1)
            {
                args.Player.SendMessage("You must put quotes around a multiple worded ticket. Like /hlpme ''word1 word2''", Color.Red);
            }
            else
            {
                string username = args.Player.Name;
                string ticket = args.Parameters[0];
                args.Player.SendMessage("Your Ticket has been sent!", Color.DarkCyan);
                StreamWriter tw = new StreamWriter("Tickets.txt", true);
                tw.WriteLine(string.Format("{0} - {1}: {2}", DateTime.Now, username, ticket));
                tw.Close();
            }
        }

        public static int linenumber = 1;
        public static void TicketList(CommandArgs args)
        {
            try
            {
                StreamReader sw = new StreamReader("Tickets.txt", true);
                while (sw.Peek() >= 0)
                {
                    args.Player.SendMessage(linenumber+"." +sw.ReadLine());
                    linenumber++;
                }
                sw.Close();
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

        public static int i = 0;
        public static void TicketClear(CommandArgs args)
        {
            switch (args.Parameters[0])
            {
                case "all": 
                    try
                    {
                        File.Delete("Tickets.txt");
                        args.Player.SendMessage("All of the Tickets were cleared!", Color.DarkCyan);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("{0} has cleared all of the tickets.", args.Player.Name);
                        Console.ResetColor();
                    }
                    catch (Exception e)
                    {
                        // Let the console know what went wrong, and tell the player that there was an error.
                        args.Player.SendMessage("Something went wrong when you tried to clear the tickets, contact an administrator when you can.", Color.Red);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.Message);
                        Console.ResetColor();
                    }
                    break;
                case "id":
                    if (args.Parameters.Count > 0)
                    {
                        int lineToDelete = Convert.ToInt32(args.Parameters[1]);
                        var file = new List<string>(System.IO.File.ReadAllLines("Tickets.txt"));
                        file.RemoveAt(lineToDelete);
                        File.WriteAllLines("Tickets.txt", file.ToArray());
                        args.Player.SendMessage(string.Format("Ticket ID {0} was cleared!", args.Parameters[1]), Color.DarkCyan);
                    }
                    else
                    {
                        args.Player.SendMessage("You have to state a ticket id! Syntax: /ticclear id <ticid>", Color.Red);
                    }
                    break;
            }
        }
    }
}