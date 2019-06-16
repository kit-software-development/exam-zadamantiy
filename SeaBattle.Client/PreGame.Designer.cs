using System.Collections.Generic;
using System.Windows.Forms;

namespace SeaBattle.Client
{
    sealed partial class PreGame
    {
        readonly Dictionary<PictureBox, bool> _movableItems;
        private int _xPos;
        private int _yPos;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.FieldPanel = new System.Windows.Forms.Panel();
            this.DebugRefreshButton = new System.Windows.Forms.Button();
            this.FieldPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // FieldPanel
            // 
            this.FieldPanel.BackColor = System.Drawing.Color.Transparent;
            this.FieldPanel.Controls.Add(this.DebugRefreshButton);
            this.FieldPanel.Location = new System.Drawing.Point(0, 0);
            this.FieldPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.FieldPanel.Name = "FieldPanel";
            this.FieldPanel.Size = new System.Drawing.Size(1078, 705);
            this.FieldPanel.TabIndex = 5;
            this.FieldPanel.Click += new System.EventHandler(this.FieldPanel_Click);
            // 
            // DebugRefreshButton
            // 
            this.DebugRefreshButton.Location = new System.Drawing.Point(9, 10);
            this.DebugRefreshButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.DebugRefreshButton.Name = "DebugRefreshButton";
            this.DebugRefreshButton.Size = new System.Drawing.Size(96, 28);
            this.DebugRefreshButton.TabIndex = 6;
            this.DebugRefreshButton.Text = "DebugRefresh";
            this.DebugRefreshButton.UseVisualStyleBackColor = true;
            this.DebugRefreshButton.Click += new System.EventHandler(this.DebugRefreshButton_Click);
            // 
            // PreGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::SeaBattle.Client.Properties.Resources.Field;
            this.ClientSize = new System.Drawing.Size(1077, 705);
            this.Controls.Add(this.FieldPanel);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximumSize = new System.Drawing.Size(1093, 743);
            this.MinimumSize = new System.Drawing.Size(1093, 743);
            this.Name = "PreGame";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Game";
            this.Activated += new System.EventHandler(this.PreGame_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PreGame_FormClosing);
            this.LocationChanged += new System.EventHandler(this.PreGame_LocationChanged);
            this.FieldPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Panel FieldPanel;
        private Button DebugRefreshButton;
    }
}