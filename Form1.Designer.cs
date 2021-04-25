
using System;

namespace P3D_Scenario_Generator
{
    partial class Form
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
            this.components = new System.ComponentModel.Container();
            this.TabControl = new System.Windows.Forms.TabControl();
            this.TabPageGeneral = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.TextBoxScenarioTitle = new System.Windows.Forms.TextBox();
            this.ListBoxAircraft = new System.Windows.Forms.ListBox();
            this.buttonAircraft = new System.Windows.Forms.Button();
            this.TextBoxSelectedScenario = new System.Windows.Forms.TextBox();
            this.ListBoxScenarioType = new System.Windows.Forms.ListBox();
            this.ButtonRandRunway = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.TextBoxSearchRunway = new System.Windows.Forms.TextBox();
            this.TextBoxSelectedRunway = new System.Windows.Forms.TextBox();
            this.ListBoxRunways = new System.Windows.Forms.ListBox();
            this.TabPageCircuit = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.TextBoxCircuitUpwind = new System.Windows.Forms.TextBox();
            this.TextBoxCircuitHeightUpwind = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.TextBoxCircuitBase = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.TextBoxCircuitFinal = new System.Windows.Forms.TextBox();
            this.TextBoxCircuitHeightDown = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TextBoxCircuitSpeed = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.TextBoxCircuitHeightBase = new System.Windows.Forms.TextBox();
            this.ButtonCircuitDefault = new System.Windows.Forms.Button();
            this.PictureBoxCircuit = new System.Windows.Forms.PictureBox();
            this.ButtonGenerateScenario = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.ButtonHelp = new System.Windows.Forms.Button();
            this.TabControl.SuspendLayout();
            this.TabPageGeneral.SuspendLayout();
            this.TabPageCircuit.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxCircuit)).BeginInit();
            this.SuspendLayout();
            // 
            // TabControl
            // 
            this.TabControl.Controls.Add(this.TabPageGeneral);
            this.TabControl.Controls.Add(this.TabPageCircuit);
            this.TabControl.Location = new System.Drawing.Point(12, 12);
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedIndex = 0;
            this.TabControl.Size = new System.Drawing.Size(820, 466);
            this.TabControl.TabIndex = 0;
            this.TabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl_SelectedIndexChanged);
            // 
            // TabPageGeneral
            // 
            this.TabPageGeneral.Controls.Add(this.label8);
            this.TabPageGeneral.Controls.Add(this.TextBoxScenarioTitle);
            this.TabPageGeneral.Controls.Add(this.ListBoxAircraft);
            this.TabPageGeneral.Controls.Add(this.buttonAircraft);
            this.TabPageGeneral.Controls.Add(this.TextBoxSelectedScenario);
            this.TabPageGeneral.Controls.Add(this.ListBoxScenarioType);
            this.TabPageGeneral.Controls.Add(this.ButtonRandRunway);
            this.TabPageGeneral.Controls.Add(this.label2);
            this.TabPageGeneral.Controls.Add(this.label1);
            this.TabPageGeneral.Controls.Add(this.TextBoxSearchRunway);
            this.TabPageGeneral.Controls.Add(this.TextBoxSelectedRunway);
            this.TabPageGeneral.Controls.Add(this.ListBoxRunways);
            this.TabPageGeneral.Location = new System.Drawing.Point(4, 24);
            this.TabPageGeneral.Name = "TabPageGeneral";
            this.TabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageGeneral.Size = new System.Drawing.Size(812, 438);
            this.TabPageGeneral.TabIndex = 0;
            this.TabPageGeneral.Text = "General";
            this.TabPageGeneral.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 402);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(77, 15);
            this.label8.TabIndex = 14;
            this.label8.Text = "Scenario Title";
            // 
            // TextBoxScenarioTitle
            // 
            this.TextBoxScenarioTitle.Location = new System.Drawing.Point(112, 399);
            this.TextBoxScenarioTitle.Name = "TextBoxScenarioTitle";
            this.TextBoxScenarioTitle.Size = new System.Drawing.Size(206, 23);
            this.TextBoxScenarioTitle.TabIndex = 13;
            // 
            // ListBoxAircraft
            // 
            this.ListBoxAircraft.FormattingEnabled = true;
            this.ListBoxAircraft.ItemHeight = 15;
            this.ListBoxAircraft.Location = new System.Drawing.Point(488, 24);
            this.ListBoxAircraft.Name = "ListBoxAircraft";
            this.ListBoxAircraft.Size = new System.Drawing.Size(250, 94);
            this.ListBoxAircraft.TabIndex = 12;
            // 
            // buttonAircraft
            // 
            this.buttonAircraft.Location = new System.Drawing.Point(488, 135);
            this.buttonAircraft.Name = "buttonAircraft";
            this.buttonAircraft.Size = new System.Drawing.Size(120, 23);
            this.buttonAircraft.TabIndex = 11;
            this.buttonAircraft.Text = "aircraft.cfg";
            this.toolTip1.SetToolTip(this.buttonAircraft, "Select the \"aircraft.cfg\" file for your aircraft and then the variation from the " +
        "list above");
            this.buttonAircraft.UseVisualStyleBackColor = true;
            this.buttonAircraft.Click += new System.EventHandler(this.ButtonAircraft_Click);
            // 
            // TextBoxSelectedScenario
            // 
            this.TextBoxSelectedScenario.Enabled = false;
            this.TextBoxSelectedScenario.Location = new System.Drawing.Point(300, 136);
            this.TextBoxSelectedScenario.Name = "TextBoxSelectedScenario";
            this.TextBoxSelectedScenario.Size = new System.Drawing.Size(119, 23);
            this.TextBoxSelectedScenario.TabIndex = 7;
            // 
            // ListBoxScenarioType
            // 
            this.ListBoxScenarioType.FormattingEnabled = true;
            this.ListBoxScenarioType.ItemHeight = 15;
            this.ListBoxScenarioType.Location = new System.Drawing.Point(300, 24);
            this.ListBoxScenarioType.Name = "ListBoxScenarioType";
            this.ListBoxScenarioType.Size = new System.Drawing.Size(120, 94);
            this.ListBoxScenarioType.TabIndex = 6;
            this.ListBoxScenarioType.Click += new System.EventHandler(this.ListBoxScenarioType_Click);
            this.ListBoxScenarioType.SelectedIndexChanged += new System.EventHandler(this.ListBoxScenarioType_SelectedIndexChanged);
            // 
            // ButtonRandRunway
            // 
            this.ButtonRandRunway.Location = new System.Drawing.Point(112, 233);
            this.ButtonRandRunway.Name = "ButtonRandRunway";
            this.ButtonRandRunway.Size = new System.Drawing.Size(119, 23);
            this.ButtonRandRunway.TabIndex = 5;
            this.ButtonRandRunway.Text = "Random Runway";
            this.ButtonRandRunway.UseVisualStyleBackColor = true;
            this.ButtonRandRunway.Click += new System.EventHandler(this.ButtonRandRunway_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 191);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Selected";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 143);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Search";
            // 
            // TextBoxSearchRunway
            // 
            this.TextBoxSearchRunway.Location = new System.Drawing.Point(112, 136);
            this.TextBoxSearchRunway.Name = "TextBoxSearchRunway";
            this.TextBoxSearchRunway.Size = new System.Drawing.Size(119, 23);
            this.TextBoxSearchRunway.TabIndex = 2;
            this.TextBoxSearchRunway.TextChanged += new System.EventHandler(this.TextBoxSearchRunway_TextChanged);
            // 
            // TextBoxSelectedRunway
            // 
            this.TextBoxSelectedRunway.Enabled = false;
            this.TextBoxSelectedRunway.Location = new System.Drawing.Point(112, 183);
            this.TextBoxSelectedRunway.Name = "TextBoxSelectedRunway";
            this.TextBoxSelectedRunway.Size = new System.Drawing.Size(119, 23);
            this.TextBoxSelectedRunway.TabIndex = 1;
            // 
            // ListBoxRunways
            // 
            this.ListBoxRunways.FormattingEnabled = true;
            this.ListBoxRunways.ItemHeight = 15;
            this.ListBoxRunways.Location = new System.Drawing.Point(112, 24);
            this.ListBoxRunways.Name = "ListBoxRunways";
            this.ListBoxRunways.Size = new System.Drawing.Size(120, 94);
            this.ListBoxRunways.TabIndex = 0;
            this.ListBoxRunways.SelectedIndexChanged += new System.EventHandler(this.ListBoxRunways_SelectedIndexChanged);
            // 
            // TabPageCircuit
            // 
            this.TabPageCircuit.Controls.Add(this.tableLayoutPanel1);
            this.TabPageCircuit.Controls.Add(this.ButtonCircuitDefault);
            this.TabPageCircuit.Controls.Add(this.PictureBoxCircuit);
            this.TabPageCircuit.Location = new System.Drawing.Point(4, 24);
            this.TabPageCircuit.Name = "TabPageCircuit";
            this.TabPageCircuit.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageCircuit.Size = new System.Drawing.Size(812, 438);
            this.TabPageCircuit.TabIndex = 1;
            this.TabPageCircuit.Text = "Circuit";
            this.TabPageCircuit.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 7;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label6, 6, 0);
            this.tableLayoutPanel1.Controls.Add(this.TextBoxCircuitUpwind, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.TextBoxCircuitHeightUpwind, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.label9, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.TextBoxCircuitBase, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label5, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.TextBoxCircuitFinal, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.TextBoxCircuitHeightDown, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.TextBoxCircuitSpeed, 6, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label10, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.TextBoxCircuitHeightBase, 5, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 361);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(795, 58);
            this.tableLayoutPanel1.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AllowDrop = true;
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(31, 7);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 15);
            this.label7.TabIndex = 10;
            this.label7.Text = "Upwind";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(719, 7);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(39, 15);
            this.label6.TabIndex = 9;
            this.label6.Text = "Speed";
            // 
            // TextBoxCircuitUpwind
            // 
            this.TextBoxCircuitUpwind.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.TextBoxCircuitUpwind.Location = new System.Drawing.Point(5, 32);
            this.TextBoxCircuitUpwind.Name = "TextBoxCircuitUpwind";
            this.TextBoxCircuitUpwind.Size = new System.Drawing.Size(100, 23);
            this.TextBoxCircuitUpwind.TabIndex = 5;
            this.TextBoxCircuitUpwind.Text = "0";
            this.TextBoxCircuitUpwind.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitUpwind, "Distance between runway and gate 1 in miles");
            this.TextBoxCircuitUpwind.TextChanged += new System.EventHandler(this.TextBoxCircuitUpwind_TextChanged);
            // 
            // TextBoxCircuitHeightUpwind
            // 
            this.TextBoxCircuitHeightUpwind.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.TextBoxCircuitHeightUpwind.Location = new System.Drawing.Point(342, 32);
            this.TextBoxCircuitHeightUpwind.Name = "TextBoxCircuitHeightUpwind";
            this.TextBoxCircuitHeightUpwind.Size = new System.Drawing.Size(100, 23);
            this.TextBoxCircuitHeightUpwind.TabIndex = 13;
            this.TextBoxCircuitHeightUpwind.Text = "0";
            this.TextBoxCircuitHeightUpwind.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitHeightUpwind, "Height of circuit above runway in feet (gate 1)");
            this.TextBoxCircuitHeightUpwind.TextChanged += new System.EventHandler(this.TextBoxCircuitHeight_TextChanged);
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(583, 7);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(87, 15);
            this.label9.TabIndex = 15;
            this.label9.Text = "Height (Gate 8)";
            // 
            // TextBoxCircuitBase
            // 
            this.TextBoxCircuitBase.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.TextBoxCircuitBase.Location = new System.Drawing.Point(116, 32);
            this.TextBoxCircuitBase.Name = "TextBoxCircuitBase";
            this.TextBoxCircuitBase.Size = new System.Drawing.Size(100, 23);
            this.TextBoxCircuitBase.TabIndex = 1;
            this.TextBoxCircuitBase.Text = "0";
            this.TextBoxCircuitBase.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitBase, "Distance between gates 2 and 3 (6 and 7) in miles");
            this.TextBoxCircuitBase.TextChanged += new System.EventHandler(this.TextBoxCircuitBase_TextChanged);
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(460, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(103, 15);
            this.label5.TabIndex = 8;
            this.label5.Text = "Height (Gates 3-6)";
            // 
            // TextBoxCircuitFinal
            // 
            this.TextBoxCircuitFinal.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.TextBoxCircuitFinal.Location = new System.Drawing.Point(227, 32);
            this.TextBoxCircuitFinal.Name = "TextBoxCircuitFinal";
            this.TextBoxCircuitFinal.Size = new System.Drawing.Size(100, 23);
            this.TextBoxCircuitFinal.TabIndex = 2;
            this.TextBoxCircuitFinal.Text = "0";
            this.TextBoxCircuitFinal.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitFinal, "Distance between gate 8 and runway in miles");
            this.TextBoxCircuitFinal.TextChanged += new System.EventHandler(this.TextBoxCircuitFinal_TextChanged);
            // 
            // TextBoxCircuitHeightDown
            // 
            this.TextBoxCircuitHeightDown.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.TextBoxCircuitHeightDown.Location = new System.Drawing.Point(461, 32);
            this.TextBoxCircuitHeightDown.Name = "TextBoxCircuitHeightDown";
            this.TextBoxCircuitHeightDown.Size = new System.Drawing.Size(100, 23);
            this.TextBoxCircuitHeightDown.TabIndex = 3;
            this.TextBoxCircuitHeightDown.Text = "0";
            this.TextBoxCircuitHeightDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitHeightDown, "Height of circuit above runway in feet (gates 3 to 6)");
            this.TextBoxCircuitHeightDown.TextChanged += new System.EventHandler(this.TextBoxCircuitHeight_TextChanged);
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(261, 7);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 15);
            this.label4.TabIndex = 7;
            this.label4.Text = "Final";
            // 
            // TextBoxCircuitSpeed
            // 
            this.TextBoxCircuitSpeed.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.TextBoxCircuitSpeed.Location = new System.Drawing.Point(688, 32);
            this.TextBoxCircuitSpeed.Name = "TextBoxCircuitSpeed";
            this.TextBoxCircuitSpeed.Size = new System.Drawing.Size(100, 23);
            this.TextBoxCircuitSpeed.TabIndex = 4;
            this.TextBoxCircuitSpeed.Text = "0";
            this.TextBoxCircuitSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitSpeed, "Cruise speed between gates 1 and 8 in knots");
            this.TextBoxCircuitSpeed.TextChanged += new System.EventHandler(this.TextBoxCircuitSpeed_TextChanged);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(151, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "Base";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(349, 7);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(87, 15);
            this.label10.TabIndex = 16;
            this.label10.Text = "Height (Gate 1)";
            // 
            // TextBoxCircuitHeightBase
            // 
            this.TextBoxCircuitHeightBase.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.TextBoxCircuitHeightBase.Location = new System.Drawing.Point(576, 32);
            this.TextBoxCircuitHeightBase.Name = "TextBoxCircuitHeightBase";
            this.TextBoxCircuitHeightBase.Size = new System.Drawing.Size(100, 23);
            this.TextBoxCircuitHeightBase.TabIndex = 12;
            this.TextBoxCircuitHeightBase.Text = "0";
            this.TextBoxCircuitHeightBase.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitHeightBase, "Height of circuit above runway in feet (gate 8)");
            this.TextBoxCircuitHeightBase.TextChanged += new System.EventHandler(this.TextBoxCircuitHeight_TextChanged);
            // 
            // ButtonCircuitDefault
            // 
            this.ButtonCircuitDefault.Location = new System.Drawing.Point(469, 15);
            this.ButtonCircuitDefault.Name = "ButtonCircuitDefault";
            this.ButtonCircuitDefault.Size = new System.Drawing.Size(75, 23);
            this.ButtonCircuitDefault.TabIndex = 11;
            this.ButtonCircuitDefault.Text = "Default";
            this.ButtonCircuitDefault.UseVisualStyleBackColor = true;
            this.ButtonCircuitDefault.Click += new System.EventHandler(this.ButtonCircuitDefault_Click);
            // 
            // PictureBoxCircuit
            // 
            this.PictureBoxCircuit.Location = new System.Drawing.Point(15, 15);
            this.PictureBoxCircuit.Name = "PictureBoxCircuit";
            this.PictureBoxCircuit.Size = new System.Drawing.Size(783, 325);
            this.PictureBoxCircuit.TabIndex = 0;
            this.PictureBoxCircuit.TabStop = false;
            // 
            // ButtonGenerateScenario
            // 
            this.ButtonGenerateScenario.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.ButtonGenerateScenario.Location = new System.Drawing.Point(343, 493);
            this.ButtonGenerateScenario.Name = "ButtonGenerateScenario";
            this.ButtonGenerateScenario.Size = new System.Drawing.Size(152, 43);
            this.ButtonGenerateScenario.TabIndex = 1;
            this.ButtonGenerateScenario.Text = "Generate Scenario";
            this.ButtonGenerateScenario.UseVisualStyleBackColor = true;
            this.ButtonGenerateScenario.Click += new System.EventHandler(this.ButtonGenerateScenario_Click);
            // 
            // ButtonHelp
            // 
            this.ButtonHelp.Location = new System.Drawing.Point(753, 513);
            this.ButtonHelp.Name = "ButtonHelp";
            this.ButtonHelp.Size = new System.Drawing.Size(75, 23);
            this.ButtonHelp.TabIndex = 2;
            this.ButtonHelp.Text = "Help";
            this.ButtonHelp.UseVisualStyleBackColor = true;
            this.ButtonHelp.Click += new System.EventHandler(this.ButtonHelp_Click);
            // 
            // Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(844, 553);
            this.Controls.Add(this.ButtonHelp);
            this.Controls.Add(this.ButtonGenerateScenario);
            this.Controls.Add(this.TabControl);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form";
            this.Text = "P3D Scenario Generator";
            this.TabControl.ResumeLayout(false);
            this.TabPageGeneral.ResumeLayout(false);
            this.TabPageGeneral.PerformLayout();
            this.TabPageCircuit.ResumeLayout(false);
            this.TabPageCircuit.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxCircuit)).EndInit();
            this.ResumeLayout(false);

        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        #endregion

        private System.Windows.Forms.TabControl TabControl;
        private System.Windows.Forms.TabPage TabPageGeneral;
        private System.Windows.Forms.TabPage TabPageCircuit;
        private System.Windows.Forms.TextBox TextBoxSearchRunway;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ButtonRandRunway;
        private System.Windows.Forms.Button ButtonGenerateScenario;
        private System.Windows.Forms.PictureBox PictureBoxCircuit;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonAircraft;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button ButtonCircuitDefault;
        internal System.Windows.Forms.ListBox ListBoxRunways;
        internal System.Windows.Forms.TextBox TextBoxSelectedRunway;
        internal System.Windows.Forms.TextBox TextBoxSelectedScenario;
        internal System.Windows.Forms.ListBox ListBoxScenarioType;
        internal System.Windows.Forms.TextBox TextBoxCircuitUpwind;
        internal System.Windows.Forms.TextBox TextBoxCircuitSpeed;
        internal System.Windows.Forms.TextBox TextBoxCircuitHeightDown;
        internal System.Windows.Forms.TextBox TextBoxCircuitFinal;
        internal System.Windows.Forms.TextBox TextBoxCircuitBase;
        internal System.Windows.Forms.ListBox ListBoxAircraft;
        private System.Windows.Forms.Button ButtonHelp;
        private System.Windows.Forms.Label label8;
        internal System.Windows.Forms.TextBox TextBoxScenarioTitle;
        internal System.Windows.Forms.TextBox TextBoxCircuitHeightUpwind;
        internal System.Windows.Forms.TextBox TextBoxCircuitHeightBase;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}

