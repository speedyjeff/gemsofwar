using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using OpenCvSharp;
using OpenCvSharp.ML;

namespace gemsofwar.engine.ai
{
    public class ImageClassifier
    {
        public static ImageClassifier Train(List<ModelData> modeldata, ImageClassifierConfig config = default(ImageClassifierConfig))
        {
            // split the List<ModelData> into List<float[]> and float[]
            var data = new List<float[]>();
            var labels = new List<float>();

            foreach (var md in modeldata)
            {
                data.Add(md.Data);
                labels.Add(md.Label);
            }

            return Train(data, labels, config);
        }

        public static ImageClassifier Train(List<float[]> input, List<float> labels, ImageClassifierConfig config = default(ImageClassifierConfig))
        {
            if (input == null || input.Count < 1 ||
                labels == null) return null;

            // determine the length based on the skips
            var length = input[0].Length;
            for (int i = 0; i < input[0].Length; i++)
            {
                // removed unused features
                if (config.IsValid &&
                    config.SkipFeatures != null &&
                    i <= config.SkipFeatures.Length &&
                    config.SkipFeatures[i])
                {
                    length--;
                }
            }

            // normalize data
            var data = new float[input.Count, length];
            for (int i = 0; i < input.Count; i++)
            {
                // normalize data
                var index = 0;
                for (int j = 0; j < input[i].Length; j++)
                {
                    // normaize data
                    if (config.IsValid &&
                        config.SkipFeatures != null &&
                        j <= config.SkipFeatures.Length &&
                        config.SkipFeatures[j])
                    {
                        // nothing
                    }
                    else
                    {
                        data[i, index++] = Normalize(input[i][j], min: 1f, max: 255f, config.IsValid ? config.PrecisionOfRound : 2);
                    }
                }
            }

            // create input
            using (var lableInput = InputArray.Create<float>(labels))
            {
                using (var dataInput = InputArray.Create<float>(data))
                {
                    // create and train
                    var imgclass = new ImageClassifier()
                    {
                        Config = config
                    };
                    
                    imgclass.Model = OpenCvSharp.ML.DTrees.Create();
                    imgclass.Model.MaxDepth = (config.IsValid) ? config.MaxDepth : 2147483647; // 2147483647
                    imgclass.Model.CVFolds = (config.IsValid) ? config.CVFolds : 1; // 0
                    imgclass.Model.MaxCategories = (config.IsValid) ? config.MaxCategories : 10; // 10
                    imgclass.Model.MinSampleCount = (config.IsValid) ? config.MinSampleCount : 10; // 10
                    imgclass.Model.RegressionAccuracy = (config.IsValid) ? config.RegressionAccuracy : 0.01f; // 0.01
                    imgclass.Model.TruncatePrunedTree = (config.IsValid) ? config.TruncatePrunedTree : true; // true
                    imgclass.Model.Use1SERule = (config.IsValid) ? config.Use1SERule : true; // true
                    imgclass.Model.UseSurrogates = false; // false, true nyi
                    imgclass.Model.Train(dataInput, SampleTypes.RowSample, lableInput);
                    
                    return imgclass;
                }
            }
        }

        public float Predict(float[] data, out float[] features)
        {
            if (Model == null) throw new Exception("Must successfully load a trained model first");

            // determine length based on skips
            var length = data.Length;
            for (int i = 0; i < data.Length; i++)
            {
                if (Config.IsValid &&
                    Config.SkipFeatures != null &&
                    i <= Config.SkipFeatures.Length &&
                    Config.SkipFeatures[i])
                {
                    length--;
                }
            }
            var pdata = new float[length];

            // normalize the data
            var index = 0;
            for (int i = 0; i < data.Length; i++)
            {
                // normalize data
                if (Config.IsValid &&
                    Config.SkipFeatures != null &&
                    i <= Config.SkipFeatures.Length &&
                    Config.SkipFeatures[i])
                {
                    // nothing
                }
                else
                {
                    pdata[index++] = Normalize(data[i], min: 1f, max: 255f, Config.IsValid ? Config.PrecisionOfRound : 2);
                }
            }

            // predict
            using (var input = InputArray.Create<float>(pdata))
            {
                using (var mat = new Mat(pdata.Length, 1, MatType.CV_32F))
                {
                    using (var output = OutputArray.Create(mat))
                    {
                        var prediction = Model.Predict(input, output);
                        mat.GetArray(out features);
                        return prediction;
                    }
                }
            }
        }

