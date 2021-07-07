using engine.Common;
using engine.Winforms;
using gemsofwar.engine;
using System;
using System.Windows.Forms;

namespace gemsofwar.window
{
    public partial class GemBoard : UserControl
    {
        public GemBoard()
        {
            InitializeComponent();

            DoubleBuffered = true;
            IsDisplayDebug = false;
            SelectedToolboxRow = -1;
            PreferredGemOrdering = new GemId[]
                {
                    GemId.G1,
                    GemId.G2,
                    GemId.G3,
                    GemId.G4,
                    GemId.G5,
                    GemId.G6,
                    GemId.G7,
                    GemId.G8,
                    GemId.G9,
                    GemId.Empty
                };

            // creat the new board
            Board = new Board(new BoardConfiguration()
            {
                Width = 720,
                Height = 640,
                Rows = RowCount,
                Columns = ColCount + 1, // +1 for toolbox
                EdgeAngle = 0,
                Background = new RGBA() { R = 255, G = 255, B = 255, A = 255 }
            });
            Board.OnCellPaint += Board_OnCellPaint;
            Board.OnCellClicked += Board_OnCellClicked;
            CellType = new Fingerprint[RowCount][];
            for (int i = 0; i < CellType.Length; i++)
            {
                CellType[i] = new Fingerprint[ColCount];
                for (int j = 0; j < CellType[i].Length; j++) CellType[i][j] = null;
            }

            // link to this control
            UI = new UIHookup(this, Board);
        }

        public GameMode GameMode { get; set; }
        public Move SuggestedMove { get; set; }
        public GemId[] PreferredGemOrdering { get; private set; }

        public const int RowCount = 8;
        public const int ColCount = 8;

        public Fingerprint this[int row, int col]
        {
            get
            {
                if (row < 0 || row >= CellType.Length ||
                    col < 0 || col >= CellType[row].Length) throw new Exception("outside of the board dimensions");
                return CellType[row][col];
            }
            set
            {
                if (row < 0 || row >= CellType.Length ||
                    col < 0 || col >= CellType[row].Length) throw new Exception("outside of the board dimensions");
                CellType[row][col] = value;
            }
        }

        #region private
        private Fingerprint[][] CellType;
        private UIHookup UI;
        private Board Board;
        private bool IsDisplayDebug;
        private int SelectedToolboxRow;

        private void Board_OnCellClicked(int row, int col, float x, float y)
        {
            if (row < 0 || row >= CellType.Length ||
                    col < 0) throw new Exception("outside of the board dimensions");

            // toolbox area
            if (col >= CellType[row].Length)
            {
                if (row == 0)
                {
                    // toggle the display mode
                    IsDisplayDebug = !IsDisplayDebug;
                }

                // set selected row
                if (row >= 1 && row <= 2)
                {
                    if (SelectedToolboxRow == row) SelectedToolboxRow = -1;
                    else SelectedToolboxRow = row;
                }

                // cycle the color of the preferred match order
                if (row >= 3 && row <= 7)
                {
                    var ishigh = y > ((Board.Height / Board.Rows) / 2);

                    // choose index - 0,1 2,3 4,5 6,7
                    var index = ((row - 3) * 2) + (ishigh ? 1 : 0);

                    // rotate
                    PreferredGemOrdering[index]++;
                    if ((int)PreferredGemOrdering[index] > (int)engine.GemId.G9) PreferredGemOrdering[index] = GemId.G1;
                }
            }
            else if (CellType[row][col] != null)
            {
                // change the label up or down
                switch (SelectedToolboxRow)
                {
                    case -1:
                        // flip the Label
                        var isup = y > ((Board.Height / Board.Rows) / 2);

                        if (isup)
                        {
                            if (CellType[row][col].Label < (int)engine.GemId.G9) CellType[row][col].Label++;
                        }
                        else
                        {
                            if (CellType[row][col].Label > (int)engine.GemId.Empty) CellType[row][col].Label--;
                        }
                        break;
                    case 1:
                        // apply wolf
                        if (CellType[row][col].Effect == engine.GemEffect.Lycanthropy) CellType[row][col].Effect = engine.GemEffect.None;
                        else CellType[row][col].Effect = engine.GemEffect.Lycanthropy;
                        break;
                    case 2:
                        // apply explode9x9
                        if (CellType[row][col].Effect == engine.GemEffect.Explode9x9) CellType[row][col].Effect = engine.GemEffect.None;
                        else CellType[row][col].Effect = engine.GemEffect.Explode9x9;
                        break;
                }
            }
        }

