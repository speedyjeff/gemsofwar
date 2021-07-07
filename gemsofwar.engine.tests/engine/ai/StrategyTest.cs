using gemsofwar.engine.ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.tests.engine.ai
{
    static class StrategyTest
    {
        public static void Run(GameMode gamemode)
        {
            Realworld(gamemode);
            ValidateStrategy(matrix: new byte[][]
                {
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,1,0,0,0,0,0},
                    new byte[] {0,1,0,0,0,0,0,0},
                    new byte[] {0,0,1,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,1,1,0,1,0},
                    new byte[] {0,0,0,0,0,1,0,0}
                },
                row: 6,
                col: 5,
                direction: Direction.Down
                );
                ValidateStrategy(matrix: new byte[][]
                {
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,1,0,0,0,0,0},
                    new byte[] {0,1,0,0,0,0,0,0},
                    new byte[] {0,0,1,0,0,0,0,0},
                    new byte[] {0,0,0,0,1,1,0,0},
                    new byte[] {0,0,1,1,0,0,0,0},
                    new byte[] {0,0,0,0,1,1,0,0},
                    new byte[] {0,0,0,0,0,0,1,0}
                },
                row: 6,
                col: 6,
                direction: Direction.Down
                );
        }

        private static void ValidateStrategy(byte[][] matrix, int row, int col, Direction direction)
        {
            var board = Common.Setup(matrix);
            var strategies = Strategy.Analyze(
                board,
                depth: 2,
                gemIdOrder: new GemId[] { GemId.G1 });

            if (strategies == null || strategies.Count == 0) throw new Exception("no valid strategies");

            // validate that the top sorted strategy matches
            if (strategies[0].Move.Row != row ||
                strategies[0].Move.Col != col ||
                strategies[0].Move.Direction != direction) throw new Exception($"move did not match : {strategies[0].Move.Row} != {row} || {strategies[0].Move.Col} != {col} || {strategies[0].Move.Direction} != {direction}");
        }

        private static void Realworld(GameMode gamemode)
        {
            IGemFactory factory = (gamemode == GameMode.Battle) ? new BattleFactory(seed: 1234) : new TreasureFactory(seed: 1234);
            var board = new GemsBoard(factory, (gamemode == GameMode.Battle) ? Rules.RemoveGemOnMatch : Rules.UpgradeGemOnMatch);

            Console.WriteLine("**********************************");

            var strategies = Strategy.Analyze(
                board,
                depth: 2,
                gemIdOrder: new GemId[] { GemId.G7 });

            if (strategies.Count == 0) throw new Exception("failed to get the right number of strategies");

            foreach (var s in strategies)
            {
                Console.WriteLine($" @ {s.Move.Row},{s.Move.Col} {s.Move.Direction} {s.LargestGemCount} {s.CreatesLargestGemCount}");
                foreach (var kvp in s.GemCounts) Console.WriteLine($"  - [1] {kvp.Key} {kvp.Value}");
                foreach (var kvp in s.CreatesGemCounts) Console.WriteLine($"  - [2] {kvp.Key} {kvp.Value}");
            }
        }
    }
}