        public static ImageClassifierFitness Evaluate(List<ModelData> modeldata, ImageClassifierConfig config = default(ImageClassifierConfig), int seed = 0)
        {
            // split the List<ModelData> into List<float[]> and float[]
            var data = new List<float[]>();
            var labels = new List<float>();

            foreach(var md in modeldata)
            {
                data.Add(md.Data);
                labels.Add(md.Label);
            }

            return Evaluate(data, labels, config, seed);
        }

        public static ImageClassifierFitness Evaluate(List<float[]> data, List<float> labels, ImageClassifierConfig config = default(ImageClassifierConfig), int seed = 0)
        {
            if (data.Count != labels.Count) throw new Exception("length does not match for data and labels");
            if (data.Count == 0) throw new Exception("must pass in a non-zero list of data");

            var result = new ImageClassifierFitness() { LabelSuccessPercent = new Dictionary<float, float>() };

            // split the data set into two distributions
            //  1) training and 2) validation
            // then generate predictions for the full set and compare against expected

            // split the data
            var traindata = new List<float[]>();
            var trainlabels = new List<float>();
            var rand = (seed == 0) ? new Random() : new Random(seed);
            for(int i=0; i<data.Count; i++)
            {
                // 80/20 split
                if (rand.Next() % 10 > 1)
                {
                    // train
                    traindata.Add(data[i]);
                    trainlabels.Add(labels[i]);
                }
            }

            // check the the splits are about right
            result.TrainingSplitPercent = (100f * (float)traindata.Count) / (float)data.Count;

            if (result.TrainingSplitPercent < 60f || result.TrainingSplitPercent > 90f) throw new Exception("bad train/val split");

            // train the model
            var model = Train(traindata, trainlabels, config);

            // generate the fitness using R and basic stats

            // R = (Count (sum of ab) - (sum of a)(sum of b)) / [sqrt((Count(sum a^2) - (sum of a)^2)(Count *(sum of b^2) - (sum of b)^2)]
            //  a == labels
            //  b == prediction
            var ab = 0d;
            var a = 0d;
            var b = 0d;
            var a2 = 0d;
            var b2 = 0d;
            var countbylabel = new Dictionary<float, int>();
            for (int i = 0; i < data.Count; i++)
            {
                var pred = model.Predict(data[i], out float[] features);
                var predlabel = (float)Math.Round(pred);

                // track the most useful feature
                if (result.FeaturePercent == null) result.FeaturePercent = new float[features.Length];
                var max = 0f;
                var index = 0;
                for(int j=0; j<features.Length;j++) if (features[j] > max) { max = features[j]; index = j; }
                result.FeaturePercent[index]++;

                // basic stats
                result.Total++;
                result.SuccessfulPredictions += (predlabel == labels[i]) ? 1 : 0;
                if (!result.LabelSuccessPercent.ContainsKey(labels[i]))
                {
                    result.LabelSuccessPercent.Add(labels[i], 0);
                    countbylabel.Add(labels[i], 0);
                }
                result.LabelSuccessPercent[labels[i]] += (predlabel == labels[i]) ? 1 : 0;
                countbylabel[labels[i]]++;

                // r calculation
                a += labels[i];
                b += predlabel;
                ab += (labels[i] * predlabel);
                a2 += Math.Pow(labels[i], 2);
                b2 += Math.Pow(predlabel, 2);
            }

            // compute percentages
            foreach(var label in result.LabelSuccessPercent.Keys)
            {
                result.LabelSuccessPercent[label] = result.LabelSuccessPercent[label] / (float)countbylabel[label];
            }
            for(int i=0; i<result.FeaturePercent.Length;i++)
            {
                result.FeaturePercent[i] /= (float)result.Total;
            }

            // complete R
            var r2 = ((data.Count * ab) - (a * b)) / Math.Sqrt(((data.Count * a2) - Math.Pow(a, 2)) * (data.Count * b2 - Math.Pow(b, 2)));
            result.R2 = Math.Pow(r2, 2);

            return result;
        }

        public ImageClassifierConfig Config { get; private set; }

        #region private
        private DTrees Model = null;

        private static float Normalize(float value, float min, float max, int precision)
        {
            if (value > max) throw new Exception("invalid data input format");
            if (value > min) return (float)Math.Round(value / max, precision);
            else return value;
        }
        #endregion

    }
}
