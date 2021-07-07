using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.tests.engine
{
    static class GameBoardTest
    {
        public static void Run(GameMode gamemode)
        {
            IGemFactory factory = (gamemode == GameMode.Battle) ? new BattleFactory(seed: 1234) : new TreasureFactory(seed: 1234);
            var board = new GemsBoard(factory, (gamemode == GameMode.Battle) ? Rules.RemoveGemOnMatch : Rules.UpgradeGemOnMatch);

            Console.WriteLine("**********************************");

            // make moves
            for (int i = 0; i < 10; i++)
            {
                if (board.AvailableMoves.Count == 0) board.Shuffle();
                var move = board.AvailableMoves[0];
                Console.WriteLine("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
                Console.WriteLine($"{move.Direction} {move.Row},{move.Col}");
                var before = board.ToString();
                board.TryMakeMove(move, out List<Match> matches);
                var after = board.ToString();
                if (matches == null || matches.Count == 0) throw new Exception("failed to return a match");
                Console.WriteLine(before);
                Console.WriteLine(after);
                foreach (var m in matches)
                {
                    Console.WriteLine($" {m.Gem.Id} {m.Row},{m.Col} {m.Size}");
                }
            }
        }
    }
}
