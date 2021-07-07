using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.tests.engine
{
    static class MatchTest
    {
        private static void Match(byte[][] matrix, int size, int row, int col, Direction direction)
        {
            var board = Common.Setup(matrix);
            var before = board.ToString();
            if (!board.TryMakeMove(new Move() { Row = row, Col = col, Direction = direction }, out List<gemsofwar.engine.Match> matches))
                throw new Exception("failed to make the move");
            var after = board.ToString();

            Console.WriteLine("***************************************");
            foreach (var m in matches) Console.WriteLine($"{m.Gem.Id} {m.Row},{m.Col} {m.Size}");
            Console.WriteLine(before);
            Console.WriteLine(after);

            if (matches.Count != 1) throw new Exception($"wrong number of matches : {matches.Count}");
            if (matches[0].Gem.Id != Common.MatchId) throw new Exception($"invalid gem id : {matches[0].Gem.Id}");
            if (matches[0].Size != size) throw new Exception($"invalid size: {size} != {matches[0].Size}");

            // verify that the resulting matrix is correct
            // adjust matrix based on move
            matrix[row][col] = 0;
            switch (direction)
            {
                case Direction.Up: matrix[row - 1][col] = 1; break;
                case Direction.Left: matrix[row][col - 1] = 1; break;
                case Direction.Right: matrix[row][col + 1] = 1; break;
                case Direction.Down: matrix[row + 1][col] = 1; break;
                default: throw new Exception($"invalid direction : {direction}");
            }
            // check that columns have the right number of new gems
            var gems = board.Gems;
            for (int c = 0; c < gems[0].Length; c++)
            {
                var potential = 0;
                var actual = 0;
                var leftover = 0;
                for (int r = 0; r < gems.Length; r++)
                {
                    if (matrix[r][c] == 1) potential++;
                    if (gems[r][c].Id == Common.MatchId) leftover++;
                    else if (gems[r][c].Id >= Common.MarkerId) actual++;
                }

                if (potential - leftover != actual) throw new Exception($"failed to have enough gems : {potential}-{leftover} != {actual}");
            }
        }

        public static void Run()
        {
            //
            // test match configurations
            //
            Match(matrix: new byte[][]
               {
                    new byte[] {0,0,1,0,0,0,0,0},
                    new byte[] {0,1,0,0,0,0,0,0},
                    new byte[] {0,1,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0}
               },
               size: 3,
               row: 0,
               col: 2,
               direction: Direction.Left
               );
            Match(matrix: new byte[][]
                {
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,1,0,0},
                    new byte[] {0,0,0,0,1,0,1,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0}
                },
                size: 3,
                row: 3,
                col: 5,
                direction: Direction.Down
                );
            Match(matrix: new byte[][]
                {
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,1,0,0,0,0,0,0},
                    new byte[] {0,0,1,1,0,0,0,0},
                    new byte[] {0,1,0,1,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0}
                },
                size: 3,
                row: 4,
                col: 1,
                direction: Direction.Up
                );
            Match(matrix: new byte[][]
                {
                    new byte[] {0,0,0,0,0,1,0,0},
                    new byte[] {0,0,0,0,0,0,1,0},
                    new byte[] {0,0,0,0,1,0,1,0},
                    new byte[] {0,0,0,0,0,1,0,0},
                    new byte[] {0,0,0,0,1,0,1,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,1,0},
                    new byte[] {0,0,0,0,0,0,0,0}
                },
                size: 4,
                row: 3,
                col: 5,
                direction: Direction.Right
                );
            Match(matrix: new byte[][]
                {
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,1},
                    new byte[] {0,0,0,0,0,0,0,1},
                    new byte[] {0,0,0,0,0,1,1,0},
                    new byte[] {0,0,0,0,0,0,0,1}
                },
                size: 5,
                row: 7,
                col: 7,
                direction: Direction.Up
                );
            Match(matrix: new byte[][]
                {
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,1,0,0,0},
                    new byte[] {0,0,0,1,1,1,0,0},
                    new byte[] {0,0,0,0,0,1,0,0},
                    new byte[] {0,0,0,0,1,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0}
                },
                size: 3,
                row: 3,
                col: 5,
                direction: Direction.Left
                );
            Match(matrix: new byte[][]
                {
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,1,1,0,0,0,0},
                    new byte[] {1,1,0,1,0,0,0,0}
                },
                size: 3,
                row: 7,
                col: 3,
                direction: Direction.Left
                );
        }
    }
}
