using engine.Common;
using gemsofwar.engine;
using gemsofwar.engine.ai;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace gemsofwar.window
{
    public class Fingerprint
    {
        public int Label { get; set; }
        public List<RGBA> Points { get; set; }
        public GemEffect Effect { get; set; }

        public const int UnknownLabel = 0;

        public static bool DebugInfo = false;

        public static Fingerprint AnalyzeOne(Image img)
        {
            // todo ensure that img size makes sense?
            // todo try to center image?
            // todo shift the pixels to push the middles first?

            // based on the image, create a finger print
            var points = new List<RGBA>();
            var mostcommoncolor = RGBA.White;
            using (var bmp = new Bitmap(img))
            {
                // gather a list of points at wel known locations
                // ----------------------
                // |bbbbbbbbbbbbbbbbb|
                // |b       x       b|
                // |b       x       b|
                // |b x x x x x x x b|
                // |b       x       b|
                // |b       x       b|
                // |bbbbbbbbbbbbbbbbb|
                // ----------------------

                // check the ring of black
                var left = 0;
                var top = 0;
                var bottom = 0;
                var right = 0;
                for (left = 0; left < bmp.Width / 2; left++) if (!IsBlack(bmp, x: left, y: bmp.Height / 2)) break;
                for (right = bmp.Width-1; right > bmp.Width / 2; right--) if (!IsBlack(bmp, x: right, y: bmp.Height / 2)) break;
                for (top = 0; top < bmp.Height / 2; top++) if (!IsBlack(bmp, x: bmp.Width/2, y: top)) break;
                for (bottom = bmp.Height-1; bottom > bmp.Height / 2; bottom--) if (!IsBlack(bmp, x: bmp.Width / 2, y: bottom)) break;

                if (left == 0 || right == 0 ||
                    top == 0 || bottom == 0) throw new Exception("failed to find border");

                // get step function for width and height
                var numpoints = 10;
                var wstep = (right - left) / (numpoints / 2);
                var hstep = (bottom - top) / (numpoints / 2);
                var x1 = left + (wstep / 2);
                var x2 = bmp.Width / 2;

                // grab the pixels
                var y1 = bmp.Height / 2; 
                var y2 = top + (hstep / 2);
                while (x1 < right && y2 < bottom)
                {
                    // left to right
                    var pixel = bmp.GetPixel(x1, y1);
                    points.Add(new RGBA() { R = pixel.R, G = pixel.G, B = pixel.B, A = 255 });

                    // top to bottom
                    pixel = bmp.GetPixel(x2, y2);
                    points.Add(new RGBA() { R = pixel.R, G = pixel.G, B = pixel.B, A = 255 });

                    if (points.Count == numpoints) break;

                    // increment
                    x1 += wstep;
                    y2 += hstep;
                }

                if (points.Count != numpoints) throw new Exception($"different number of points : {points.Count}");
            }

            return new Fingerprint()
            {
                Points = points,
                Effect = GemEffect.None
            };
        }

        public static List<ImageDetails> AnalyzeAll(Image img, ImageClassifier classifier)
        {
            var width = img.Width / 8;
            var height = img.Height / 8;

            // split the image into an 8x8
            using (var g = Graphics.FromImage(img))
            {
                // add grids
                for (int i = 0; i < 8; i++)
                {
                    // columns
                    g.DrawLine(BlackPen, x1: (i * width), y1: 0, x2: (i * width), y2: img.Height);
                    // rows
                    g.DrawLine(BlackPen, x1: 0, y1: i * height, x2: img.Width, y2: i * height);
                }
            }

            // cells
            var gems = new List<ImageDetails>();
            var bounds = new Rectangle() { Width = width, Height = height };
            for (var row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    bounds.X = (col * width);
                    bounds.Y = (row * height);
                    using (var dstbmp = new Bitmap(img))
                    {
                        var gemimg = dstbmp.Clone(bounds, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                        if (DebugInfo) gemimg.Save($@"images\{row}.{col}.bmp");

                        var fp = AnalyzeOne(gemimg);

                        if (DebugInfo) System.Diagnostics.Debug.WriteLine($"{col},{row} {fp.Points.Count}");

                        // associate with known labels (from config)
                        fp.Label = UnknownLabel;

                        // predict
                        if (classifier != null)
                        {
                            var data = AsModelInput(fp);
                            var score = classifier.Predict(data, out float[] features);
                            fp.Label = (int)Math.Round(score);

                            if (DebugInfo)
                            {
                                System.Diagnostics.Debug.Write($"{score} {fp.Label}");
                                for (int i = 0; i < features.Length; i++) System.Diagnostics.Debug.Write($" {features[i]}");
                                System.Diagnostics.Debug.WriteLine("");
                            }
                        }

                        // list all gems for labeling
                        var detail = new ImageDetails()
                        {
                            Image = gemimg,
                            Fingerprint = fp,
                            Row = row,
                            Col = col
                        };
                        // if the label is not known, sort to the front
                        if (fp.Label == UnknownLabel) gems.Insert(0, detail);
                        else gems.Add(detail);
                    }
                }
            }

            return gems;
        }

        public static float[] AsModelInput(Fingerprint fp)
        {
            var points = new List<float>();

            for (int i = 0; i < fp.Points.Count; i++)
            {
                points.Add(fp.Points[i].R);
                points.Add(fp.Points[i].G);
                points.Add(fp.Points[i].B);
            }

            return points.ToArray();
        }

        #region private
        private static Pen BlackPen = new Pen(new SolidBrush(Color.Black), width: 20f);

        private static bool IsBlack(Bitmap bmp, int x, int y)
        {
            var pixel = bmp.GetPixel(x, y);
            return (pixel.R == Color.Black.R &&
                pixel.G == Color.Black.G &&
                pixel.B == Color.Black.B);
        }
        #endregion
    }
}
