using gemsofwar.engine.ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.console
{
    class PopulationGenerator : IGeneticAlgorithmGenerator<ImageClassifierConfig>
    {
        public PopulationGenerator(string modeldatafilename, int generations, int population)
        {
            CachedModelData = ModelData.Read(modeldatafilename);
            MaxGeneration = generations;
            PopulationSize = population;
            Random = new Random();
            // using a static seed to provide the same split for all inputs
            do
            {
                Seed = Random.Next();
            }
            while (Seed == 0);
        }

        public int NumberOfTraits { get { return NumberOfModelTraits + NumberOfOhterTraits + NumberOfFeatureTraits; } }

        public int MaxGeneration { get; private set; }

        public int PopulationSize { get; private set; }

        public ImageClassifierConfig Evaluate(ImageClassifierConfig traits)
        {
            // evaluate this configuration
            var result = ImageClassifier.Evaluate(CachedModelData, traits, Seed);

            // use the lowest prediction success as the fitness
            traits.Fitness = double.MaxValue;
            foreach(var kvp in result.LabelSuccessPercent) if (kvp.Value < traits.Fitness) traits.Fitness = kvp.Value;

            if (traits.Fitness == double.MaxValue) throw new Exception("did not find a suitable fitness to use for this model");

            return traits;
        }

        public ImageClassifierConfig Generate()
        {
            // generate a new random config
            var config = new ImageClassifierConfig()
            {
                // todo is there a better place to infer this?
                NumberOfFeatures = NumberOfFeatureTraits
            };

            //
            // model
            //

            // 1 to 100
            config.MaxDepth = (Random.Next() % 2147483646) + 1; ;
            // 0 to 1
            config.CVFolds = (Random.Next() % 2);
            // 2 to 50
            config.MaxCategories = (Random.Next() % 49) + 2;
            // 0 to 49
            config.MinSampleCount = (Random.Next() % 50);
            // 0.0f to 0.1f
            config.RegressionAccuracy = (float)(Random.Next() % 100) / 1000f;
            config.TruncatePrunedTree = Random.Next() % 2 == 0;
            config.Use1SERule = Random.Next() % 2 == 0;

            //
            // features
            //

            // create a bool array of which features should be skipped
            config.SkipFeatures = new bool[config.NumberOfFeatures];
            var chunk = Random.Next() % 11;
            // choose one feature to skip
            if ((chunk*3) < config.SkipFeatures.Length)
            {
                config.SkipFeatures[(chunk * 3)] = true;
                config.SkipFeatures[(chunk * 3) + 1] = true;
                config.SkipFeatures[(chunk * 3) + 2] = true;
            }
            // 0 to 5
            config.PrecisionOfRound = Random.Next() % 6;

            return config;
        }

        public double GetFitness(ImageClassifierConfig traits)
        {
            return traits.Fitness;
        }

        public ImageClassifierConfig SetTrait(ImageClassifierConfig src, ImageClassifierConfig dst, int index)
        {
            // take the trait from src and set in dst
            if (index < (NumberOfModelTraits + NumberOfOhterTraits))
            {
                switch (index)
                {
                    case 0: dst.MaxDepth = src.MaxDepth; break;
                    case 1: dst.CVFolds = src.CVFolds; break;
                    case 2: dst.MaxCategories = src.MaxCategories; break;
                    case 3: dst.MinSampleCount = src.MinSampleCount; break;
                    case 4: dst.RegressionAccuracy = src.RegressionAccuracy; break;
                    case 5: dst.TruncatePrunedTree = src.TruncatePrunedTree; break;
                    case 6: dst.Use1SERule = src.Use1SERule; break;
                    case 7: dst.PrecisionOfRound = src.PrecisionOfRound; break;
                    default: throw new Exception($"invalid index : {index}");
                }
            }
            else
            {
                // this is an index into the SkipFeatures array
                var localindex = index - (NumberOfModelTraits + NumberOfOhterTraits);
                if (dst.SkipFeatures == null) dst.SkipFeatures = new bool[NumberOfFeatureTraits];
                if (src.SkipFeatures != null && localindex < src.SkipFeatures.Length && localindex < dst.SkipFeatures.Length)
                {
                    dst.SkipFeatures[localindex] = src.SkipFeatures[localindex];
                }
            }

            return dst;
        }

        #region private
        private List<ModelData> CachedModelData;
        private Random Random;
        private int Seed;

        private const int NumberOfModelTraits = 7;
        private const int NumberOfFeatureTraits = 30;
        private const int NumberOfOhterTraits = 1;
        #endregion
    }
}
