using gemsofwar.engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using gemsofwar.engine.ai;
using System.Text;

namespace gemsofwar.window
{
    public partial class MainPage : Form
    {
        public MainPage()
        {
            InitializeComponent();

            // setup
            CaptureButton.Click += CaptureButton_Click;
            TrainButton.Click += TrainButton_Click;
            GuessButton.Click += GuessButton_Click;
            DemoButton.Click += DemoButton_Click;
            DemoPathButton.Click += DemoPathButton_Click;
            GemModeComboBox.SelectedIndex = (int)GameMode.Battle;
            Factory = new BattleFactory(seed: 0);
            GemModeComboBox.SelectedIndexChanged += GemModeComboBox_SelectedIndexChanged;
            GemBoardControl.GameMode = (GameMode)GemModeComboBox.SelectedIndex;

            // read trained data from disk
            ClassifierConfig = ImageClassifierConfig.FromDisk($"{ClassifierFilenames[(int)GemBoardControl.GameMode]}.config");
            TrainedModelData = ModelData.Read(ClassifierFilenames[(int)GemBoardControl.GameMode]);
            Classifier = ImageClassifier.Train(TrainedModelData, ClassifierConfig);
        }

        #region private
        private IGemFactory Factory;
        private ImageClassifier Classifier;
        private static string[] ClassifierFilenames = new string[] { "battle.json", "treasure.json" };
        private List<ModelData> TrainedModelData;
        private ImageClassifierConfig ClassifierConfig;

        private void GuessButton_Click(object sender, EventArgs e)
        {
            // capture state of board
            var gems = new Gem[8][];
            foreach (var gem in GemGridControl.Gems)
            {
                if (gems[gem.Row] == null) gems[gem.Row] = new Gem[8];
                gems[gem.Row][gem.Col] = new Gem()
                {
                    CanMatch = true,
                    Effect = gem.Fingerprint.Effect,
                    Id = (GemId)gem.Fingerprint.Label
                };
            }

            // capture the board state and make a guess
            var gemboard = new gemsofwar.engine.GemsBoard(
                Factory, 
                rules: GemBoardControl.GameMode == GameMode.Battle ? Rules.RemoveGemOnMatch : Rules.UpgradeGemOnMatch, 
                gems);

            // get predictions and strategy
            var strategies = Strategy.Analyze(
                gemboard, 
                depth: 2, 
                gemIdOrder: GemBoardControl.PreferredGemOrdering);

            if (strategies.Count == 0) throw new Exception("failed to identify a strategy");

            // mark best move on the board
            GemBoardControl.SuggestedMove = strategies[0].Move;

            // give information about moves impact
            var lines = new List<string>();
            var gemmap = Factory.GetMapping();
            for(int i=0; i<strategies.Count; i++)
            {
                lines.Add($"{i}: ({strategies[i].Move.Row},{strategies[i].Move.Col}) {strategies[i].Move.Direction}, Largest = {strategies[i].LargestGemCount}, Creates = {strategies[i].CreatesLargestGemCount}");
                foreach(var kvp in strategies[i].GemCounts)
                {
                    var found = false;
                    foreach(var g in gemmap)
                    {
                        if (kvp.Key == g.GemId)
                        {
                            lines.Add($"  {g.Name} : {kvp.Value}");
                            found = true;
                            break;
                        }
                    }
                    if (!found) throw new Exception($"unable to find gem : {kvp.Key}");
                }
            }
            InfoPanelControl.SetText(lines.ToArray());
        }

        private void DemoPathButton_Click(object sender, EventArgs e)
        {
            CameraControl.UseStaticImage = true;
            CameraControl.RotateStaticImage(resetPath: true);

            CaptureButton_Click(sender: null, e);
        }

        private void DemoButton_Click(object sender, EventArgs e)
        {
            CameraControl.UseStaticImage = true;
            CameraControl.RotateStaticImage();

            CaptureButton_Click(sender: null, e);
        }

        private void TrainButton_Click(object sender, EventArgs e)
        {
            var dat = new int[8][];

            // train the model
            try
            {
                // disable the input buttons (temporarily)
                TrainButton.Enabled = false;
                CaptureButton.Enabled = false;

                // look through the contents of GemGrid for items that have changed
                foreach (var gem in GemGridControl.Gems)
                {
                    // capture new data for training
                    if (gem.IsDirty)
                    {
                        TrainedModelData.Add(
                            new ModelData()
                            {
                                Data = Fingerprint.AsModelInput(gem.Fingerprint),
                                Label = gem.Fingerprint.Label
                            });
                    }

                    // capture data for dat (the Label)
                    if (dat[gem.Row] == null) dat[gem.Row] = new int[8];
                    dat[gem.Row][gem.Col] = gem.Fingerprint.Label;
                }

                lock (this)
                {
                    // train the model
                    Classifier = ImageClassifier.Train(TrainedModelData, ClassifierConfig);

                    // write data to disk
                    ModelData.Write(ClassifierFilenames[(int)GemBoardControl.GameMode], TrainedModelData);
                }
            }
            finally
            {
                TrainButton.Enabled = true;
                CaptureButton.Enabled = true;
            }

            if (!CameraControl.UseStaticImage)
            {
                // save the image
                var img = CameraControl.CroppedImage;
                var index = 0;
                while (File.Exists($"image.{index}.bmp")) index++;
                img.Save($"image.{index}.bmp");

                // save the dat file with labls
                var sb = new StringBuilder();
                for (int r = 0; r < dat.Length; r++)
                {
                    for (int c = 0; c < dat[r].Length; c++)
                    {
                        sb.Append(dat[r][c]);
                    }
                    sb.Append(Environment.NewLine);
                }
                File.WriteAllText($"image.{index}.dat", sb.ToString());
            }
        }

        private void GemModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            lock(this)
            {
                // create the classifier
                GemBoardControl.GameMode = (GameMode)GemModeComboBox.SelectedIndex;
                Factory = GemBoardControl.GameMode == GameMode.Battle ? new BattleFactory(seed: 0) : new TreasureFactory(seed: 0);
                TrainedModelData = ModelData.Read(ClassifierFilenames[(int)GemBoardControl.GameMode]);
                Classifier = ImageClassifier.Train(TrainedModelData, ClassifierConfig);

                // clear the grid and board
                GemGridControl.Refresh(gems: new List<ImageDetails>(), labels: new List<string>() { "unknown" });
            }
        }

        private void CaptureButton_Click(object sender, EventArgs e)
        {
            // flip to the live camera when clicked
            if (sender != null) CameraControl.UseStaticImage = false;

            // get the latest cropped snapshot
            var img = CameraControl.CroppedImage;

            if (img == null)
            {
                InfoPanelControl.SetText(new string[] { "... no image, try again ... " });
                return;
            }

            // set text that we are processing the image has started
            InfoPanelControl.SetText(new string[] { "... processing ... " });

            // analyze the image and determine the gem color
            var gems = Fingerprint.AnalyzeAll(img, Classifier);

            lock (this)
            {
                // clear suggested move
                GemBoardControl.SuggestedMove = default(Move);

                // apply all these to the GemBoard
                foreach (var gem in gems)
                {
                    GemBoardControl[gem.Row, gem.Col] = gem.Fingerprint;
                }

                // display the gems in the gem grid
                var labels = Factory.GetMapping().Select(m => m.Name).ToList();
                labels.Insert(Fingerprint.UnknownLabel, "unknown");
                GemGridControl.Refresh(gems, labels);
            }

            // make a guess
            GuessButton_Click(sender, e);
        }
        #endregion
    }
}
