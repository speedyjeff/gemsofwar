using gemsofwar.engine.ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.console
{
    static class ModelFitness
    {
        public static void Evaluate(string filename)
        {
            // read in data to use to train the model
            var modeldata = ModelData.Read(filename);
            var config = ImageClassifierConfig.FromDisk($"{filename}.config");
            var result = ImageClassifier.Evaluate(modeldata, config);

            Display(result);

        }

        public static void Evolve(string filename, int generations, int population)
        {
            Console.WriteLine("Running a genetic algorith to determine the best options for this model");

            // use a genetic algorithm to find the optimal solution
            var genetic = new GeneticAlgorithm<ImageClassifierConfig>();
            var generator = new PopulationGenerator(filename, generations, population);
            var config = genetic.Fitness(generator);

            // get the full stats for this config
            var modeldata = ModelData.Read(filename);
            var result = ImageClassifier.Evaluate(modeldata, config);

            // save this config to disk
            config.ToDisk($"{filename}.config");

            Display(result);
        }

        #region private
        private static void Display(ImageClassifierFitness result)
        {
            Console.WriteLine($"Split   : {result.TrainingSplitPercent}");
            Console.WriteLine($"R2      : {result.R2}");
            Console.WriteLine($"Total   : {result.Total}");
            Console.WriteLine($"Success : {result.SuccessfulPredictions}");
            Console.WriteLine("Features :");
            for (int i = 0; i < result.FeaturePercent.Length; i++)
            {
                Console.WriteLine($"  {i} {result.FeaturePercent[i]}");
            }
            Console.WriteLine("Labels   :");
            foreach (var kvp in result.LabelSuccessPercent)
            {
                Console.WriteLine($"  {kvp.Key} {kvp.Value}");
            }
        }
        #endregion
    }
}
