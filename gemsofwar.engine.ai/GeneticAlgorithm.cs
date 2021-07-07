using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.ai
{
    public class GeneticAlgorithm<T>
    {
        public GeneticAlgorithm(int seed = 0)
        {
            Random = seed == 0 ? new Random() : new Random(seed);
        }

        public T Fitness(IGeneticAlgorithmGenerator<T> generator) 
        {
            // init
            var generation = 0;
            var population = new List<T>();
            var sorter = new PopulationSorter(generator);

            // create the initial population
            for(int i=0; i<generator.PopulationSize; i++)
            {
                // get a random set of traits
                var traits = generator.Generate();

                // evaluate
                traits = generator.Evaluate(traits);

                population.Add(traits);
            }

            // repeat until max generation
            while(generation < generator.MaxGeneration)
            {
                // sort by fitness (highest to lowest)
                population.Sort(sorter);

                // build next generation
                var nextGenaration = new List<T>();

                // top 10% goes to the next generation
                var top10 = population.Count / 10;
                for (int i = 0; i < top10; i++) nextGenaration.Add(population[i]);

                // combine the remaining 90%
                for (int i=top10; i<generator.PopulationSize; i++)
                {
                    // 'mate' two configurations
                    var index1 = Random.Next() % generator.PopulationSize;
                    var index2 = index1;
                    while (index1 == index2) index2 = Random.Next() % generator.PopulationSize;

                    // produce a new configuration
                    var childtraits = Combine(generator, population[index1], population[index2]);

                    // evaluate
                    childtraits = generator.Evaluate(childtraits);
                    nextGenaration.Add(childtraits);
                }

                // print stats
                Console.WriteLine($"generation {generation}");
                Console.WriteLine($"fitness : {generator.GetFitness(nextGenaration[0])}");
                Console.WriteLine($"config  : {nextGenaration[0].ToString()}");

                // advance
                population = nextGenaration;
                generation++;
            }

            // return the fittest
            return population[0];
        }

        #region private
        private Random Random;

        class PopulationSorter : IComparer<T>
        {
            // sort fitness from High to Low
            public PopulationSorter(IGeneticAlgorithmGenerator<T> generator)
            {
                Generator = generator;
            }

            public int Compare(T x, T y)
            {
                var xf = Generator.GetFitness(x);
                var yf = Generator.GetFitness(y);
                return yf.CompareTo(xf);
            }

            #region private
            private IGeneticAlgorithmGenerator<T> Generator;
            #endregion
        }

        private T Combine(IGeneticAlgorithmGenerator<T> generator, T parent1, T parent2)
        {
            var child = generator.Generate();

            for(int i=0; i<generator.NumberOfTraits; i++)
            {
                // 0-99
                var probaility = Random.Next() % 100;

                if (probaility < 45)
                {
                    // from parent1
                    child = generator.SetTrait(src: parent1, dst: child, i);
                }
                else if (probaility < 90)
                {
                    // from parent2
                    child = generator.SetTrait(src: parent2, dst: child, i);
                }
                // a mutation of the gene
                else
                {
                    // keep the random generated value for this trait
                }
            }

            return child;
        }
        #endregion
    }
}
