using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.ai
{
    public struct ImageClassifierConfig
    {
        // model config
        public int MaxDepth { get; set; }
        public int CVFolds { get; set; }
        public int MaxCategories { get; set; }
        public int MinSampleCount { get; set; }
        public float RegressionAccuracy { get; set; }
        public bool TruncatePrunedTree { get; set; }
        public bool Use1SERule { get; set; }

        // feature config
        public int PrecisionOfRound { get; set; }
        public int NumberOfFeatures { get; set; }
        public bool[] SkipFeatures { get; set; }

        public double Fitness { get; set; }

        public bool IsValid { get { return MaxDepth != 0; } }

        public void ToDisk(string filename)
        {
            var json = System.Text.Json.JsonSerializer.Serialize<ImageClassifierConfig>(this);
            File.WriteAllText(filename, json);
        }

        public static ImageClassifierConfig FromDisk(string filename)
        {
            // if it does not exist, then return the defaults
            if (!File.Exists(filename)) return new ImageClassifierConfig()
            {
                MaxDepth = 2147483647,
                CVFolds = 1,
                MaxCategories = 10,
                MinSampleCount = 10,
                RegressionAccuracy = 0.01f,
                TruncatePrunedTree = true,
                Use1SERule = true,
                SkipFeatures = null,
                PrecisionOfRound = 2
            };
            var json = File.ReadAllText(filename);
            return System.Text.Json.JsonSerializer.Deserialize<ImageClassifierConfig>(json);
        }

        public override string ToString()
        {
            var skips = new StringBuilder();
            for (int i = 0; SkipFeatures != null && i < SkipFeatures.Length; i++) skips.Append(SkipFeatures[i] ? '1' : '0');
            return $"MaxDepth:{MaxDepth} CVFolds:{CVFolds} MaxCategories:{MaxCategories} MinSampleCount:{MinSampleCount} RegressionAccuracy:{RegressionAccuracy}f TrucatedPrunedTree:{TruncatePrunedTree} Use1SRule:{Use1SERule} PrecisionOfRound:{PrecisionOfRound} SkipFeatures:{skips.ToString()}";
        }
    }
}
