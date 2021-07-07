using gemsofwar.engine.ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.tests.engine.ai
{
    static class GeneticAlgorithmTest
    {
        public static void Run()
        {
            // predict the string "food"
            var generator = new Generator(seed: 1234, expected: new Data() { Parts = new char[] { 'f', 'o', 'o', 'd' } });
            var algo = new GeneticAlgorithm<Data>(seed: 999);

            var fittest = algo.Fitness(generator);

            Console.WriteLine($"'{fittest.ToString()}' {fittest.Fitness}");

            if (fittest.Fitness != 1 ||
                fittest.Parts[0] != 'f' ||
                fittest.Parts[1] != 'o' ||
                fittest.Parts[2] != 'o' ||
                fittest.Parts[3] != 'd') throw new Exception($"failed to find match");
        }
    }

    class Data
    {
        public char[] Parts;
        public double Fitness;

        public Data()
        {
            Parts = new char[4];
        }

        public override string ToString()
        {
            return $"{Parts[0]}{Parts[1]}{Parts[2]}{Parts[3]}";
        }
    }

    class Generator : IGeneticAlgorithmGenerator<Data>
    {
        public Generator(int seed, Data expected)
        {
            Random = new Random(seed);
            Expected = expected;
        }

        public int NumberOfTraits { get { return 4; } }

        public int MaxGeneration { get { return 130; } }

        public int PopulationSize { get { return 10; } }

        public Data Evaluate(Data traits)
        {
            // percentage that match the expected (order matters)
            var correct = 0;
            for (int i = 0; i < traits.Parts.Length && i < Expected.Parts.Length; i++)
            {
                if (traits.Parts[i] == Expected.Parts[i]) correct++;
            }
            traits.Fitness = (double)correct / (double)Expected.Parts.Length;

            return traits;
        }

        public Data Generate()
        {
            // give a random 4 character string of a-z
            var data = new Data();
            for(int i=0; i<data.Parts.Length; i++)
            {
                data.Parts[i] = (char)((Random.Next() % 26) + (int)'a');
            }

            return data;
        }

        public double GetFitness(Data traits)
        {
            return traits.Fitness;
        }

        public Data SetTrait(Data src, Data dst, int index)
        {
            dst.Parts[index] = src.Parts[index];
            return dst;
        }

        #region private
        private Random Random;
        private Data Expected;
        #endregion
    }
}
