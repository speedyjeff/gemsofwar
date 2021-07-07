using gemsofwar.engine.ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.tests.engine.ai
{
    static class ImageClassifierTest
    {
        public static void Run()
        {
            Train();
        }

        private static void Train()
        {
            // build a prediction engine to predict if binary numbers are a power of 2
            var data = new List<float[]>();
            var labels = new List<float>();

            // build data (add duplicates to help with training split)
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 100; i++)
                {
                    var input = AsArray(i);
                    var log2 = Math.Log2(i);
                    bool ispower2 = Math.Round(log2) == log2;
                    var label = ispower2 ? 1f : 0f;
                    data.Add(input);
                    labels.Add(label);
                }
            }

            // evaluate
            var result = ImageClassifier.Evaluate(data, labels, config: default(ImageClassifierConfig), seed: 1234);

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

            if (result.R2 != 1) throw new Exception($"failed to get a perfect score : {result.R2}");

            // train
            var classifier = ImageClassifier.Train(data, labels);

            // predict all the inputs
            for (int i = 0; i < 100; i++)
            {
                var input = AsArray(i);
                var log2 = Math.Log2(i);
                bool ispower2 = Math.Round(log2) == log2;
                var label = ispower2 ? 1f : 0f;

                var prediction = classifier.Predict(input, out float[] features);

                Console.WriteLine($"{i} : {System.Convert.ToString(i, toBase: 2)} {label},{prediction}");

                prediction = (float)Math.Round(prediction);

                if (label != prediction) throw new Exception($"mismatched predictions: {label} != {prediction}");
            }
        }

        private static float[] AsArray(int i)
        {
            var binary = System.Convert.ToString(i, toBase: 2);
            var input = new float[8];
            var chars = binary.ToCharArray();
            for (int j = 0; j < input.Length && j < chars.Length; j++) input[j] = (float)Char.GetNumericValue(chars[j]);

            return input;
        }
    }
}
