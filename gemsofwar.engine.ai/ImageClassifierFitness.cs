using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.ai
{
    public class ImageClassifierFitness
    {
        public double R2 { get; set; }
        public int Total { get; set; }
        public int SuccessfulPredictions { get; set; }
        public float[] FeaturePercent { get; set; }
        public float TrainingSplitPercent { get; set; }
        public Dictionary<float /*label*/, float> LabelSuccessPercent { get; set; }
    }
}