        private bool Board_OnCellPaint(IGraphics g, int row, int col)
        {
            if (row < 0 || row >= CellType.Length ||
                    col < 0) throw new Exception("outside of the board dimensions");

            // clear
            g.Rectangle(RGBA.White, 0, 0, g.Width, g.Height, fill: true, border: false);

            // check if this is the toolbox column
            if (col >= CellType[row].Length)
            {
                // highlight
                if (row == SelectedToolboxRow)
                {
                    g.Rectangle(new RGBA() { R = 255, G = 255, B = 0, A = 255 }, 0, 0, g.Width, g.Height, fill: true, border: false);
                }

                // toobox
                switch (row)
                {
                    // debug toggle
                    case 0:
                        var text = (IsDisplayDebug) ? "NORM" : "DEBUG";
                        g.Text(RGBA.Black, x: 0, y: g.Width / 3, text, fontsize: 8);
                        break;

                    // special abilities
                    case 1:
                        // wolf
                        g.Text(new RGBA() { R = 255, B = 255, A = 255 }, x: 0, y: g.Width / 3, "WOLF", fontsize: 8);
                        break;
                    case 2:
                        // x5
                        g.Text(RGBA.Black, x: 0, y: g.Width / 3, "x5", fontsize: 8);
                        break;

                    // match preference
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        // choose index - 0,1 2,3 4,5 6,7
                        var index = ((row - 3) * 2);

                        // high
                        var dim = g.Width / 2;
                        g.Text(RGBA.Black, x: g.Width / 10, y:g.Height / 8, $"{index}", fontsize: 8);
                        g.Ellipse(GemIdToColor(PreferredGemOrdering[index]), x: g.Width / 2, y:0, width:dim, height:dim, fill: true, border: true);

                        // low
                        g.Text(RGBA.Black, x: g.Width / 10, y: (5*g.Height) / 8, $"{index+1}", fontsize: 8);
                        g.Ellipse(GemIdToColor(PreferredGemOrdering[index+1]), x: g.Width / 2, y:g.Height/2, width: dim, height: dim, fill: true, border: true);
                        break;
                }

                return true;
            }

            // draw bounding box
            g.Rectangle(RGBA.Black, x: 0, y: 0, g.Width, g.Height, fill: false);

            // primary cell
            if (CellType[row][col] == null) return true;

            if (IsDisplayDebug && CellType[row][col].Points.Count > 0)
            {
                // display 10 points
                var index = 0;
                for (int y = 0; y < g.Height; y += (g.Height / 4))
                {
                    for (int x = 0; x < g.Width; x += (g.Width / 4))
                    {
                        if (index >= CellType[row][col].Points.Count) break;
                        g.Ellipse(CellType[row][col].Points[index], x, y, g.Width / 4, g.Height / 4);
                        index++;
                    }
                }
            }

            var color = GemIdToColor((gemsofwar.engine.GemId)CellType[row][col].Label);

            // display
            var label = (gemsofwar.engine.GemId)CellType[row][col].Label;
            if (GameMode == GameMode.Battle && label == GemId.G7)
            {
                // make an X (for skull)
                g.Line(color, x1: 0, y1: 0, x2: g.Width, y2: g.Height, thickness: 5f);
                g.Line(color, x1: g.Width, y1: 0, x2: 0, y2: g.Height, thickness: 5f);
            }
            else if (GameMode == GameMode.Treasure &&
                (label == GemId.G5 ||
                label == GemId.G6 ||
                label == GemId.G7 ||
                label == GemId.G8))
            {
                // make a rectangle for a chest/safe
                if (IsDisplayDebug) g.Rectangle(color, x: (3 * g.Width) / 4, y: (3 * g.Height) / 4, g.Width / 4, g.Height / 4);
                else g.Rectangle(color, x: 0, y: 0, g.Width, g.Height);
            }
            else
            {
                if (IsDisplayDebug) g.Ellipse(color, x: (3 * g.Width) / 4, y: (3 * g.Height) / 4, g.Width / 4, g.Height / 4);
                else g.Ellipse(color, x: 0, y: 0, g.Width, g.Height);
            }

            // add effects
            if (CellType[row][col].Effect == engine.GemEffect.Lycanthropy)
            {
                g.Text(RGBA.Black, x: 0, y: g.Width / 3, "WOLF", fontsize: 8);
            }
            else if (CellType[row][col].Effect == engine.GemEffect.Explode9x9)
            {
                g.Text(RGBA.Black, x: 0, y: g.Width / 3, "X5", fontsize: 8);
            }

            // check if this is a suggested move
            if (SuggestedMove.IsValid)
            {
                if (SuggestedMove.Row == row && SuggestedMove.Col == col)
                {
                    // add the direction indicator
                    g.Rectangle(RGBA.White, x: g.Width / 4, y: g.Height / 4, width: g.Width / 2, height: g.Height / 2, fill: true, border: true);
                    switch (SuggestedMove.Direction)
                    {
                        case Direction.Up:
                            g.Triangle(RGBA.White, x1: 0, y1: g.Height / 2, x2: g.Width / 2, y2: 0, x3: g.Width, y3: g.Height / 2, fill: true, border: true);
                            break;
                        case Direction.Left:
                            g.Triangle(RGBA.White, x1: g.Width/2, y1: 0, x2: 0, y2: g.Height/2, x3: g.Width/2, y3: g.Height, fill: true, border: true);
                            break;
                        case Direction.Right:
                            g.Triangle(RGBA.White, x1: g.Width / 2, y1: 0, x2: g.Width, y2: g.Height/2, x3: g.Width / 2, y3: g.Height, fill: true, border: true);
                            break;
                        case Direction.Down:
                            g.Triangle(RGBA.White, x1: 0, y1: g.Height / 2, x2: g.Width / 2, y2: g.Height, x3: g.Width, y3: g.Height / 2, fill: true, border: true);
                            break;
                    }
                }
            }

            return true;
        }

