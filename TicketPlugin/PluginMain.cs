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
            List<string> permlist = new List<string>();
            TShock.Groups.AddPermissions("trustedadmin", permlist);

            Commands.ChatCommands.Add(new Command(Hlpme, "hlpme", "Hlpme"));
            Commands.ChatCommands.Add(new Command("TicketList", TicketList, "TicketList", "ticketlist", "Ticketlist", "ticketList"));
            Commands.ChatCommands.Add(new Command("TicketClear", TicketClear, "TicketClear", "Ticketclear", "ticketclear", "ticketClear", "TicketsClear", "Ticketsclear", "ticketsclear", "ticketsClear"));
        }

        public void OnUpdate()
        {
        }

        public void OnGreetPlayer(int who, HandledEventArgs e)
        {
            TShock.Players[who].SendMessage("To write a Ticket, use /hlpme <Message>", Color.DarkCyan);
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

        public static void TicketList(CommandArgs args)
        {
            try
            {
                StreamReader sw = new StreamReader("Tickets.txt", true);
                while (sw.Peek() >= 0)
                {
                    args.Player.SendMessage(sw.ReadLine());
                }
                sw.Close();
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
        }
    }
}