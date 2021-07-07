using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.ai
{
    // todo does loosing effects for gems matter?

    public class Compact
    {
        public Compact(int dimension)
        {
            var gems = new Gem[dimension][];
            for (int row = 0; row < gems.Length; row++)
            {
                gems[row] = new Gem[dimension];
                for(int col=0; col<gems[row].Length; col++)
                {
                    gems[row][col] = new Gem() { Id = GemId.Empty };
                }
            }
            Rules = Rules.RemoveGemOnMatch;
            Factory = null;

            Initialize(gems);
        }

        private Compact() { }
        public Compact(Compact other)
        {
            Height = other.Height;
            Width = other.Width;
            Factory = other.Factory;
            Rules = other.Rules;
            Raw = new byte[other.Raw.Length];
            for (int i = 0; i < Raw.Length; i++) Raw[i] = other.Raw[i];
        }

        public Compact(string hex)
        {
            // we can recreate the compact based on the hex representation
            Raw = System.Text.ASCIIEncoding.ASCII.GetBytes(hex);
            // this will produce a squre output:
            //   length = (Height * Width * BitsPerGem) / BitsPerRaw
            //   length * BitsPerRaw = dim^2 * BitsPerGem
            //   length * (BitsPerRaw / BitsPerGem) = dim^2
            //   sqrt(length * (BitsPerRaw / BitsPerGem)) = dim
            var dim = (int)Math.Floor(Math.Sqrt(Raw.Length * (BitsPerRaw / BitsPerGem)));

            // setup
            Width = Height = dim;
            Factory = null;
            Rules = Rules.RemoveGemOnMatch;
        }

        public Compact(GemsBoard board)
        {
            var gems = board.Gems;
            Rules = board.Rules;
            Factory = board.Factory;

            Initialize(gems);
        }

        public GemsBoard ToBoard()
        {
            // load gems into board
            var gems = new Gem[Height][];
            for (int row = 0; row < gems.Length; row++)
            {
                gems[row] = new Gem[Width];
                for (int col = 0; col < gems[row].Length; col++)
                {
                    gems[row][col] = new Gem() { Id = (GemId)this[row, col] };
                }
            }

            return new GemsBoard(Factory, Rules, gems);
        }

        public override string ToString()
        {
            return System.Text.ASCIIEncoding.ASCII.GetString(Raw);
        }

        public GemId this[int row,int col]
        {
            get
            {
                var start = (row * Width) + (col);
                var index = (start % 2 == 0) ? start / 2 : (start - 1) / 2;
                if (index >= Raw.Length) throw new Exception($"provided a row,col that is too large : {row},{col} {start} {index}");
                if (start % 2 == 0)
                {
                    return (GemId)(Raw[index] & 0xF);
                }
                else
                {
                    var val = Raw[index] & 0xF0;
                    return (GemId)(val >> BitsPerGem);
                }
            }
            set
            {
                var start = (row * Width) + (col);
                var index = (start % 2 == 0) ? start / 2 : (start - 1) / 2;
                if (index >= Raw.Length) throw new Exception($"provided a row,col that is too large : {row},{col} {start} {index}");
                if (start % 2 == 0)
                {
                    Raw[index] = (byte)((int)value & 0xF);
                }
                else
                {
                    var val = Raw[index] & 0xF;
                    Raw[index] = (byte)((((int)value & 0xF) << BitsPerGem) | val);
                }
            }
        }

        #region private
        private byte[] Raw;
        private int Width;
        private int Height;
        private IGemFactory Factory;
        private Rules Rules;

        private const int BitsPerGem = 4;
        private const int MaxCell = 15;
        private const int BitsPerRaw = 8;

        private void Initialize(Gem[][] gems)
        {
            Height = gems.Length;
            Width = gems[0].Length;

            // 3 bits per cell * area (round up)
            var length = (int)Math.Ceiling(((float)(Height * Width * BitsPerGem)) / (float)BitsPerRaw);
            Raw = new byte[length];

            // encode the gems in each cell
            for (int row = 0; row < gems.Length; row++)
            {
                if (gems[row].Length != Width) throw new Exception("must be a rectangle board");
                for (int col = 0; col < gems[row].Length; col++)
                {
                    if ((int)gems[row][col].Id > MaxCell) throw new Exception($"the cell exceeds the ability to compact : {(int)gems[row][col].Id}");
                    this[row, col] = gems[row][col].Id;
                }
            }
        }
        #endregion
    }
}
