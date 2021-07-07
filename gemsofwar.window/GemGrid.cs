using gemsofwar.engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gemsofwar.window
{
    public partial class GemGrid : UserControl
    {
        public GemGrid()
        {
            InitializeComponent();

            // setup
            LessButton.Click += LessButton_Click;
            MoreButton.Click += MoreButton_Click;
            Gem1ComboBox.SelectedIndexChanged += (sender, e) => { GemComboBox_SelectedIndexChanged(0, sender, e); };
            Gem2ComboBox.SelectedIndexChanged += (sender, e) => { GemComboBox_SelectedIndexChanged(1, sender, e); };
            Gem3ComboBox.SelectedIndexChanged += (sender, e) => { GemComboBox_SelectedIndexChanged(2, sender, e); };
            Gem4ComboBox.SelectedIndexChanged += (sender, e) => { GemComboBox_SelectedIndexChanged(3, sender, e); };
            Gem5ComboBox.SelectedIndexChanged += (sender, e) => { GemComboBox_SelectedIndexChanged(4, sender, e); };
            Gem6ComboBox.SelectedIndexChanged += (sender, e) => { GemComboBox_SelectedIndexChanged(5, sender, e); };
        }

        public List<ImageDetails> Gems { get; private set; }
        public List<string> Labels { get; private set; }

        public void Refresh(List<ImageDetails> gems, List<string> labels)
        {
            if (gems == null || labels == null ||
                labels.Count < 1) throw new Exception("must provide valid content");

            // setup
            FromIndex = 0;
            ToIndex = NumberSlots;
            LessButton.Visible = false;
            MoreButton.Visible = gems.Count > ToIndex;

            // show
            lock (this)
            {
                Labels = labels;
                Gems = gems;
                ShowNext();
            }
        }

        #region private
        private int FromIndex;
        private int ToIndex;

        private const int NumberSlots = 6;

        private void MoreButton_Click(object sender, EventArgs e)
        {
            lock (this)
            {
                // ensure we have not gone too far
                if ((FromIndex + NumberSlots) > Gems.Count) throw new Exception("invalid window");

                // increment
                FromIndex += NumberSlots;
                ToIndex += NumberSlots;

                // show more/less
                LessButton.Visible = true;
                MoreButton.Visible = Gems.Count > ToIndex;

                // update grid
                ShowNext();
            }
        }

        private void LessButton_Click(object sender, EventArgs e)
        {
            lock (this)
            {
                // ensure we have not gone too far
                if ((FromIndex - NumberSlots) < 0) throw new Exception("invalid window");

                // increment
                FromIndex -= NumberSlots;
                ToIndex -= NumberSlots;

                // show more/less
                LessButton.Visible = FromIndex > 0;
                MoreButton.Visible = Gems.Count > ToIndex;

                // update grid
                ShowNext();
            }
        }

        private void GemComboBox_SelectedIndexChanged(int comboindex, object sender, EventArgs e)
        {
            var box = GetComboBox(comboindex);
            var index = FromIndex + comboindex;

            lock (this)
            {
                // change the selection for this item
                if (index >= 0 && index < Gems.Count)
                {
                    Gems[index].Fingerprint.Label = box.SelectedIndex;
                    Gems[index].IsDirty = true;
                }
            }
        }

        private void ShowNext()
        {
            // must hold lock on Gems

            // display the gems in the list
            for (int i = FromIndex; i < ToIndex; i++)
            {
                // image
                if (i >= Gems.Count) SetPictureBox(i, null);
                else SetPictureBox(i, Gems[i].Image);

                // drop down
                if (i >= Gems.Count) SetComboBox(i, label: 0);
                else SetComboBox(i, Gems[i].Fingerprint.Label);
            }
        }

        private void SetPictureBox(int i, Image img)
        {
            PictureBox pict = null;
            // choose the right picturebox
            switch (i % 6)
            {
                case 0: pict = Gem1PictureBox; break;
                case 1: pict = Gem2PictureBox; break;
                case 2: pict = Gem3PictureBox; break;
                case 3: pict = Gem4PictureBox; break;
                case 4: pict = Gem5PictureBox; break;
                case 5: pict = Gem6PictureBox; break;
                default: throw new Exception("invalid PictureBox id");
            }
            // set the image
            pict.Image = img;
        }

        private ComboBox GetComboBox(int i)
        {
            // choose the right combobox
            switch (i % 6)
            {
                case 0: return Gem1ComboBox;
                case 1: return Gem2ComboBox; 
                case 2: return Gem3ComboBox; 
                case 3: return Gem4ComboBox; 
                case 4: return Gem5ComboBox; 
                case 5: return Gem6ComboBox; 
                default: throw new Exception("invalid PictureBox id");
            }
        }

        private void SetComboBox(int i, int label)
        {
            var box = GetComboBox(i);
            box.BeginUpdate();
            {
                // configure the box
                box.Items.Clear();
                // add all the choices
                box.Items.AddRange(Labels.ToArray());
                // select the appropriate one
                box.SelectedIndex = label;
                box.SelectedItem = box.Text = Labels[label];
            }
            box.EndUpdate();
        }
        #endregion
    }
}
