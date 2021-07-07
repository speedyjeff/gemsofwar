using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.tests.engine
{
    static class MoveTest
    {
        private static void Moves(byte[][] matrix, int row, int col, Direction direction, int count = 1)
        {
            var board = Common.Setup(matrix);
            var before = board.ToString();
            var moves = board.AvailableMoves;

            Console.WriteLine("***************************************");
            foreach (var m in moves) Console.WriteLine($"{m.Row},{m.Col} {m.Direction} {m.Gems.Length}");
            Console.WriteLine(before);

            if (moves.Count != count) throw new Exception($"Invalid number of matches : {moves.Count}");
            if (moves[0].Row != row ||
                moves[0].Col != col ||
                moves[0].Direction != direction) throw new Exception($"invalid move information : {moves[0].Row},{moves[0].Col} {moves[0].Direction}");
        }

        public static void Run()
        {
            //
            // moves
            //
            Moves(matrix: new byte[][]
                {
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,1,0,0,0,0,0},
                    new byte[] {0,1,0,0,0,0,0,0},
                    new byte[] {0,0,1,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0}
                },
                row: 2,
                col: 1,
                direction: Direction.Right
                );
            Moves(matrix: new byte[][]
               {
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,1,0,0},
                    new byte[] {0,0,0,0,1,0,1,1},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0}
               },
               row: 1,
               col: 5,
               direction: Direction.Down,
               count: 2
               );
            Moves(matrix: new byte[][]
                {
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,1,0,1,0},
                    new byte[] {0,0,0,0,0,1,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0}
                },
                row: 2,
                col: 5,
                direction: Direction.Down
                );
            Moves(matrix: new byte[][]
                {
                    new byte[] {0,1,0,0,0,0,0,0},
                    new byte[] {0,0,1,1,0,0,0,0},
                    new byte[] {0,1,0,0,0,0,0,0},
                    new byte[] {0,1,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0}
                },
                row: 0,
                col: 1,
                direction: Direction.Down,
                count: 3
                );
            Moves(matrix: new byte[][]
                {
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,1,0,0,0,0},
                    new byte[] {0,1,1,0,1,1,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0}
                },
                row: 4,
                col: 3,
                direction: Direction.Down,
                count: 3
                );
            Moves(matrix: new byte[][]
                {
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,1,0,1,1},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0}
                },
                row: 3,
                col: 4,
                direction: Direction.Right
                );
            Moves(matrix: new byte[][]
                {
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {0,0,0,0,0,0,0,0},
                    new byte[] {1,0,0,0,0,0,0,0},
                    new byte[] {0,1,1,0,0,0,0,0}
                },
                row: 6,
                col: 0,
                direction: Direction.Down
                );
        }
    }
}
