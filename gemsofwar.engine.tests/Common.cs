using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.tests
{
    class Common
    {
        public const GemId MatchId = (GemId)999;
        public const GemId MarkerId = (GemId)100;

        public static GemsBoard Setup(byte[][] matrix)
        {
            var factory = new TestFactory();
            var initial = new Gem[matrix.Length][];
            for (int row = 0; row < initial.Length; row++)
            {
                initial[row] = new Gem[matrix[row].Length];
                for (int col = 0; col < initial[row].Length; col++)
                {
                    initial[row][col] = matrix[row][col] == 1 ? new Gem() { Id = MatchId, CanMatch = true, Effect = GemEffect.None } : factory.Next();
                }
            }
            // set a marker so that above we can track where new gems fall in
            factory.SetMarker(MarkerId);
            return new GemsBoard(factory, Rules.RemoveGemOnMatch, initial);
        }
    }
}