        private RGBA GemIdToColor(GemId id)
        {
            var color = RGBA.White;
            switch (id)
            {
                case gemsofwar.engine.GemId.Empty:
                    color = RGBA.Black;
                    break;
                case gemsofwar.engine.GemId.G1:
                    // red | bronze
                    color = GameMode == GameMode.Battle ? new RGBA() { R = 208, G = 0, B = 0, A = 255 } : new RGBA() { R = 185, G = 92, B = 0, A = 255 };
                    break;
                case gemsofwar.engine.GemId.G2:
                    // brown | silver
                    color = GameMode == GameMode.Battle ? new RGBA() { R = 119, G = 84, B = 72, A = 255 } : new RGBA() { R = 192, G = 192, B = 192, A = 255 };
                    break;
                case gemsofwar.engine.GemId.G3:
                    // green | gold
                    color = GameMode == GameMode.Battle ? new RGBA() { R = 30, G = 179, B = 60, A = 255 } : new RGBA() { R = 213, G = 198, B = 17, A = 255 };
                    break;
                case gemsofwar.engine.GemId.G4:
                    // blue | bag
                    color = GameMode == GameMode.Battle ? new RGBA() { R = 14, G = 14, B = 194, A = 255 } : new RGBA() { R = 120, G = 82, B = 71, A = 255 };
                    break;
                case gemsofwar.engine.GemId.G5:
                    // purple | chest
                    color = GameMode == GameMode.Battle ? new RGBA() { R = 121, G = 35, B = 173, A = 255 } : new RGBA() { R = 20, G = 20, B = 20, A = 255 };
                    break;
                case gemsofwar.engine.GemId.G6:
                    // yellow | green chest
                    color = GameMode == GameMode.Battle ? new RGBA() { R = 237, G = 237, B = 14, A = 255 } : new RGBA() { R = 40, G = 108, B = 46, A = 255 };
                    break;
                case gemsofwar.engine.GemId.G7:
                    // skull | red check
                    color = GameMode == GameMode.Battle ? new RGBA() { R = 128, G = 128, B = 128, A = 255 } : new RGBA() { R = 185, G = 0, B = 0, A = 255 };
                    break;
                case gemsofwar.engine.GemId.G8:
                    // barrier | safe
                    color = GameMode == GameMode.Battle ? new RGBA() { R = 128, G = 128, B = 128, A = 255 } : new RGBA() { R = 104, G = 0, B = 208, A = 255 };
                    break;
                case gemsofwar.engine.GemId.G9:
                    // n/a
                    color = RGBA.Black;
                    break;
                default: throw new Exception("unknown cell type");
            }

            return color;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            UI.ProcessCmdKey(keyData);
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion
    }
}
