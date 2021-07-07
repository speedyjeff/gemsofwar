using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine
{
    public struct GemMap
    {
        public int Id;
        public GemId GemId;
        public string Name;
    }

    public interface IGemFactory
    {
        public bool NewGemPause { get; set; }
        public Gem Next();
        public Gem Upgrade(Gem gem);
        public List<GemMap> GetMapping();
    }
}
