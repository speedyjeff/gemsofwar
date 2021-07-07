using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace gemsofwar.window
{
    public partial class Camera : UserControl
    {
        public Camera()
        {
            InitializeComponent();

            // initialize camera
            Device = new VideoCapture((int)DeviceId.Back);

            if (!Device.IsOpened())
            {
                // try opening the front facing
                Device = new VideoCapture((int)DeviceId.Front);
            }

            if (!Device.IsOpened())
            {
                MessageBox.Show("Failed to open camera :(");
                return;
            }

            // setup
            BackColor = Color.White;
            Device.FrameWidth = 1920;
            Device.FrameHeight = 1080;
            CameraLock = 0;
            RegionCorner1 = new System.Drawing.Point() { X = 0, Y = 0 };
            RegionCorner2 = new System.Drawing.Point() { X = Width-1, Y = Height-1 };
            CameraCanvas.SizeMode = PictureBoxSizeMode.StretchImage;
            BrightnessBar.Value = SliderStep;
            ContrastBar.Value = SliderStep;
            GammaBar.Value = SliderStep;

            // hook up interaction events
            CameraCanvas.MouseDown += Camera_MouseDown;
            CameraCanvas.MouseUp += Camera_MouseUp;
            CameraCanvas.MouseMove += CameraCanvas_MouseMove;
            Leave += Camera_Leave;

            // setup the static image cache
            UseStaticImage = false;
            StaticImageIndex = 0;

            // setup camera refresh timer
            var timer = new Timer();
            timer.Tick += Timer_Tick;
            timer.Interval = 250; //ms
            timer.Start();
        }

        public bool UseStaticImage { get; set; }

        public void RotateStaticImage(bool resetPath = false)
        {
            // check if we need a path to load images from
            if (StaticImages == null || StaticImages.Length == 0 || resetPath)
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    var result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        StaticImages = Directory.GetFiles(fbd.SelectedPath, "*.bmp");
                        StaticImageIndex = 0;
                    }
                }
            }

            if (StaticImages != null && StaticImages.Length > 0)
            {
                if (StaticImageRaw != null) StaticImageRaw.Dispose();
                // switch to the next image
                var filename = StaticImages[StaticImageIndex++ % StaticImages.Length];
                System.Diagnostics.Debug.WriteLine($"loading {filename}");
                var img = Image.FromFile(filename);
                lock (this)
                {
                    StaticImageRaw = img;
                    RawImage = (Image)StaticImageRaw.Clone();
                }
            }
        }

        public Image CroppedImage
        {
            get {
                lock (this)
                {
                    if (RawImage == null) return null;

                    // clip the current image to just the region
                    var bounds = GetRegion(RawImage.Width, RawImage.Height);

                    // crop
                    using (var dstbmp = new Bitmap(RawImage))
                    {
                        return dstbmp.Clone(bounds, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    }
                }
            }
        }

        #region private
        enum DeviceId { Front = 0, Back = 1 };
        private VideoCapture Device;
        private System.Drawing.Point RegionCorner1;
        private System.Drawing.Point RegionCorner2;
        private Pen BlackPen = new Pen(new SolidBrush(Color.Black), 10f);
        private bool IsMouseDown;
        private Image RawImage;
        private int CameraLock;

        private const int SliderStep = 10;

        // showing static images
        private int StaticImageIndex;
        private string[] StaticImages;
        private Image StaticImageRaw;

        private static Image AdjustImage(Image img, float brightness = 1f, float contrast = 1f, float gamma = 1f)
        {
            // exit eraly if no work to be done
            if (brightness == 1f && contrast == 1f && gamma == 1f) return img;
            if (img == null) return img;

            // create matrix that will brighten and contrast the image
            var adjustedBrightness = brightness - 1.0f;
            var ptsArray = new float[][] {
                new float[] {contrast, 0, 0, 0, 0}, // scale red
                new float[] {0, contrast, 0, 0, 0}, // scale green
                new float[] {0, 0, contrast, 0, 0}, // scale blue
                new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
                new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}
            };

            // apply
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

        private void Timer_Tick(object sender, EventArgs e)
        {
            // ensure there is no reentrancy
            if (System.Threading.Interlocked.CompareExchange(ref CameraLock, 1, 0) != 0) return;

            Image rawimage = null;
            if (UseStaticImage)
            {
                lock (this)
                {
                    rawimage = StaticImageRaw != null ? (Image)StaticImageRaw.Clone() : null;
                }
            }
            else
            {
                // get image from camera
                using (Mat image = new Mat()) // Frame image buffer
                {
                    // ream image
                    Device.Read(image);
                    if (image.Empty())
                    {
                        System.Diagnostics.Debug.WriteLine("empty image");
                        return;
                    }
                    using (var stream = image.ToMemoryStream(ext: ".bmp"))
                    {
                        rawimage = Image.FromStream(stream);
                    }
                }
            }

            // adjustimage
            rawimage = AdjustImage(rawimage,
                brightness: (float)BrightnessBar.Value / (float)SliderStep,
                contrast: (float)ContrastBar.Value / (float)SliderStep,
                gamma: (float)GammaBar.Value / (float)SliderStep);

            if (rawimage != null)
            {
                // mark the region
                using (var g = Graphics.FromImage(rawimage))
                {
                    // scale the screen region to img
                    var region = GetRegion(rawimage.Width, rawimage.Height);

                    // draw a rectangle from corner1 to corner2
                    g.DrawRectangle(BlackPen, region);
                }

                // set image
                lock (this)
                {
                    if (RawImage != null) RawImage.Dispose();
                    RawImage = rawimage;
                    CameraCanvas.Image = rawimage;
                }
            }

            // set state back to not running
            System.Threading.Volatile.Write(ref CameraLock, 0);
        }

        private Rectangle GetRegion(int imgwidth, int imgheight)
        {
            // get bounds (in CameraCanvas scale)
            var x = (float)Math.Min(RegionCorner1.X, RegionCorner2.X);
            var y = (float)Math.Min(RegionCorner1.Y, RegionCorner2.Y);
            var width = (float)Math.Abs(RegionCorner1.X - RegionCorner2.X);
            var height = (float)Math.Abs(RegionCorner1.Y - RegionCorner2.Y);

            // scale (to RawImage scale)
            var wscale = (float)imgwidth / (float)CameraCanvas.Width;
            var hscale = (float)imgheight / (float)CameraCanvas.Height;
            x *= wscale;
            width *= wscale;
            y *= hscale;
            height *= hscale;

            // sanitize the input (if the rectangle is outside of the image an oom is thrown)
            if (x < 0) x = 0;
            if (x >= imgwidth) x = imgwidth - 1;
            if (y < 0) y = 0;
            if (y >= imgheight) y = imgheight - 1;
            if ((x + width) >= imgwidth) width = (imgwidth - x) - 1;
            if ((y + height) >= imgheight) height = (imgheight - y) - 1;

            return new Rectangle() { X = (int)x, Y = (int)y, Width = (int)width, Height = (int)height};
        }

        private void Camera_Leave(object sender, EventArgs e)
        {
            IsMouseDown = false;
        }

        private void CameraCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            // mark region of the image to consider
            if (IsMouseDown)
            {
                // set this point
                RegionCorner2.X = e.X;
                RegionCorner2.Y = e.Y;
            }
        }

        private void Camera_MouseUp(object sender, MouseEventArgs e)
        {
            // mark region of the image to consider
            // set this point
            RegionCorner2.X = e.X;
            RegionCorner2.Y = e.Y;
            IsMouseDown = false;
        }

        private void Camera_MouseDown(object sender, MouseEventArgs e)
        {
            // mark the region of the image to consider
            // set the point and clear the other
            RegionCorner1.X = e.X;
            RegionCorner1.Y = e.Y;
            RegionCorner2.X = Width;
            RegionCorner2.Y = Height;
            IsMouseDown = true;
        }
        #endregion
    }
}
