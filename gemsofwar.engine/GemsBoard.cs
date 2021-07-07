using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace gemsofwar.engine
{
    public enum Rules { RemoveGemOnMatch, UpgradeGemOnMatch }
    public class GemsBoard
    {
        static GemsBoard()
        {
            // enable matching with empty to enable predictions
            Empty = new Gem() { Id = GemId.Empty, CanMatch = true };
            Marked = new Gem { Id = (GemId)Int32.MaxValue, CanMatch = true };
        }

        public GemsBoard(IGemFactory factory) : this(factory, rules: Rules.RemoveGemOnMatch, gems: null) { }
        public GemsBoard(IGemFactory factory, Rules rules) : this(factory, rules, gems: null) { }

        public GemsBoard(IGemFactory factory, Rules rules, Gem[][] gems)
        {
            // store the gems for later
            Factory = factory;
            Rules = rules;

            // initalize the board
            Region = new Gem[gems == null ? Height : gems.Length][];
            for (int row = 0; row < Region.Length; row++)
            {
                Region[row] = new Gem[gems == null ? Width : gems[row].Length];
                for (int col = 0; col < Region[row].Length; col++)
                {
                    Region[row][col] = gems == null ? Empty.Copy() : gems[row][col].Copy();
                }
            }

            // fill the board
            Resolve();
            // set available moves
            AvailableMoves = Moves();
        }

        public Rules Rules { get; private set; }
        public IGemFactory Factory { get; private set; }
        public List<Move> AvailableMoves { get; private set; }
        public Gem[][] Gems
        {
            get
            {
                // make a copy of Region
                var copy = new Gem[Region.Length][];
                for(int row=0; row<Region.Length; row++)
                {
                    copy[row] = new Gem[Region[row].Length];
                    for(int col=0; col<Region[row].Length; col++)
                    {
                        copy[row][col] = Region[row][col].Copy();
                    }
                }
                return copy;
            }
        }

        public bool TryMakeMove(Move move, out List<Match> matches)
        {
            matches = null;

            // check if valid
            if (AvailableMoves.Count == 0) return false;
            if (move.Row < 0 || move.Col < 0 ||
                move.Row >= Region.Length || 
                move.Col >= Region[move.Row].Length) return false;
            if (!Region[move.Row][move.Col].CanMatch) return false;
            switch (move.Direction)
            {
                case Direction.Up:
                    if (move.Row < 1) return false;
                    if (!Region[move.Row-1][move.Col].CanMatch) return false;
                    break;
                case Direction.Left:
                    if (move.Col < 1) return false;
                    if (!Region[move.Row][move.Col-1].CanMatch) return false;
                    break;
                case Direction.Right:
                    if (move.Col >= Region[move.Row].Length - 1) return false;
                    if (!Region[move.Row][move.Col+1].CanMatch) return false;
                    break;
                case Direction.Down:
                    if (move.Row >= Region.Length - 1) return false;
                    if (!Region[move.Row+1][move.Col].CanMatch) return false;
                    break;
                default: throw new Exception($"invalid direction : {move.Direction}");
            }

            // try apply move
            lock (this)
            {
                // apply
                switch (move.Direction)
                {
                    case Direction.Up:
                        Swap(move.Row, move.Col, move.Row - 1, move.Col);
                        break;
                    case Direction.Left:
                        Swap(move.Row, move.Col, move.Row, move.Col - 1);
                        break;
                    case Direction.Right:
                        Swap(move.Row, move.Col, move.Row, move.Col + 1);
                        break;
                    case Direction.Down:
                        Swap(move.Row, move.Col, move.Row + 1, move.Col);
                        break;
                    default: throw new Exception($"invalid direction : {move.Direction}");
                }

                // resolve
                matches = Resolve(move, matchfirst: true);

                if (matches.Count == 0)
                {
                    // unapply
                    switch (move.Direction)
                    {
                        case Direction.Up:
                            Swap(move.Row - 1, move.Col, move.Row, move.Col);
                            break;
                        case Direction.Left:
                            Swap(move.Row, move.Col - 1, move.Row, move.Col);
                            break;
                        case Direction.Right:
                            Swap(move.Row, move.Col + 1, move.Row, move.Col);
                            break;
                        case Direction.Down:
                            Swap(move.Row + 1, move.Col, move.Row, move.Col);
                            break;
                        default: throw new Exception($"invalid direction : {move.Direction}");
                    }

                    // this was not a valid change, unapply
                    return false;
                }

                // set available moves
                AvailableMoves = Moves();

                return true;
            }
        }

        public void Shuffle()
        {
            throw new Exception("nyi");
        }

        public override string ToString()
        {
            var board = new StringBuilder();
            for(int row=0; row<Region.Length;row++)
            {
                for(int col=0; col<Region[row].Length; col++)
                {
                    board.Append($"| {Region[row][col].Id} ");
                }
                board.AppendLine("|");
                for (int col = 0; col < Region[row].Length; col++) board.Append("----");
                board.AppendLine("-");
            }

            return board.ToString();
        }

        #region private
        private Gem[][] Region;

        private static Gem Empty;
        private static Gem Marked;

        private const int Width = 8;
        private const int Height = 8;

        private List<Match> Resolve(Move move = default(Move), bool matchfirst = false)
        {
            var matches = new List<Match>();

            // have gems fall, and resolve matches
            var skipFillNDrop = matchfirst;
            while (true)
            {
                if (!skipFillNDrop)
                {
                    // add a row of gems
                    if (!Factory.NewGemPause)
                    {
                        Fill();

                        // no gaps on the top row
                        if (CheckRowEqualTo(row: 0, Empty)) throw new Exception("Top row is missing a value");
                    }

                    // have gems fall
                    if (Drop()) continue;

                    // no empty spaces below a gem
                    if (CheckForAirGaps()) throw new Exception("An air gap exists");
                }

                // resolve matches
                var resolved = 0;
                var effects = new List<EffectRegion>();
                while (true)
                {
                    var before = ToString();

                    var match = Match(ref effects, move);
                    if (match.Size == 0) break;
                    resolved++;
                    matches.Add(match);
                }

                // if there are effects, apply them
                Effects(effects);

                // done
                if (resolved == 0) break;

                // always renable fill and drop
                skipFillNDrop = false;
            }

            return matches;
        }

        private bool Fill()
        {
            if (Factory.NewGemPause) return false;

            var count = 0;

            // add gems to empty slots
            var row = 0;
            for (int col = 0; col < Region[row].Length; col++)
            {
                if (Region[row][col].Id == Empty.Id)
                {
                    // get the next gem
                    Region[row][col] = Factory.Next();
                    count++;
                }
            }

            return count > 0;
        }

        private bool Drop()
        {
            var count = 0;

            // drop gems that should drop (1 row)
            for(int row=Region.Length - 1; row>0; row--)
            {
                for(int col=0;col<Region[row].Length; col++)
                {
                    // check if the current gem is empty
                    if (Region[row][col].Id == Empty.Id)
                    {
                        // check if the gem above is not empty
                        if (Region[row-1][col].Id != Empty.Id)
                        {
                            // swap
                            Region[row][col] = Region[row - 1][col];
                            Region[row - 1][col] = Empty.Copy();
                            count++;
                        }
                    }
                }
            }

            return count > 0;
        }

        private Match Match(ref List<EffectRegion> effects, Move move = default(Move))
        {
            // find 1 match and resolve it (3 or more side to side or up and down)
            for (int row=0; row<Region.Length; row++)
            {
                for (int col = 0; col < Region.Length; col++)
                {
                    if (Region[row][col].Id == Empty.Id) continue;

                    if ((
                        // check right
                        col <= Region[row].Length - 3 &&
                        Region[row][col].Id == Region[row][col + 1].Id &&
                        Region[row][col + 1].Id == Region[row][col + 2].Id) ||
                        (
                        // check down
                        row <= Region.Length - 3 &&
                        Region[row][col].Id == Region[row + 1][col].Id &&
                        Region[row + 1][col].Id == Region[row + 2][col].Id
                        ))
                    {
                        var gem = Region[row][col].Copy();
                        var count = RemoveMatch(row, col, Region[row][col].Id, ref effects);
                        var after = ToString();
                        if (count == 0) throw new Exception("Expected to remove a match");

                        // apply rule set
                        if (Rules == Rules.UpgradeGemOnMatch)
                        {
                            // upgrade the gem
                            var upgradedgem = Factory.Upgrade(gem);

                            // find the inflection point
                            var point = FindInflectionPoint(move);

                            // validate
                            if (point.Row < 0 || point.Row >= Region.Length ||
                                point.Col < 0 || point.Col >= Region[point.Row].Length ||
                                Region[point.Row][point.Col].Id != Marked.Id) throw new Exception("Invalid inflection point");

                            // place this gem at the inflection point
                            Region[point.Row][point.Col] = upgradedgem.Copy();
                        }

                        // unmark items in the region
                        SetToEmpty(Marked);

                        // return the match
                        return new Match()
                        {
                            Gem = gem,
                            Row = row,
                            Col = col,
                            Size = count
                        };
                    }
                }
            }

            return new Match() { Size = 0 };
        }

        private void Effects(List<EffectRegion> effects)
        {
            foreach (var e in effects)
            {
                switch (e.Effect)
                {
                    case GemEffect.Explode9x9:
                        // set the gems within this 9x9 to Empty
                        for (int row = e.Row - 1; row <= e.Row + 1; row++)
                        {
                            if (row < 0 || row >= Region.Length) continue;
                            for (int col = e.Col - 1; col <= e.Col + 1; col++)
                            {
                                if (col < 0 || col >= Region[row].Length) continue;
                                Region[row][col] = Empty.Copy();
                            }
                        }
                        break;
                    default: throw new Exception($"unknown effect : {e.Effect}");
                }
            }
        }

        private List<Move> Moves()
        {
            // generate a list of all valid moves
            // removes duplicates (eg. 1,1 slide down is the same as 2,1 slide up)
            var moves = new List<Move>();

            for(int row=0; row < Region.Length; row++)
            {
                for(int col=0; col<Region.Length; col++)
                {
                    // evaluate if moving row,col RIGHT/DOWN would result in 3 in a row

                    // RIGHT: swap A with B
                    //           -2 -1 c  +1 +2 +3
                    // -2              B? A?
                    // -1              B? A?
                    // r  (bhor) B? B? A  B  A? A? (ahor)
                    // +1              B? A?
                    // +2              B? A?
                    //          bvert  avert
                    if (col <= Region[row].Length - 2)
                    {
                        var threeinarowa = false;
                        var threeinarowb = false;
                        var A = Region[row][col];
                        var B = Region[row][col+1];
                        var ahor = new bool[5] { true, Check(A, row, col+2), Check(A, row, col+3), false, false };
                        var bhor = new bool[5] { Check(B, row, col-2), Check(B, row , col-1), true, false, false };
                        var avert = new bool[5] { Check(A,row-2,col+1), Check(A, row - 1, col + 1), true, Check(A, row + 1, col + 1), Check(A, row + 2, col + 1) };
                        var bvert = new bool[5] { Check(B,row-2,col), Check(B, row - 1, col), true, Check(B, row + 1, col), Check(B, row + 2, col) };

                        // check if 3 in a row
                        for (int i = 2; i < ahor.Length; i++)
                        {
                            threeinarowa |= (ahor[i - 2] && ahor[i - 1] && ahor[i]);
                            threeinarowb |= (bhor[i - 2] && bhor[i - 1] && bhor[i]);
                            threeinarowa |= (avert[i - 2] && avert[i - 1] && avert[i]);
                            threeinarowb |= (bvert[i - 2] && bvert[i - 1] && bvert[i]);
                        }

                        // moving RIGHT results in a match
                        if (threeinarowa || threeinarowb)
                        {
                            if ((threeinarowa && A.Id == GemId.Empty) || (threeinarowb && B.Id == GemId.Empty)) throw new Exception("invalid match on an empty gem");
                            moves.Add(new Move()
                            {
                                Gems = (threeinarowa && threeinarowb) ? new Gem[] { A, B } : (threeinarowa ? new Gem[] { A } : new Gem[] { B }),
                                Row = row,
                                Col = col,
                                Direction = Direction.Right
                            });
                        }
                    }

                    // DOWN: swap A with B
                    //     -2 -1 c +1 +2
                    //         (bvert)
                    // -2        B?
                    // -1        B?
                    // r   B? B? A B? B? (bhor)
                    // +1  A? A? B A? A? (ahor)
                    // +2        A?
                    // +3        A?
                    //         (avert)
                    if (row <= Region.Length - 2)
                    {
                        var threeinarowa = false;
                        var threeinarowb = false;
                        var A = Region[row][col];
                        var B = Region[row+1][col];
                        var avert = new bool[5] { true, Check(A, row+2, col), Check(A, row+3, col), false, false };
                        var bvert = new bool[5] { Check(B, row - 2, col), Check(B, row - 1, col), true, false, false };
                        var ahor = new bool[5] { Check(A, row +1, col-2), Check(A, row + 1, col - 1), true, Check(A, row + 1, col + 1), Check(A, row + 1, col + 2) };
                        var bhor = new bool[5] { Check(B, row, col - 2), Check(B, row, col - 1), true, Check(B, row, col+1), Check(B, row, col+2) };

                        // check if 3 in a row
                        for (int i = 2; i < ahor.Length; i++)
                        {
                            threeinarowa |= (ahor[i - 2] && ahor[i - 1] && ahor[i]);
                            threeinarowb |= (bhor[i - 2] && bhor[i - 1] && bhor[i]);
                            threeinarowa |= (avert[i - 2] && avert[i - 1] && avert[i]);
                            threeinarowb |= (bvert[i - 2] && bvert[i - 1] && bvert[i]);
                        }

                        // moving DOWN results in a match
                        if (threeinarowa || threeinarowb)
                        {
                            if ((threeinarowa && A.Id == GemId.Empty) || (threeinarowb && B.Id == GemId.Empty)) throw new Exception("invalid match on an empty gem");
                            moves.Add(new Move()
                            {
                                Gems = (threeinarowa && threeinarowb) ? new Gem[] { A, B } : (threeinarowa ? new Gem[] { A } : new Gem[] { B }),
                                Row = row,
                                Col = col,
                                Direction = Direction.Down
                            });
                        }
                    }
                }
            }

            return moves;
        }

        private int RemoveMatch(int row, int col, GemId id, ref List<EffectRegion> effects)
        {
            // replace all instances of id starting at row,col
            if (row < 0 || col < 0 || 
                row >= Region.Length ||
                col >= Region[row].Length ||
                Region[row][col].Id == Empty.Id ||
                Region[row][col].Id == Marked.Id ||
                Region[row][col].Id != id) return 0;
            else
            {
                if (Region[row][col].Id != id) throw new Exception("Invalid remove state");

                // check that we are in at least a row/column of three
                // need to check id or marked as some values may already be replaced with empty
                var rgems = new bool[5];
                var cgems = new bool[5];
                for(int i=0;i<rgems.Length && i<cgems.Length; i++)
                {
                    var r = (row - 2) + i;
                    var c = (col - 2) + i;
                    // top down
                    if (r >= 0 && r < Region.Length)
                    {
                        rgems[i] = Region[r][col].Id == id || Region[r][col].Id == Marked.Id;
                    }

                    // left right
                    if (c >= 0 && c < Region[row].Length)
                    {
                        cgems[i] = Region[row][c].Id == id || Region[row][c].Id == Marked.Id;
                    }
                }

                // check that there is a three in the row
                var threeinarow = false;
                for (int i = 2; i < rgems.Length && i < cgems.Length; i++)
                {
                    threeinarow |= (rgems[i - 2] && rgems[i - 1] && rgems[i]);
                    threeinarow |= (cgems[i - 2] && cgems[i - 1] && cgems[i]);
                }

                // remove
                var count = 0;
                if (threeinarow)
                {
                    // if effect, then add to queue to apply latter
                    if (Region[row][col].Effect != GemEffect.None)
                    {
                        effects.Add(new EffectRegion() { Row = row, Col = col, Effect = Region[row][col].Effect });
                    }

                    // mark as marked
                    Region[row][col] = Marked.Copy();
                    count++;

                    // check the neighbors
                    count += RemoveMatch(row + 1, col, id, ref effects);
                    count += RemoveMatch(row - 1, col, id, ref effects);
                    count += RemoveMatch(row, col + 1, id, ref effects);
                    count += RemoveMatch(row, col - 1, id, ref effects);
                }

                return count;
            }
        }

        private Move FindInflectionPoint(Move move)
        {
            // find the point within the region (with Marked.Id)
            // that would be the point where the connection is made
            // IF {move.Row,move.Col} is within that shape, use that

            // check if this were a move
            if (move.Direction != Direction.None)
            {
                var mrow1 = move.Row;
                var mcol1 = move.Col;
                var mrow2 = move.Row;
                var mcol2 = move.Col;

                // set in the place where it 
                switch (move.Direction)
                {
                    case Direction.Up:
                        mrow1--;
                        break;
                    case Direction.Left:
                        mcol1--;
                        break;
                    case Direction.Right:
                        mcol2++;
                        break;
                    case Direction.Down:
                        mrow2++;
                        break;
                    default: throw new Exception($"Inavlid direction : {move.Direction}");
                }

                // check if these points are within the marked region
                if (mrow1 >= 0 && mrow1 < Region.Length &&
                    mcol1 >= 0 && mcol1 < Region[mrow1].Length &&
                    Region[mrow1][mcol1].Id == Marked.Id) return new Move() { Row = mrow1, Col = mcol1 };
                if (mrow2 >= 0 && mrow2 < Region.Length &&
                    mcol2 >= 0 && mcol2 < Region[mrow2].Length &&
                    Region[mrow2][mcol2].Id == Marked.Id) return new Move() { Row = mrow2, Col = mcol2 };
            }

            // search for indices
            var points = new List<Move>();
            for(int row = 0; row<Region.Length; row++)
            {
                for(int col=0; col<Region[row].Length; col++)
                {
                    if (Region[row][col].Id == Marked.Id) points.Add(new Move() { Row = row, Col = col }); 
                }
            }

            if (points.Count == 0) throw new Exception("failed to find inflection point");

            // choose one as the inflection point
            // todo ... make this better
            return points[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Check(Gem gem, int row, int col)
        {
            if (row < 0 || col < 0 ||
                row >= Region.Length || col >= Region[row].Length ||
                !gem.CanMatch ||
                !Region[row][col].CanMatch ||
                gem.Id == GemId.Empty ||
                Region[row][col].Id == GemId.Empty) return false;
            return gem.Id == Region[row][col].Id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap(int row1, int col1, int row2, int col2)
        {
            Gem tmp = Region[row1][col1];
            Region[row1][col1] = Region[row2][col2];
            Region[row2][col2] = tmp;
        }

        private void SetToEmpty(Gem gem)
        {
            for(int row=0;row<Region.Length; row++)
            {
                for(int col=0; col<Region[row].Length; col++)
                {
                    if (Region[row][col].Id == gem.Id) Region[row][col] = Empty.Copy();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckRowEqualTo(int row, Gem gem)
        {
            // validate that the top row is not empty
            for (int col = 0; col < Region[row].Length; col++) if (Region[row][col].Id == gem.Id) return true;
            return false;
        }

        private bool CheckForAirGaps()
        {
            // check if there are gems with Empty below
            for(int row=0; row<Region.Length-1;row++)
            {
                for(int col=0;col<Region[row].Length; col++)
                {
                    if (Region[row][col].Id != Empty.Id && Region[row + 1][col].Id == Empty.Id) return true;
                }
            }

            return false;
        }

        #endregion
    }
}
