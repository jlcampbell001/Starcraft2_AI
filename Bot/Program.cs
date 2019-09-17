using SC2APIProtocol;
using System;
using System.Collections.Generic;

namespace Bot
{
    internal class Program
    {
        // Settings for your bot.

        //private static readonly Bot bot = new RaxBot();
        //private const Race race = Race.Terran;

        private static readonly Bot bot = new JCZergBot();
        private const Race race = Race.Zerg;

        private static Random random = new Random();

        // Settings for single player mode.
        //        private static string mapName = "AbyssalReefLE.SC2Map";
        //        private static string mapName = "AbiogenesisLE.SC2Map";
        //        private static string mapName = "FrostLE.SC2Map";
        //        private static readonly string mapName = "(2)16-BitLE.SC2Map";
        private static String[] maps = {"AutomatonLE.SC2Map",
            "CyberForestLE.SC2Map",
            "KairosJunctionLE.SC2Map",
            "KingsCoveLE.SC2Map",
            "NewRepugnancyLE.SC2Map",
            "PortAleksanderLE.SC2Map",
            "YearZeroLE.SC2Map"};

        private static readonly string mapName = maps[random.Next(maps.Length)];

        private static readonly Race opponentRace = Race.Terran;
        //private static readonly Race opponentRace = Race.Protoss;
        //private static readonly Race opponentRace = Race.Zerg;
        //        private static readonly Race opponentRace = Race.Random;
        private static readonly Difficulty opponentDifficulty = Difficulty.VeryEasy;

        private static readonly Boolean realTime = false;

        public static GameConnection gc;

        private static void Main(string[] args)
        {
            try
            {
                gc = new GameConnection();
                if (args.Length == 0)
                {
                    gc.readSettings();
                    gc.RunSinglePlayer(bot, mapName, race, opponentRace, opponentDifficulty, realTime).Wait();
                }
                else
                    gc.RunLadder(bot, race, args).Wait();
            }
            catch (Exception ex)
            {
                Logger.Info(ex.ToString());
            }

            Logger.Info("Terminated.");
        }
    }
}
