using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine
{
    public enum BattleGemValue : byte { Red = 1, Brown = 2, Green = 3, Blue = 4, Purple = 5, Yellow = 6, Skull = 7, Barrier = 8 };
    
    public class BattleFactory : IGemFactory
    {
        public BattleFactory(int seed)
        {
            Random = (seed == 0) ? new Random() : new Random(seed);
        }

        public bool NewGemPause { get; set; }
        public Gem Next()
        {
            if (NextGems == null) NextGems = new List<Gem>();

            if (NextGems.Count == 0)
            {
                foreach (var detail in Probailities)
                {
                    // skip until it is ready to appear
                    if (detail.Round > NextGemsRound) continue;

                    // add Probaility number of elements per Distribution
                    var count = (int)(Distribution * detail.Probability);
                    for (int i = 0; i < count; i++) NextGems.Add(new Gem() { Id = detail.Id, CanMatch = detail.CanMatch, Effect = detail.Effect });
                }

                // randomize the results
                int index;
                for (int i = 0; i < NextGems.Count; i++)
                {
                    // get random index
                    do
                    {
                        index = Random.Next() % NextGems.Count;
                    }
                    while (i == index);
                    // swap
                    var gem = NextGems[i];
                    NextGems[i] = NextGems[index];
                    NextGems[index] = gem;
                }

                // increment round
                NextGemsRound++;
            }

            // get next
            var result = NextGems[0];
            NextGems.RemoveAt(0);
            return result;
        }

        public Gem Upgrade(Gem gem)
        {
            throw new Exception("BattleGem does not support upgraded gems");
        }

        public List<GemMap> GetMapping()
        {
            var results = new List<GemMap>();
            foreach (var detail in Probailities)
            {
                results.Add(new GemMap() { GemId = detail.Id, Id = (int)detail.BattleId, Name = detail.BattleId.ToString() });
            }
            return results;
        }

        #region private
        private Random Random;
        private List<Gem> NextGems;
        private int NextGemsRound = 0;
        private const int Distribution = 200;

        struct GemDetail
        {
            public GemId Id;
            public BattleGemValue BattleId;
            public GemEffect Effect;
            public float Probability;
            public int Round;
            public bool CanMatch;
        }

        private static GemDetail[] Probailities = new GemDetail[]
        {
                    new GemDetail() { Id = GemId.G1, BattleId = BattleGemValue.Red, Effect = GemEffect.None, Round = 0, Probability = 0.2f, CanMatch = true },
                    new GemDetail() { Id = GemId.G2, BattleId = BattleGemValue.Brown, Effect = GemEffect.None, Round = 0, Probability = 0.2f, CanMatch = true },
                    new GemDetail() { Id = GemId.G3, BattleId = BattleGemValue.Green, Effect = GemEffect.None, Round = 0, Probability = 0.2f, CanMatch = true },
                    new GemDetail() { Id = GemId.G4, BattleId = BattleGemValue.Blue, Effect = GemEffect.None, Round = 0, Probability = 0.2f, CanMatch = true },
                    new GemDetail() { Id = GemId.G5, BattleId = BattleGemValue.Purple, Effect = GemEffect.None, Round = 0, Probability = 0.2f, CanMatch = true },
                    new GemDetail() { Id = GemId.G6, BattleId = BattleGemValue.Yellow, Effect = GemEffect.None, Round = 0, Probability = 0.2f, CanMatch = true },
                    new GemDetail() { Id = GemId.G7, BattleId = BattleGemValue.Skull, Effect = GemEffect.None, Round = 0, Probability = 0.1f, CanMatch = true },
                    new GemDetail() { Id = GemId.G7, BattleId = BattleGemValue.Skull, Effect = GemEffect.Explode9x9, Round = 2, Probability = 0.01f, CanMatch = true },
                    new GemDetail() { Id = GemId.G8, BattleId = BattleGemValue.Barrier, Effect = GemEffect.None, Round = 0, Probability = 0f, CanMatch = false }
        };
        #endregion
    }
}
