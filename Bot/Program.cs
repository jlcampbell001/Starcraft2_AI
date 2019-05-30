using SC2APIProtocol;
using System;

namespace Bot
{
    internal class Program
    {
        // Settings for your bot.

        //private static readonly Bot bot = new RaxBot();
        //private const Race race = Race.Terran;

        private static readonly Bot bot = new JCZergBot();
        private const Race race = Race.Zerg;

        // Settings for single player mode.
        //        private static string mapName = "AbyssalReefLE.SC2Map";
        //        private static string mapName = "AbiogenesisLE.SC2Map";
        //        private static string mapName = "FrostLE.SC2Map";
        //        private static readonly string mapName = "(2)16-BitLE.SC2Map";
        private static readonly string mapName = "AutomatonLE.SC2Map";


        private static readonly Race opponentRace = Race.Terran;
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