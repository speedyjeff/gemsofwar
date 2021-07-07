using gemsofwar.engine;
using gemsofwar.engine.ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.console
{
    class GamePlay
    {
        public static void Play(GameMode gamemode)
        {
            IGemFactory gemfactory = (gamemode == GameMode.Battle) ? new BattleFactory(seed: 0) : new TreasureFactory(seed: 0);
            var board = new GemsBoard(gemfactory, (gamemode == GameMode.Battle) ? Rules.RemoveGemOnMatch : Rules.UpgradeGemOnMatch);
            var tally = new int[(int)GemId.G9 + 1];

            var done = false;
            while (!done)
            {
                if (board.AvailableMoves.Count == 0) board.Shuffle();

                // get predictions and strategy
                var strategies = Strategy.Analyze(
                    board, 
                    depth: 2,
                    gemIdOrder: new GemId[] { GemId.G7 });

                // display the board and moves
                ConsoleDisplayBoard(board, tally, strategies, gamemode);

                while (true)
                {
                    Console.WriteLine("Select a move by the number (q for quit):");
                    var input = Console.ReadLine();

                    if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
                    {
                        done = true;
                        break;
                    }
                    else if (Int32.TryParse(input, out int index))
                    {
                        if (index >= 0 && index < strategies.Count)
                        {
                            // make the selection and apply it
                            var move = strategies[index].Move;
                            if (!board.TryMakeMove(move, out List<Match> matches)) throw new Exception($"Failed to apply move {move.Row},{move.Col} {move.Direction}");

                            // sum the matches to the tally
                            foreach (var m in matches)
                            {
                                Console.WriteLine($"   [{m.Row},{m.Col} {m.Gem.Id} {m.Size}]");
                                tally[(int)m.Gem.Id] += m.Size;
                            }
                            break;
                        }
                    }
                }
            }
        }

        #region private
        private static void ConsoleDisplayBoard(GemsBoard board, int[] tally, List<Strategy> strategies, GameMode gamemode)
        {
            var gems = board.Gems;

            // dispaly the board
            for (int row = 0; row < gems.Length; row++)
            {
                if (row == 0)
                {
                    Console.Write("  ");
                    for (int col = 0; col < gems[row].Length; col++) Console.Write($"{col} ");
                    Console.WriteLine();
                }

                Console.Write($"{row} ");
                for (int col = 0; col < gems[row].Length; col++)
                {
                    DisplayGem(gems[row][col].Id, gamemode);
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            // display the current tally
            for (int i = (int)GemId.G1; i < tally.Length && i <= ((gamemode == GameMode.Battle) ? (int)GemId.G7 : (int)GemId.G8); i++)
            {
                DisplayGem((GemId)i, gamemode);
                Console.Write($":{tally[i]} ");
            }
            Console.WriteLine();

            // display the moves
            Console.WriteLine("");
            for (int i = 0; i < strategies.Count; i++)
            {
                Console.WriteLine($" {i} - {strategies[i].Move.Row},{strategies[i].Move.Col} {strategies[i].Move.Direction} [largest:{strategies[i].LargestGemCount} creates:{strategies[i].CreatesLargestGemCount}]");
            }
        }

        private static void DisplayGem(GemId id, GameMode gamemode)
        {
            var color = Console.ForegroundColor;

            try
            {
                switch (id)
                {
                    case GemId.Empty:
                        Console.ForegroundColor = Console.BackgroundColor;
                        break;
                    case GemId.G1:
                        Console.ForegroundColor = (gamemode == GameMode.Battle) ? ConsoleColor.Red : ConsoleColor.DarkGray;
                        break;
                    case GemId.G2:
                        Console.ForegroundColor = (gamemode == GameMode.Battle) ? ConsoleColor.DarkGray : ConsoleColor.White;
                        break;
                    case GemId.G3:
                        Console.ForegroundColor = (gamemode == GameMode.Battle) ? ConsoleColor.Green : ConsoleColor.Yellow;
                        break;
                    case GemId.G4:
                        Console.ForegroundColor = (gamemode == GameMode.Battle) ? ConsoleColor.Blue : ConsoleColor.DarkBlue;
                        break;
                    case GemId.G5:
                        Console.ForegroundColor = (gamemode == GameMode.Battle) ? ConsoleColor.Magenta : ConsoleColor.DarkYellow;
                        break;
                    case GemId.G6:
                        Console.ForegroundColor = (gamemode == GameMode.Battle) ? ConsoleColor.Yellow : ConsoleColor.Green;
                        break;
                    case GemId.G7:
                        Console.ForegroundColor = (gamemode == GameMode.Battle) ? ConsoleColor.White : ConsoleColor.Red;
                        break;
                    case GemId.G8:
                        // todo doom skull
                        Console.ForegroundColor = (gamemode == GameMode.Battle) ? ConsoleColor.White : ConsoleColor.Magenta;
                        break;
                    case GemId.G9:
                        Console.ForegroundColor = (gamemode == GameMode.Battle) ? ConsoleColor.Gray : ConsoleColor.Gray;
                        break;
                }
                Console.Write("@ ");
            }
            finally
            {
                Console.ForegroundColor = color;
            }
        }
        #endregion
    }
}
