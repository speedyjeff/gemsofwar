
using gemsofwar.engine;

namespace gemsofwar.window
{
    partial class MainPage
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.CameraControl = new gemsofwar.window.Camera();
            this.CaptureButton = new System.Windows.Forms.Button();
            this.GemGridControl = new gemsofwar.window.GemGrid();
            this.GemModeComboBox = new System.Windows.Forms.ComboBox();
            this.GemBoardControl = new gemsofwar.window.GemBoard();
            this.TrainButton = new System.Windows.Forms.Button();
            this.DemoButton = new System.Windows.Forms.Button();
            this.GuessButton = new System.Windows.Forms.Button();
            this.InfoPanelControl = new gemsofwar.window.InfoPanel();
            this.DemoPathButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CameraControl
            // 
            this.CameraControl.BackColor = System.Drawing.Color.White;
            this.CameraControl.Location = new System.Drawing.Point(0, -3);
            this.CameraControl.Name = "CameraControl";
            this.CameraControl.Size = new System.Drawing.Size(640, 600);
            this.CameraControl.TabIndex = 0;
            this.CameraControl.UseStaticImage = false;
            // 
            // CaptureButton
            // 
            this.CaptureButton.Location = new System.Drawing.Point(12, 623);
            this.CaptureButton.Name = "CaptureButton";
            this.CaptureButton.Size = new System.Drawing.Size(150, 46);
            this.CaptureButton.TabIndex = 1;
            this.CaptureButton.Text = "Capture";
            this.CaptureButton.UseVisualStyleBackColor = true;
            // 
            // GemGridControl
            // 
            this.GemGridControl.Location = new System.Drawing.Point(0, 753);
            this.GemGridControl.Name = "GemGridControl";
            this.GemGridControl.Size = new System.Drawing.Size(640, 500);
            this.GemGridControl.TabIndex = 2;
            // 
            // GemModeComboBox
            // 
            this.GemModeComboBox.FormattingEnabled = true;
            this.GemModeComboBox.Items.AddRange(new object[] {
            "Battle",
            "Treasure"});
            this.GemModeComboBox.Location = new System.Drawing.Point(12, 679);
            this.GemModeComboBox.Name = "GemModeComboBox";
            this.GemModeComboBox.Size = new System.Drawing.Size(242, 40);
            this.GemModeComboBox.TabIndex = 3;
            // 
            // GemBoardControl
            // 
            this.GemBoardControl.GameMode = gemsofwar.engine.GameMode.Battle;
            this.GemBoardControl.Location = new System.Drawing.Point(691, 6);
            this.GemBoardControl.Name = "GemBoardControl";
            this.GemBoardControl.Size = new System.Drawing.Size(726, 642);
            this.GemBoardControl.TabIndex = 5;
            // 
            // TrainButton
            // 
            this.TrainButton.Location = new System.Drawing.Point(168, 623);
            this.TrainButton.Name = "TrainButton";
            this.TrainButton.Size = new System.Drawing.Size(150, 46);
            this.TrainButton.TabIndex = 6;
            this.TrainButton.Text = "Train";
            this.TrainButton.UseVisualStyleBackColor = true;
            // 
            // DemoButton
            // 
            this.DemoButton.Location = new System.Drawing.Point(480, 623);
            this.DemoButton.Name = "DemoButton";
            this.DemoButton.Size = new System.Drawing.Size(150, 46);
            this.DemoButton.TabIndex = 8;
            this.DemoButton.Text = "Demo";
            this.DemoButton.UseVisualStyleBackColor = true;
            // 
            // GuessButton
            // 
            this.GuessButton.Location = new System.Drawing.Point(324, 623);
            this.GuessButton.Name = "GuessButton";
            this.GuessButton.Size = new System.Drawing.Size(150, 46);
            this.GuessButton.TabIndex = 9;
            this.GuessButton.Text = "Guess";
            this.GuessButton.UseVisualStyleBackColor = true;
            // 
            // InfoPanelControl
            // 
            this.InfoPanelControl.Location = new System.Drawing.Point(687, 679);
            this.InfoPanelControl.Name = "InfoPanelControl";
            this.InfoPanelControl.Size = new System.Drawing.Size(730, 561);
            this.InfoPanelControl.TabIndex = 10;
            // 
            // DemoPathButton
            // 
            this.DemoPathButton.Location = new System.Drawing.Point(480, 679);
            this.DemoPathButton.Name = "DemoPathButton";
            this.DemoPathButton.Size = new System.Drawing.Size(150, 46);
            this.DemoPathButton.TabIndex = 11;
            this.DemoPathButton.Text = "Demo Path";
            this.DemoPathButton.UseVisualStyleBackColor = true;
            // 
            // MainPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1523, 1281);
            this.Controls.Add(this.DemoPathButton);
            this.Controls.Add(this.InfoPanelControl);
            this.Controls.Add(this.GuessButton);
            this.Controls.Add(this.DemoButton);
            this.Controls.Add(this.TrainButton);
            this.Controls.Add(this.GemBoardControl);
            this.Controls.Add(this.GemModeComboBox);
            this.Controls.Add(this.GemGridControl);
            this.Controls.Add(this.CaptureButton);
            this.Controls.Add(this.CameraControl);
            this.Name = "MainPage";
            this.Text = "gemsofwar";
            this.ResumeLayout(false);

        }

        #endregion

        private Camera CameraControl;
        private System.Windows.Forms.Button CaptureButton;
        private GemGrid GemGridControl;
        private System.Windows.Forms.ComboBox GemModeComboBox;
        private GemBoard GemBoardControl;
        private System.Windows.Forms.Button TrainButton;
        private System.Windows.Forms.Button DemoButton;
        private System.Windows.Forms.Button GuessButton;
        private InfoPanel InfoPanelControl;
        private System.Windows.Forms.Button DemoPathButton;
    }
}

