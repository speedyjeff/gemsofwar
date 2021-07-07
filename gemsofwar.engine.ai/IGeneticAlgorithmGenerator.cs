using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.ai
{
    public interface IGeneticAlgorithmGenerator<T>
    {
        public T Generate();
        public double GetFitness(T traits);
        public T Evaluate(T traits);

        // getting/setting individual traits
        public int NumberOfTraits { get; }
        public T SetTrait(T src, T dst, int index);

        // stats
        public int MaxGeneration { get; }
        public int PopulationSize { get; }
    }
}
