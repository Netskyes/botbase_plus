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
            this.dtg_PacketsData = new System.Windows.Forms.DataGridView();
            this.dtg_Socket = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dtg_EndPoint = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dtg_Length = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dtg_Content = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dtg_Packets = new System.Windows.Forms.DataGridView();
            this.container2 = new Container();
            this.dtg_Relays = new System.Windows.Forms.DataGridView();
            this.dtg_ClientSocket = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dtg_ClientRemote = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.container1 = new Container();
            this.txtbox_PacketContents = new System.Windows.Forms.TextBox();
            this.btn_SendPacket = new System.Windows.Forms.Button();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ASCII = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dtg_PacketsData)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtg_Packets)).BeginInit();
            this.container2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtg_Relays)).BeginInit();
            this.container1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtbox_Console
            // 
            this.txtbox_Console.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtbox_Console.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.txtbox_Console.Location = new System.Drawing.Point(0, 391);
            this.txtbox_Console.Multiline = true;
            this.txtbox_Console.Name = "txtbox_Console";
            this.txtbox_Console.Size = new System.Drawing.Size(724, 135);
            this.txtbox_Console.TabIndex = 0;
            // 
            // dtg_PacketsData
            // 
            this.dtg_PacketsData.AllowUserToAddRows = false;
            this.dtg_PacketsData.AllowUserToDeleteRows = false;
            this.dtg_PacketsData.AllowUserToResizeColumns = false;
            this.dtg_PacketsData.AllowUserToResizeRows = false;
            this.dtg_PacketsData.BackgroundColor = System.Drawing.Color.White;
            this.dtg_PacketsData.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dtg_PacketsData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtg_PacketsData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dtg_Socket,
            this.dtg_EndPoint,
            this.dtg_Length,
            this.dtg_Content});
            this.dtg_PacketsData.Dock = System.Windows.Forms.DockStyle.Top;
            this.dtg_PacketsData.GridColor = System.Drawing.Color.LightGray;
            this.dtg_PacketsData.Location = new System.Drawing.Point(0, 0);
            this.dtg_PacketsData.MultiSelect = false;
            this.dtg_PacketsData.Name = "dtg_PacketsData";
            this.dtg_PacketsData.RowHeadersVisible = false;
            this.dtg_PacketsData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dtg_PacketsData.Size = new System.Drawing.Size(724, 344);
            this.dtg_PacketsData.TabIndex = 1;
            // 
            // dtg_Socket
            // 
            this.dtg_Socket.HeaderText = "Socket";
            this.dtg_Socket.Name = "dtg_Socket";
            this.dtg_Socket.ReadOnly = true;
            // 
            // dtg_EndPoint
            // 
            this.dtg_EndPoint.HeaderText = "Remote";
            this.dtg_EndPoint.Name = "dtg_EndPoint";
            this.dtg_EndPoint.ReadOnly = true;
            this.dtg_EndPoint.Width = 130;
            // 
            // dtg_Length
            // 
            this.dtg_Length.HeaderText = "Length";
            this.dtg_Length.Name = "dtg_Length";
            this.dtg_Length.ReadOnly = true;
            this.dtg_Length.Width = 70;
            // 
            // dtg_Content
            // 
            this.dtg_Content.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dtg_Content.HeaderText = "Content";
            this.dtg_Content.Name = "dtg_Content";
            this.dtg_Content.ReadOnly = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(732, 552);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.txtbox_Console);
            this.tabPage1.Controls.Add(this.dtg_PacketsData);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(724, 526);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Monitor";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.container2);
            this.tabPage2.Controls.Add(this.dtg_Packets);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(724, 526);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Packet Editor";
            this.tabPage2.UseVisualStyleBackColor = true;
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
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.Column1,
            this.dataGridViewTextBoxColumn3,
            this.ASCII});
            this.dtg_Packets.Dock = System.Windows.Forms.DockStyle.Top;
            this.dtg_Packets.GridColor = System.Drawing.Color.LightGray;
            this.dtg_Packets.Location = new System.Drawing.Point(0, 0);
            this.dtg_Packets.MultiSelect = false;
            this.dtg_Packets.Name = "dtg_Packets";
            this.dtg_Packets.RowHeadersVisible = false;
            this.dtg_Packets.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dtg_Packets.Size = new System.Drawing.Size(724, 362);
            this.dtg_Packets.TabIndex = 3;
            this.dtg_Packets.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dtg_Packets_CellClick);
            // 
            // container2
            // 
            this.container2.Controls.Add(this.dtg_Relays);
            this.container2.Controls.Add(this.container1);
            this.container2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.container2.Location = new System.Drawing.Point(0, 368);
            this.container2.Name = "container2";
            this.container2.Size = new System.Drawing.Size(724, 158);
            this.container2.TabIndex = 7;
            // 
            // dtg_Relays
            // 
            this.dtg_Relays.AllowUserToAddRows = false;
            this.dtg_Relays.AllowUserToDeleteRows = false;
            this.dtg_Relays.AllowUserToResizeColumns = false;
            this.dtg_Relays.AllowUserToResizeRows = false;
            this.dtg_Relays.BackgroundColor = System.Drawing.Color.White;
            this.dtg_Relays.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dtg_Relays.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtg_Relays.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dtg_ClientSocket,
            this.dtg_ClientRemote});
            this.dtg_Relays.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dtg_Relays.GridColor = System.Drawing.Color.LightGray;
            this.dtg_Relays.Location = new System.Drawing.Point(0, 0);
            this.dtg_Relays.MultiSelect = false;
            this.dtg_Relays.Name = "dtg_Relays";
            this.dtg_Relays.RowHeadersVisible = false;
            this.dtg_Relays.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dtg_Relays.Size = new System.Drawing.Size(286, 158);
            this.dtg_Relays.TabIndex = 2;
            // 
            // dtg_ClientSocket
            // 
            this.dtg_ClientSocket.HeaderText = "Socket";
            this.dtg_ClientSocket.Name = "dtg_ClientSocket";
            this.dtg_ClientSocket.ReadOnly = true;
            // 
            // dtg_ClientRemote
            // 
            this.dtg_ClientRemote.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dtg_ClientRemote.HeaderText = "Remote";
            this.dtg_ClientRemote.Name = "dtg_ClientRemote";
            this.dtg_ClientRemote.ReadOnly = true;
            // 
            // container1
            // 
            this.container1.Controls.Add(this.txtbox_PacketContents);
            this.container1.Controls.Add(this.btn_SendPacket);
            this.container1.Dock = System.Windows.Forms.DockStyle.Right;
            this.container1.Location = new System.Drawing.Point(286, 0);
            this.container1.Name = "container1";
            this.container1.Size = new System.Drawing.Size(438, 158);
            this.container1.TabIndex = 6;
            // 
            // txtbox_PacketContents
            // 
            this.txtbox_PacketContents.Location = new System.Drawing.Point(179, 16);
            this.txtbox_PacketContents.Multiline = true;
            this.txtbox_PacketContents.Name = "txtbox_PacketContents";
            this.txtbox_PacketContents.Size = new System.Drawing.Size(251, 100);
            this.txtbox_PacketContents.TabIndex = 5;
            // 
            // btn_SendPacket
            // 
            this.btn_SendPacket.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_SendPacket.Location = new System.Drawing.Point(98, 16);
            this.btn_SendPacket.Name = "btn_SendPacket";
            this.btn_SendPacket.Size = new System.Drawing.Size(75, 23);
            this.btn_SendPacket.TabIndex = 4;
            this.btn_SendPacket.Text = "Send Packet";
            this.btn_SendPacket.UseVisualStyleBackColor = true;
            this.btn_SendPacket.Click += new System.EventHandler(this.btn_SendPacket_Click);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "Socket";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "Remote";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.Width = 130;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "OpCode";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 70;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.HeaderText = "Length";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Width = 70;
            // 
            // ASCII
            // 
            this.ASCII.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ASCII.HeaderText = "ASCII";
            this.ASCII.Name = "ASCII";
            this.ASCII.ReadOnly = true;
            // 
            // Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(732, 552);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Name = "Window";
            this.ShowIcon = false;
            this.Text = "BotRelay";
            this.Load += new System.EventHandler(this.Window_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dtg_PacketsData)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtg_Packets)).EndInit();
            this.container2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtg_Relays)).EndInit();
            this.container1.ResumeLayout(false);
            this.container1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtbox_Console;
        private System.Windows.Forms.DataGridView dtg_PacketsData;
        private System.Windows.Forms.DataGridViewTextBoxColumn dtg_Socket;
        private System.Windows.Forms.DataGridViewTextBoxColumn dtg_EndPoint;
        private System.Windows.Forms.DataGridViewTextBoxColumn dtg_Length;
        private System.Windows.Forms.DataGridViewTextBoxColumn dtg_Content;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dtg_Relays;
        private System.Windows.Forms.DataGridViewTextBoxColumn dtg_ClientSocket;
        private System.Windows.Forms.DataGridViewTextBoxColumn dtg_ClientRemote;
        private System.Windows.Forms.DataGridView dtg_Packets;
        private System.Windows.Forms.Button btn_SendPacket;
        private System.Windows.Forms.TextBox txtbox_PacketContents;
        private Container container1;
        private Container container2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn ASCII;
    }
}

