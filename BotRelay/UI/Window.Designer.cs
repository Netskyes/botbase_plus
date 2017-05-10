namespace BotRelay
{
    partial class Window
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
            this.txtbox_Console = new System.Windows.Forms.TextBox();
            this.dtg_Packets = new System.Windows.Forms.DataGridView();
            this.dtg_Length = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dtg_Content = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dtg_Packets)).BeginInit();
            this.SuspendLayout();
            // 
            // txtbox_Console
            // 
            this.txtbox_Console.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtbox_Console.Location = new System.Drawing.Point(0, 298);
            this.txtbox_Console.Multiline = true;
            this.txtbox_Console.Name = "txtbox_Console";
            this.txtbox_Console.Size = new System.Drawing.Size(755, 121);
            this.txtbox_Console.TabIndex = 0;
            // 
            // dtg_Packets
            // 
            this.dtg_Packets.AllowUserToAddRows = false;
            this.dtg_Packets.AllowUserToDeleteRows = false;
            this.dtg_Packets.AllowUserToResizeColumns = false;
            this.dtg_Packets.AllowUserToResizeRows = false;
            this.dtg_Packets.BackgroundColor = System.Drawing.Color.White;
            this.dtg_Packets.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dtg_Packets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtg_Packets.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dtg_Length,
            this.dtg_Content});
            this.dtg_Packets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtg_Packets.GridColor = System.Drawing.Color.LightGray;
            this.dtg_Packets.Location = new System.Drawing.Point(0, 0);
            this.dtg_Packets.MultiSelect = false;
            this.dtg_Packets.Name = "dtg_Packets";
            this.dtg_Packets.RowHeadersVisible = false;
            this.dtg_Packets.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dtg_Packets.Size = new System.Drawing.Size(755, 298);
            this.dtg_Packets.TabIndex = 1;
            // 
            // dtg_Length
            // 
            this.dtg_Length.HeaderText = "Length";
            this.dtg_Length.Name = "dtg_Length";
            this.dtg_Length.ReadOnly = true;
            // 
            // dtg_Content
            // 
            this.dtg_Content.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dtg_Content.HeaderText = "Content";
            this.dtg_Content.Name = "dtg_Content";
            this.dtg_Content.ReadOnly = true;
            // 
            // Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(755, 419);
            this.Controls.Add(this.dtg_Packets);
            this.Controls.Add(this.txtbox_Console);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Name = "Window";
            this.ShowIcon = false;
            this.Text = "BotRelay";
            this.Load += new System.EventHandler(this.Window_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dtg_Packets)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtbox_Console;
        private System.Windows.Forms.DataGridView dtg_Packets;
        private System.Windows.Forms.DataGridViewTextBoxColumn dtg_Length;
        private System.Windows.Forms.DataGridViewTextBoxColumn dtg_Content;
    }
}

