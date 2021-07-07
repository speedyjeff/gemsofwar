using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.tests
{
    class TestFactory : IGemFactory
    {
        public bool NewGemPause { get; set; }
        public Gem Next()
        {
            return new Gem() { Id = (GemId)(++Counter), CanMatch = true, Effect = GemEffect.None };
        }

        public Gem Upgrade(Gem gem)
        {
            throw new Exception("nyi");
        }

        public void SetMarker(GemId id)
        {
            Counter = (int)id;
        }

        public List<GemMap> GetMapping()
        {
            throw new NotImplementedException();
        }

        #region private
        private int Counter = 0;
        #endregion
    }
}
