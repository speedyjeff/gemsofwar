using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.ai
{
    public class ModelData
    {
        public float[] Data { get; set; }
        public float Label { get; set; }

        public static List<ModelData> Read(string path)
        {
            if (!File.Exists(path)) return new List<ModelData>();

            // read in the json content
            var json = File.ReadAllText(path);
            return System.Text.Json.JsonSerializer.Deserialize<List<ModelData>>(json);
        }

        public static void Write(string path, List<ModelData> fingerprints)
        {
            // write out the json content
            var json = System.Text.Json.JsonSerializer.Serialize(fingerprints);
            File.WriteAllText(path, json);
        }
    }
}
