using gemsofwar.engine;
using gemsofwar.engine.ai;
using gemsofwar.window;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.console
{
    class Region
    {
        // pixels to subtract
        public int Top;
        public int Bottom;
        public int Left;
        public int Right;

        public override string ToString()
        {
            return $"{Top},{Bottom},{Left},{Right}";
        }
    }

    class ModelTraining
    {
        public static void Train(string inputdirectory, string outputfilename)
        {
            var modeldata = new List<ModelData>();
            var numberoffeatures = 0;
            foreach (var filename in Directory.GetFiles(inputdirectory, "*.bmp"))
            {
                Console.WriteLine($"Processing {filename}...");

                var image = Image.FromFile(filename);
                var labels = ReadLabels(filename.Replace(".bmp", ".dat"));

                if (labels == null) throw new Exception("must provide labels for training");

                // adjust the image around to get a different mix of pixels
                foreach (var region in new List<Region>()
                    {
                        new Region() { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                        new Region() { Top = 1, Bottom = 1, Left = 1, Right = 1 },
                        new Region() { Top = 5, Bottom = 5, Left = 5, Right = 5 },
                    })
                {
                    // adjust brightness
                    foreach (var brightness in new List<float>() { 1f, 1.1f, 1.2f })
                    {
                        Console.WriteLine($"training with {region.ToString()} and brightness {brightness}");

                        // adjust image
                        var adjusted = Crop(image, region);
                        adjusted = AdjustBrightness(adjusted, brightness, contrast: 1f, gamma: 1f);

                        try
                        {
                            // generate finger prints
                            var gems = Fingerprint.AnalyzeAll(adjusted, null);

                            // apply labels and save for future training
                            foreach (var gem in gems)
                            {
                                // apply label
                                gem.Fingerprint.Label = labels[gem.Row][gem.Col];
                                // store
                                modeldata.Add(
                                    new ModelData()
                                    {
                                        Data = Fingerprint.AsModelInput(gem.Fingerprint),
                                        Label = gem.Fingerprint.Label
                                    });
                                // capture the number of features
                                if (numberoffeatures == 0) numberoffeatures = modeldata[0].Data.Length;
                            }
                        }
                        catch (Exception) 
                        {
                            Console.WriteLine("failed");
                        }
                    }
                }
            }

            // save training data to disk
            ModelData.Write(outputfilename, modeldata);
            var classifierconfig = ImageClassifierConfig.FromDisk($"{outputfilename}.config");
            // set the feature count
            classifierconfig.NumberOfFeatures = numberoffeatures;
            classifierconfig.ToDisk($"{outputfilename}.config");
        }

        public static void Check(string inputdirectory, string filename)
        {
            if (!File.Exists(filename)) throw new Exception($"unable to find : {filename}");
            if (!Directory.Exists(inputdirectory)) throw new Exception($"unable to find : {inputdirectory}");

            // load model and config
            var modeldata = ModelData.Read(filename);
            var modelconfig = ImageClassifierConfig.FromDisk($"{filename}.config");
            var classifier = ImageClassifier.Train(modeldata, modelconfig);

            // check all the files with what data is in the dat file
            foreach (var file in Directory.GetFiles(inputdirectory, "*.bmp"))
            {
                var labels = ReadLabels(file.Replace(".bmp", ".dat"));

                if (labels != null)
                {
                    Console.WriteLine($"Processing '{file}' ...");

                    // load image
                    var image = Image.FromFile(file);

                    // generate finger prints
                    var gems = Fingerprint.AnalyzeAll(image, classifier);

                    // check that they match
                    foreach(var gem in gems)
                    {
                        if (gem.Row < 0 || gem.Row >= labels.Length ||
                            gem.Col < 0 || gem.Col >= labels[gem.Row].Length) throw new Exception("invalid dimension");

                        // check that they match
                        if (gem.Fingerprint.Label != labels[gem.Row][gem.Col])
                        {
                            Console.WriteLine($" {gem.Col},{gem.Row} : in image ({labels[gem.Row][gem.Col]}) and predicted ({gem.Fingerprint.Label})");
                        }
                    }
                }
            }
        }

        #region private
        private static Image Crop(Image img, Region region)
        {
            // translate region into Rectangle
            var bounds = new Rectangle()
            {
                X = region.Left,
                Y = region.Top,
                Width = (int)(img.Width - (region.Left + region.Right)),
                Height = (int)(img.Height - (region.Top + region.Bottom))
            };

            // crop
            using (var dstbmp = new Bitmap(img))
            {
                return dstbmp.Clone(bounds, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }
        }

        private static Image AdjustBrightness(Image img, float brightness = 1f, float contrast = 1f, float gamma = 1f)
        {
            var adjustedBrightness = brightness - 1.0f;
            // create matrix that will brighten and contrast the image
            var ptsArray = new float[][] {
                new float[] {contrast, 0, 0, 0, 0}, // scale red
                new float[] {0, contrast, 0, 0, 0}, // scale green
                new float[] {0, 0, contrast, 0, 0}, // scale blue
                new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
                new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}
            };

            var imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
            var newimg = new Bitmap(img);
            using (var g = Graphics.FromImage(newimg))
            {
                g.DrawImage(img, new Rectangle(0, 0, newimg.Width, newimg.Height), 
                    0, 0, newimg.Width, newimg.Height,
                    GraphicsUnit.Pixel, imageAttributes);
            }
            return newimg;
        }

        private static int[][] ReadLabels(string filename)
        {
            // .dat files contain the labels in an 8x8 grid

            if (!File.Exists(filename)) throw new Exception($"dat file does not exist {filename}");

            var labels = new int[8][];

            var row = 0;
            foreach (var line in File.ReadAllLines(filename))
            {
                if (row >= 8) throw new Exception($"too many rows in {filename}");

                labels[row] = new int[8];
                var col = 0;
                foreach (var c in line.Trim().ToCharArray())
                {
                    if (col >= 8) throw new Exception($"too many cols in {line} in {filename}");
                    labels[row][col] = (int)Char.GetNumericValue(c);

                    if (labels[row][col] < (int)GemId.Empty || labels[row][col] > (int)GemId.G9) throw new Exception($"inavlid label {labels[row][col]} in '{line}'");

                    if (labels[row][col] == 0) throw new Exception("failed to find label");

                    // advance
                    col++;
                }
                if (col != 8) throw new Exception($"not enough cols in {line} in {filename}");

                row++;
            }
            if (row != 8) throw new Exception($"not enough rows in {filename}");

            return labels;
        }
        #endregion
    }
}
