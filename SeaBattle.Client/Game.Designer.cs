namespace SeaBattle.Client
{
    partial class Game
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.FieldPanel = new System.Windows.Forms.Panel();
            this.DebugRefresh = new System.Windows.Forms.Button();
            this.FieldPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // FieldPanel
            // 
            this.FieldPanel.BackColor = System.Drawing.Color.Transparent;
            this.FieldPanel.Controls.Add(this.DebugRefresh);
            this.FieldPanel.Location = new System.Drawing.Point(0, 0);
            this.FieldPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.FieldPanel.Name = "FieldPanel";
            this.FieldPanel.Size = new System.Drawing.Size(1078, 706);
            this.FieldPanel.TabIndex = 9;
            this.FieldPanel.Click += new System.EventHandler(this.FieldPanel_Click);
            // 
            // DebugRefresh
            // 
            this.DebugRefresh.Location = new System.Drawing.Point(9, 10);
            this.DebugRefresh.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.DebugRefresh.Name = "DebugRefresh";
            this.DebugRefresh.Size = new System.Drawing.Size(96, 28);
            this.DebugRefresh.TabIndex = 6;
            this.DebugRefresh.Text = "DebugRefresh";
            this.DebugRefresh.UseVisualStyleBackColor = true;
            this.DebugRefresh.Click += new System.EventHandler(this.DebugRefresh_Click);
            // 
            // Game
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::SeaBattle.Client.Properties.Resources.Field;
            this.ClientSize = new System.Drawing.Size(1077, 705);
            this.Controls.Add(this.FieldPanel);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximumSize = new System.Drawing.Size(1093, 743);
            this.MinimumSize = new System.Drawing.Size(1093, 743);
            this.Name = "Game";
            this.Text = "Game";
            this.Activated += new System.EventHandler(this.Game_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Game_FormClosing);
            this.FieldPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        
        private System.Windows.Forms.Panel FieldPanel;
        private System.Windows.Forms.Button DebugRefresh;
    }
}