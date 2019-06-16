namespace SeaBattle.Client
{
    partial class LobbyForm
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
            this.DebugRefreshButton = new System.Windows.Forms.Button();
            this.FieldPanel = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.FieldPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // DebugRefreshButton
            // 
            this.DebugRefreshButton.Location = new System.Drawing.Point(9, 10);
            this.DebugRefreshButton.Margin = new System.Windows.Forms.Padding(2);
            this.DebugRefreshButton.Name = "DebugRefreshButton";
            this.DebugRefreshButton.Size = new System.Drawing.Size(96, 28);
            this.DebugRefreshButton.TabIndex = 7;
            this.DebugRefreshButton.Text = "DebugRefresh";
            this.DebugRefreshButton.UseVisualStyleBackColor = true;
            this.DebugRefreshButton.Click += new System.EventHandler(this.DebugRefreshButton_Click);
            // 
            // FieldPanel
            // 
            this.FieldPanel.BackColor = System.Drawing.Color.Transparent;
            this.FieldPanel.Controls.Add(this.button1);
            this.FieldPanel.Location = new System.Drawing.Point(0, 0);
            this.FieldPanel.Margin = new System.Windows.Forms.Padding(2);
            this.FieldPanel.Name = "FieldPanel";
            this.FieldPanel.Size = new System.Drawing.Size(779, 574);
            this.FieldPanel.TabIndex = 8;
            this.FieldPanel.Click += new System.EventHandler(this.FieldPanel_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(9, 10);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(96, 28);
            this.button1.TabIndex = 6;
            this.button1.Text = "DebugRefresh";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.DebugRefreshButton_Click);
            // 
            // LobbyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::SeaBattle.Client.Properties.Resources.Field;
            this.ClientSize = new System.Drawing.Size(779, 574);
            this.Controls.Add(this.FieldPanel);
            this.Controls.Add(this.DebugRefreshButton);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximumSize = new System.Drawing.Size(795, 612);
            this.MinimumSize = new System.Drawing.Size(795, 612);
            this.Name = "LobbyForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Lobby";
            this.Activated += new System.EventHandler(this.LobbyForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LobbyForm_FormClosing);
            this.LocationChanged += new System.EventHandler(this.LobbyForm_LocationChanged);
            this.FieldPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button DebugRefreshButton;
        public System.Windows.Forms.Panel FieldPanel;
        private System.Windows.Forms.Button button1;
    }
}