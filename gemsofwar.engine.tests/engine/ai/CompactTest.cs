using gemsofwar.engine.ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.tests.engine.ai
{
    static class CompactTest
    {
        public static void Run(GameMode gamemode)
        {
            IGemFactory factory = (gamemode == GameMode.Battle) ? new BattleFactory(seed: 1234) : new TreasureFactory(seed: 1234);
            var board = new GemsBoard(factory, (gamemode == GameMode.Battle) ? Rules.RemoveGemOnMatch : Rules.UpgradeGemOnMatch);

            Console.WriteLine("**********************************");

            var compact = new Compact(board);
            var board2 = compact.ToBoard();
            var hex = compact.ToString();
            var compact2 = new Compact(hex);
            var hex2 = compact2.ToString();

            Console.WriteLine($"{hex}");

            // check that these round trip
            var igems = board.Gems;
            var rgems = board2.Gems;

            if (igems.Length != rgems.Length) throw new Exception("failed to produce the same size");
            if (board.Factory != board2.Factory) throw new Exception("did not get the right factory");
            if (board.Rules != board2.Rules) throw new Exception("failed to get the same rules");
            if (!string.Equals(hex, hex2)) throw new Exception($"failed to get same ToString values : {hex} != {hex2}");

            for (int row = 0; row < igems.Length; row++)
            {
                if (igems[row].Length != rgems[row].Length) throw new Exception($"failed to get the same column length : {row} {igems[row].Length} != {rgems[row].Length}");
                for (int col = 0; col < igems[row].Length; col++)
                {
                    if (igems[row][col].Id != rgems[row][col].Id) throw new Exception($"failed to get the same gem : {row},{col} {igems[row][col].Id} != {rgems[row][col].Id}");
                    if (compact[row, col] != igems[row][col].Id) throw new Exception($"failed to get the id : {row},{col} {igems[row][col].Id} != {compact[row, col]}");
                    if (compact[row, col] != compact2[row, col]) throw new Exception($"failed to get same value in ToString round trip : {row},{col}");
                }
            }

        }
    }
}
