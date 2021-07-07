
namespace gemsofwar.window
{
    partial class Camera
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.CameraCanvas = new System.Windows.Forms.PictureBox();
            this.BrightnessBar = new System.Windows.Forms.TrackBar();
            this.ContrastBar = new System.Windows.Forms.TrackBar();
            this.GammaBar = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.CameraCanvas)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BrightnessBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ContrastBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GammaBar)).BeginInit();
            this.SuspendLayout();
            // 
            // CameraCanvas
            // 
            this.CameraCanvas.Location = new System.Drawing.Point(0, 0);
            this.CameraCanvas.Name = "CameraCanvas";
            this.CameraCanvas.Size = new System.Drawing.Size(640, 480);
            this.CameraCanvas.TabIndex = 0;
            this.CameraCanvas.TabStop = false;
            // 
            // BrightnessBar
            // 
            this.BrightnessBar.Location = new System.Drawing.Point(11, 533);
            this.BrightnessBar.Maximum = 20;
            this.BrightnessBar.Minimum = 1;
            this.BrightnessBar.Name = "BrightnessBar";
            this.BrightnessBar.Size = new System.Drawing.Size(208, 90);
            this.BrightnessBar.TabIndex = 1;
            this.BrightnessBar.Value = 1;
            // 
            // ContrastBar
            // 
            this.ContrastBar.Location = new System.Drawing.Point(214, 533);
            this.ContrastBar.Maximum = 20;
            this.ContrastBar.Minimum = 1;
            this.ContrastBar.Name = "ContrastBar";
            this.ContrastBar.Size = new System.Drawing.Size(208, 90);
            this.ContrastBar.TabIndex = 2;
            this.ContrastBar.Value = 1;
            // 
            // GammaBar
            // 
            this.GammaBar.Location = new System.Drawing.Point(419, 533);
            this.GammaBar.Maximum = 20;
            this.GammaBar.Minimum = 1;
            this.GammaBar.Name = "GammaBar";
            this.GammaBar.Size = new System.Drawing.Size(208, 90);
            this.GammaBar.TabIndex = 3;
            this.GammaBar.Value = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 487);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 32);
            this.label1.TabIndex = 4;
            this.label1.Text = "brightness";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(214, 487);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 32);
            this.label2.TabIndex = 5;
            this.label2.Text = "contrast";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(419, 487);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 32);
            this.label3.TabIndex = 6;
            this.label3.Text = "gamma";
            // 
            // Camera
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.GammaBar);
            this.Controls.Add(this.ContrastBar);
            this.Controls.Add(this.BrightnessBar);
            this.Controls.Add(this.CameraCanvas);
            this.Name = "Camera";
            this.Size = new System.Drawing.Size(640, 640);
            ((System.ComponentModel.ISupportInitialize)(this.CameraCanvas)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BrightnessBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ContrastBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GammaBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox CameraCanvas;
        private System.Windows.Forms.TrackBar BrightnessBar;
        private System.Windows.Forms.TrackBar ContrastBar;
        private System.Windows.Forms.TrackBar GammaBar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}
