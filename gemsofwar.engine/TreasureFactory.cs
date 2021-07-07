using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine
{
    public enum TreasureGemValue : byte { Bronze = 1, Silver = 2, Gold = 3, Bag = 4, Chest = 5, GreenChest = 6, RedChest = 7, Safe = 8 };

    public struct TreasureFactory : IGemFactory
    {
        public TreasureFactory(int seed)
        {
            Random = (seed == 0) ? new Random() : new Random(seed);
            NewGemPause = false;
            NextGems = null;
        }
        public bool NewGemPause { get; set; }
        public Gem Next()
        {
            if (NextGems == null || NextGems.Count == 0)
            {
                NextGems = new List<Gem>();
                foreach (var detail in Details)
                {
                    // add Probaility number of elements per Distribution
                    var count = (int)(Distribution * detail.Probability);
                    for (int i = 0; i < count; i++) NextGems.Add(Create(detail));
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
            }

            // get next
            var result = NextGems[0];
            NextGems.RemoveAt(0);
            return result;
        }

        public Gem Upgrade(Gem gem)
        {
            // when a match occurs on a gem it is upgraded to the next
            foreach (var detail in Details)
            {
                if (detail.Id == gem.Id)
                {
                    // find the upgrade detail
                    foreach(var upgrade in Details)
                    {
                        if (upgrade.Id == detail.UpgradeId)
                        {
                            return Create(upgrade);
                        }
                    }
                }
            }

            throw new Exception("Failed to make an upgrade gem");
        }

        public List<GemMap> GetMapping()
        {
            var results = new List<GemMap>();
            foreach (var detail in Details)
            {
                results.Add(new GemMap() { GemId = detail.Id, Id = (int)detail.TreasureId, Name = detail.TreasureId.ToString() });
            }
            return results;
        }

        #region private
        private Random Random;
        private List<Gem> NextGems;
        private const int Distribution = 200;

        struct Detail
        {
            public GemId Id;
            public GemId UpgradeId;
            public TreasureGemValue TreasureId;
            public float Probability;
            public bool CanMatch;
        }

        private static Detail[] Details = new Detail[]
                {
                    new Detail() { Id = GemId.G1, UpgradeId = GemId.G2, TreasureId = TreasureGemValue.Bronze, Probability = 0.5f, CanMatch = true},
                    new Detail() { Id = GemId.G2, UpgradeId = GemId.G3, TreasureId = TreasureGemValue.Silver, Probability = 0.25f, CanMatch = true},
                    new Detail() { Id = GemId.G3, UpgradeId = GemId.G4, TreasureId = TreasureGemValue.Gold, Probability = 0.1f, CanMatch = true},
                    new Detail() { Id = GemId.G4, UpgradeId = GemId.G5, TreasureId = TreasureGemValue.Bag, Probability = 0.05f, CanMatch = true},
                    new Detail() { Id = GemId.G5, UpgradeId = GemId.G6, TreasureId = TreasureGemValue.Chest, Probability = 0f, CanMatch = true},
                    new Detail() { Id = GemId.G6, UpgradeId = GemId.G7, TreasureId = TreasureGemValue.GreenChest, Probability = 0f, CanMatch = true},
                    new Detail() { Id = GemId.G7, UpgradeId = GemId.G8, TreasureId = TreasureGemValue.RedChest, Probability = 0f, CanMatch = true},
                    new Detail() { Id = GemId.G8, UpgradeId = GemId.G8, TreasureId = TreasureGemValue.Safe, Probability = 0f, CanMatch = false}
                };

        private Gem Create(Detail detail)
        {
            return new Gem() { Id = detail.Id, CanMatch = detail.CanMatch, Effect = GemEffect.None };
        }
        #endregion
    }
}
