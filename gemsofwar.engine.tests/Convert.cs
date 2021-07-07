using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.tests
{
    public static class Convert
    {
        public static Move ToMove(byte val)
        {
            if (val < 0 || val > 127) throw new Exception("valid values are from 0 to 127");

            // encoding
            // _   _  _ _ _  _ _ _
            // |  |    |      |
            // |  |    row    col
            // |  down/right
            // |
            // unused
            var direction = (val >> 6) & 0x1;
            return new Move()
            {
                Row = (val >> 3) & 0x7,
                Col = (val) & 0x7,
                Direction = (direction == 1) ? Direction.Right : Direction.Down
            };
        }

        public static byte ToMoveByte(Move move)
        {
            var row = move.Row;
            var col = move.Col;
            var dir = move.Direction;

            // can only encode Down/Right, so if another direction need to adjust the row/col
            if (dir == Direction.Up)
            {
                row--;
                dir = Direction.Down;
            }
            if (dir == Direction.Left)
            {
                col--;
                dir = Direction.Right;
            }

            // check if valid
            if (row < 0 || row > 7 ||
                col < 0 || col > 7 ||
                (dir != Direction.Right && dir != Direction.Down)) throw new Exception("valid inputs range from 0 to 7");

            return (byte)((dir == Direction.Right ? 0x40 : 0) |
                (row << 3) |
                (col));
        }

        // tests
        private static void Run()
        {
            RoundTrip(new Move() { Row = 2, Col = 4, Direction = Direction.Down }, adjust: false);
            RoundTrip(new Move() { Row = 7, Col = 3, Direction = Direction.Right }, adjust: false);

            RoundTrip(new Move() { Row = 0, Col = 7, Direction = Direction.Left }, adjust: true);
            RoundTrip(new Move() { Row = 5, Col = 0, Direction = Direction.Up }, adjust: true);

            RoundTrip(new Move() { Row = 0, Col = 0, Direction = Direction.Down }, adjust: false);
            RoundTrip(new Move() { Row = 7, Col = 6, Direction = Direction.Right }, adjust: false);
        }

        private static void RoundTrip(Move move, bool adjust)
        {
            var b1 = ToMoveByte(move);
            var m2 = ToMove(b1);
            var b2 = ToMoveByte(m2);

            Console.WriteLine("**********************************************");
            Console.WriteLine($"{move.Row},{move.Col},{move.Direction} {m2.Row},{m2.Col},{m2.Direction} {b1} {b2}");

            if (b1 != b2) throw new Exception("failed to round trip ToMoveByte : 0x{b1:x} 0x{b2:x}");
            var row = m2.Row;
            var col = m2.Col;
            var dir = m2.Direction;
            if (adjust)
            {
                if (dir == Direction.Right)
                {
                    dir = Direction.Left;
                    col++;
                }
                else if (dir == Direction.Down)
                {
                    dir = Direction.Up;
                    row++;
                }
            }
            if (move.Row != row ||
                    move.Col != col ||
                    move.Direction != dir) throw new Exception($"Invalid move : {move.Row},{row} {move.Col},{col} {move.Direction},{dir}");
        }
    }
}
