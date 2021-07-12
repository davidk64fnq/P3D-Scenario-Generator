
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
            this.TabPagePhoto = new System.Windows.Forms.TabPage();
            this.label18 = new System.Windows.Forms.Label();
            this.TextBoxPhotoHotspotRadius = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.TextBoxPhotoMaxBearingChange = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.TextBoxPhotoWindowSize = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.TextBoxPhotoMinLegDist = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.TextBoxPhotoMaxNoLegs = new System.Windows.Forms.TextBox();
            this.TextBoxPhotoMinNoLegs = new System.Windows.Forms.TextBox();
            this.ButtonPhotoTourDefault = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.TextBoxPhotoMaxLegDist = new System.Windows.Forms.TextBox();
            this.TabPageSign = new System.Windows.Forms.TabPage();
            this.PictureBoxSignWriting = new System.Windows.Forms.PictureBox();
            this.TextBoxSignWindowWidth = new System.Windows.Forms.TextBox();
            this.TextBoxSignFont = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.TextBoxSignTilt = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.TextBoxSignMessage = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.ButtonGenerateScenario = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.ButtonHelp = new System.Windows.Forms.Button();
            this.TabControl.SuspendLayout();
            this.TabPageGeneral.SuspendLayout();
            this.TabPageCircuit.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxCircuit)).BeginInit();
            this.TabPagePhoto.SuspendLayout();
            this.TabPageSign.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxSignWriting)).BeginInit();
            this.SuspendLayout();
            // 
            // TabControl
            // 
            this.TabControl.Controls.Add(this.TabPageGeneral);
            this.TabControl.Controls.Add(this.TabPageCircuit);
            this.TabControl.Controls.Add(this.TabPagePhoto);
            this.TabControl.Controls.Add(this.TabPageSign);
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
            this.label1.Location = new System.Drawing.Point(16, 144);
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
            this.TextBoxCircuitUpwind.Text = "1";
            this.TextBoxCircuitUpwind.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitUpwind, "Distance between runway and gate 1 in miles");
            this.TextBoxCircuitUpwind.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxDouble_Validating);
            // 
            // TextBoxCircuitHeightUpwind
            // 
            this.TextBoxCircuitHeightUpwind.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.TextBoxCircuitHeightUpwind.Location = new System.Drawing.Point(342, 32);
            this.TextBoxCircuitHeightUpwind.Name = "TextBoxCircuitHeightUpwind";
            this.TextBoxCircuitHeightUpwind.Size = new System.Drawing.Size(100, 23);
            this.TextBoxCircuitHeightUpwind.TabIndex = 13;
            this.TextBoxCircuitHeightUpwind.Text = "500";
            this.TextBoxCircuitHeightUpwind.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitHeightUpwind, "Height of circuit above runway in feet (gate 1)");
            this.TextBoxCircuitHeightUpwind.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxDouble_Validating);
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
            this.TextBoxCircuitBase.Text = "0.5";
            this.TextBoxCircuitBase.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitBase, "Distance between gates 2 and 3 (6 and 7) in miles");
            this.TextBoxCircuitBase.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxDouble_Validating);
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
            this.TextBoxCircuitFinal.Text = "1";
            this.TextBoxCircuitFinal.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitFinal, "Distance between gate 8 and runway in miles");
            this.TextBoxCircuitFinal.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxDouble_Validating);
            // 
            // TextBoxCircuitHeightDown
            // 
            this.TextBoxCircuitHeightDown.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.TextBoxCircuitHeightDown.Location = new System.Drawing.Point(461, 32);
            this.TextBoxCircuitHeightDown.Name = "TextBoxCircuitHeightDown";
            this.TextBoxCircuitHeightDown.Size = new System.Drawing.Size(100, 23);
            this.TextBoxCircuitHeightDown.TabIndex = 3;
            this.TextBoxCircuitHeightDown.Text = "1000";
            this.TextBoxCircuitHeightDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitHeightDown, "Height of circuit above runway in feet (gates 3 to 6)");
            this.TextBoxCircuitHeightDown.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxDouble_Validating);
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
            this.TextBoxCircuitSpeed.Text = "65";
            this.TextBoxCircuitSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitSpeed, "Cruise speed between gates 1 and 8 in knots");
            this.TextBoxCircuitSpeed.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxDouble_Validating);
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
            this.TextBoxCircuitHeightBase.Text = "500";
            this.TextBoxCircuitHeightBase.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitHeightBase, "Height of circuit above runway in feet (gate 8)");
            this.TextBoxCircuitHeightBase.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxDouble_Validating);
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
            // TabPagePhoto
            // 
            this.TabPagePhoto.Controls.Add(this.label18);
            this.TabPagePhoto.Controls.Add(this.TextBoxPhotoHotspotRadius);
            this.TabPagePhoto.Controls.Add(this.label17);
            this.TabPagePhoto.Controls.Add(this.TextBoxPhotoMaxBearingChange);
            this.TabPagePhoto.Controls.Add(this.label15);
            this.TabPagePhoto.Controls.Add(this.TextBoxPhotoWindowSize);
            this.TabPagePhoto.Controls.Add(this.label14);
            this.TabPagePhoto.Controls.Add(this.TextBoxPhotoMinLegDist);
            this.TabPagePhoto.Controls.Add(this.label13);
            this.TabPagePhoto.Controls.Add(this.label12);
            this.TabPagePhoto.Controls.Add(this.TextBoxPhotoMaxNoLegs);
            this.TabPagePhoto.Controls.Add(this.TextBoxPhotoMinNoLegs);
            this.TabPagePhoto.Controls.Add(this.ButtonPhotoTourDefault);
            this.TabPagePhoto.Controls.Add(this.label11);
            this.TabPagePhoto.Controls.Add(this.TextBoxPhotoMaxLegDist);
            this.TabPagePhoto.Location = new System.Drawing.Point(4, 24);
            this.TabPagePhoto.Name = "TabPagePhoto";
            this.TabPagePhoto.Size = new System.Drawing.Size(812, 438);
            this.TabPagePhoto.TabIndex = 2;
            this.TabPagePhoto.Text = "Photo Tour";
            this.TabPagePhoto.UseVisualStyleBackColor = true;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(16, 314);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(88, 15);
            this.label18.TabIndex = 26;
            this.label18.Text = "Hotspot Radius";
            // 
            // TextBoxPhotoHotspotRadius
            // 
            this.TextBoxPhotoHotspotRadius.Location = new System.Drawing.Point(148, 306);
            this.TextBoxPhotoHotspotRadius.Name = "TextBoxPhotoHotspotRadius";
            this.TextBoxPhotoHotspotRadius.Size = new System.Drawing.Size(119, 23);
            this.TextBoxPhotoHotspotRadius.TabIndex = 25;
            this.TextBoxPhotoHotspotRadius.Text = "1000";
            this.TextBoxPhotoHotspotRadius.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxPhotoHotspotRadius, "Radius of photo hotspot location in feet");
            this.TextBoxPhotoHotspotRadius.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxInteger_Validating);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(16, 266);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(117, 15);
            this.label17.TabIndex = 24;
            this.label17.Text = "Max Bearing Change";
            // 
            // TextBoxPhotoMaxBearingChange
            // 
            this.TextBoxPhotoMaxBearingChange.Location = new System.Drawing.Point(148, 258);
            this.TextBoxPhotoMaxBearingChange.Name = "TextBoxPhotoMaxBearingChange";
            this.TextBoxPhotoMaxBearingChange.Size = new System.Drawing.Size(119, 23);
            this.TextBoxPhotoMaxBearingChange.TabIndex = 23;
            this.TextBoxPhotoMaxBearingChange.Text = "135";
            this.TextBoxPhotoMaxBearingChange.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxPhotoMaxBearingChange, "Maximum bearing change left or right each leg in degrees");
            this.TextBoxPhotoMaxBearingChange.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxInteger_Validating);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(16, 221);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(74, 15);
            this.label15.TabIndex = 21;
            this.label15.Text = "Window Size";
            // 
            // TextBoxPhotoWindowSize
            // 
            this.TextBoxPhotoWindowSize.Location = new System.Drawing.Point(148, 213);
            this.TextBoxPhotoWindowSize.Name = "TextBoxPhotoWindowSize";
            this.TextBoxPhotoWindowSize.Size = new System.Drawing.Size(119, 23);
            this.TextBoxPhotoWindowSize.TabIndex = 19;
            this.TextBoxPhotoWindowSize.Text = "500";
            this.TextBoxPhotoWindowSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxPhotoWindowSize, "Size of leg route window in pixels");
            this.TextBoxPhotoWindowSize.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxInteger_Validating);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(16, 34);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(73, 15);
            this.label14.TabIndex = 18;
            this.label14.Text = "Min Leg Dist";
            // 
            // TextBoxPhotoMinLegDist
            // 
            this.TextBoxPhotoMinLegDist.Location = new System.Drawing.Point(148, 26);
            this.TextBoxPhotoMinLegDist.Name = "TextBoxPhotoMinLegDist";
            this.TextBoxPhotoMinLegDist.Size = new System.Drawing.Size(119, 23);
            this.TextBoxPhotoMinLegDist.TabIndex = 17;
            this.TextBoxPhotoMinLegDist.Text = "3";
            this.TextBoxPhotoMinLegDist.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxPhotoMinLegDist, "Minimum leg distance in miles to next photo");
            this.TextBoxPhotoMinLegDist.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxDouble_Validating);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(16, 174);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(79, 15);
            this.label13.TabIndex = 16;
            this.label13.Text = "Max No. Legs";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(16, 126);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(77, 15);
            this.label12.TabIndex = 15;
            this.label12.Text = "Min No. Legs";
            // 
            // TextBoxPhotoMaxNoLegs
            // 
            this.TextBoxPhotoMaxNoLegs.Location = new System.Drawing.Point(148, 166);
            this.TextBoxPhotoMaxNoLegs.Name = "TextBoxPhotoMaxNoLegs";
            this.TextBoxPhotoMaxNoLegs.Size = new System.Drawing.Size(119, 23);
            this.TextBoxPhotoMaxNoLegs.TabIndex = 14;
            this.TextBoxPhotoMaxNoLegs.Text = "7";
            this.TextBoxPhotoMaxNoLegs.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxPhotoMaxNoLegs, "Maximum leg distance in miles to next photo");
            this.TextBoxPhotoMaxNoLegs.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxInteger_Validating);
            // 
            // TextBoxPhotoMinNoLegs
            // 
            this.TextBoxPhotoMinNoLegs.Location = new System.Drawing.Point(148, 118);
            this.TextBoxPhotoMinNoLegs.Name = "TextBoxPhotoMinNoLegs";
            this.TextBoxPhotoMinNoLegs.Size = new System.Drawing.Size(119, 23);
            this.TextBoxPhotoMinNoLegs.TabIndex = 13;
            this.TextBoxPhotoMinNoLegs.Text = "3";
            this.TextBoxPhotoMinNoLegs.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxPhotoMinNoLegs, "Maximum leg distance in miles to next photo");
            this.TextBoxPhotoMinNoLegs.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxInteger_Validating);
            // 
            // ButtonPhotoTourDefault
            // 
            this.ButtonPhotoTourDefault.Location = new System.Drawing.Point(469, 15);
            this.ButtonPhotoTourDefault.Name = "ButtonPhotoTourDefault";
            this.ButtonPhotoTourDefault.Size = new System.Drawing.Size(75, 23);
            this.ButtonPhotoTourDefault.TabIndex = 12;
            this.ButtonPhotoTourDefault.Text = "Default";
            this.ButtonPhotoTourDefault.UseVisualStyleBackColor = true;
            this.ButtonPhotoTourDefault.Click += new System.EventHandler(this.ButtonPhotoTourDefault_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(16, 79);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(75, 15);
            this.label11.TabIndex = 4;
            this.label11.Text = "Max Leg Dist";
            // 
            // TextBoxPhotoMaxLegDist
            // 
            this.TextBoxPhotoMaxLegDist.Location = new System.Drawing.Point(148, 71);
            this.TextBoxPhotoMaxLegDist.Name = "TextBoxPhotoMaxLegDist";
            this.TextBoxPhotoMaxLegDist.Size = new System.Drawing.Size(119, 23);
            this.TextBoxPhotoMaxLegDist.TabIndex = 3;
            this.TextBoxPhotoMaxLegDist.Text = "10";
            this.TextBoxPhotoMaxLegDist.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxPhotoMaxLegDist, "Maximum leg distance in miles to next photo");
            this.TextBoxPhotoMaxLegDist.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxDouble_Validating);
            // 
            // TabPageSign
            // 
            this.TabPageSign.Controls.Add(this.PictureBoxSignWriting);
            this.TabPageSign.Controls.Add(this.TextBoxSignWindowWidth);
            this.TabPageSign.Controls.Add(this.TextBoxSignFont);
            this.TabPageSign.Controls.Add(this.label21);
            this.TabPageSign.Controls.Add(this.label20);
            this.TabPageSign.Controls.Add(this.TextBoxSignTilt);
            this.TabPageSign.Controls.Add(this.label19);
            this.TabPageSign.Controls.Add(this.TextBoxSignMessage);
            this.TabPageSign.Controls.Add(this.label16);
            this.TabPageSign.Location = new System.Drawing.Point(4, 24);
            this.TabPageSign.Name = "TabPageSign";
            this.TabPageSign.Size = new System.Drawing.Size(812, 438);
            this.TabPageSign.TabIndex = 3;
            this.TabPageSign.Text = "Sign Writing";
            this.TabPageSign.UseVisualStyleBackColor = true;
            // 
            // PictureBoxSignWriting
            // 
            this.PictureBoxSignWriting.Location = new System.Drawing.Point(279, 71);
            this.PictureBoxSignWriting.Name = "PictureBoxSignWriting";
            this.PictureBoxSignWriting.Size = new System.Drawing.Size(500, 241);
            this.PictureBoxSignWriting.TabIndex = 8;
            this.PictureBoxSignWriting.TabStop = false;
            // 
            // TextBoxSignWindowWidth
            // 
            this.TextBoxSignWindowWidth.Location = new System.Drawing.Point(148, 166);
            this.TextBoxSignWindowWidth.Name = "TextBoxSignWindowWidth";
            this.TextBoxSignWindowWidth.Size = new System.Drawing.Size(100, 23);
            this.TextBoxSignWindowWidth.TabIndex = 7;
            this.TextBoxSignWindowWidth.Text = "1000";
            this.TextBoxSignWindowWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextBoxSignWindowWidth.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxInteger_Validating);
            // 
            // TextBoxSignFont
            // 
            this.TextBoxSignFont.Enabled = false;
            this.TextBoxSignFont.Location = new System.Drawing.Point(148, 118);
            this.TextBoxSignFont.Name = "TextBoxSignFont";
            this.TextBoxSignFont.Size = new System.Drawing.Size(100, 23);
            this.TextBoxSignFont.TabIndex = 6;
            this.TextBoxSignFont.Text = "Segment 22";
            this.TextBoxSignFont.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(16, 175);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(86, 15);
            this.label21.TabIndex = 5;
            this.label21.Text = "Window Width";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(16, 126);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(31, 15);
            this.label20.TabIndex = 4;
            this.label20.Text = "Font";
            // 
            // TextBoxSignTilt
            // 
            this.TextBoxSignTilt.Location = new System.Drawing.Point(148, 71);
            this.TextBoxSignTilt.Name = "TextBoxSignTilt";
            this.TextBoxSignTilt.Size = new System.Drawing.Size(100, 23);
            this.TextBoxSignTilt.TabIndex = 3;
            this.TextBoxSignTilt.Text = "10";
            this.TextBoxSignTilt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextBoxSignTilt.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxInteger_Validating);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(16, 79);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(57, 15);
            this.label19.TabIndex = 2;
            this.label19.Text = "Tilt Angle";
            this.toolTip1.SetToolTip(this.label19, "Tilt Angle in degrees from horizontal");
            // 
            // TextBoxSignMessage
            // 
            this.TextBoxSignMessage.Location = new System.Drawing.Point(148, 26);
            this.TextBoxSignMessage.Name = "TextBoxSignMessage";
            this.TextBoxSignMessage.Size = new System.Drawing.Size(631, 23);
            this.TextBoxSignMessage.TabIndex = 1;
            this.TextBoxSignMessage.Text = "A";
            this.toolTip1.SetToolTip(this.TextBoxSignMessage, "Message consisting only of alphabetic characters");
            this.TextBoxSignMessage.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxString_Validating);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(16, 34);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(53, 15);
            this.label16.TabIndex = 0;
            this.label16.Text = "Message";
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
            this.TabPagePhoto.ResumeLayout(false);
            this.TabPagePhoto.PerformLayout();
            this.TabPageSign.ResumeLayout(false);
            this.TabPageSign.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxSignWriting)).EndInit();
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
        private System.Windows.Forms.TabPage TabPagePhoto;
        private System.Windows.Forms.Label label11;
        internal System.Windows.Forms.TextBox TextBoxPhotoMaxLegDist;
        private System.Windows.Forms.Button ButtonPhotoTourDefault;
        internal System.Windows.Forms.TextBox TextBoxPhotoMaxNoLegs;
        internal System.Windows.Forms.TextBox TextBoxPhotoMinNoLegs;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label14;
        internal System.Windows.Forms.TextBox TextBoxPhotoMinLegDist;
        private System.Windows.Forms.Label label17;
        internal System.Windows.Forms.TextBox TextBoxPhotoMaxBearingChange;
        private System.Windows.Forms.Label label15;
        internal System.Windows.Forms.TextBox TextBoxPhotoWindowSize;
        private System.Windows.Forms.Label label18;
        internal System.Windows.Forms.TextBox TextBoxPhotoHotspotRadius;
        private System.Windows.Forms.TabPage TabPageSign;
        internal System.Windows.Forms.TextBox TextBoxSignMessage;
        private System.Windows.Forms.Label label16;
        internal System.Windows.Forms.TextBox TextBoxSignTilt;
        private System.Windows.Forms.Label label19;
        internal System.Windows.Forms.TextBox TextBoxSignWindowWidth;
        internal System.Windows.Forms.TextBox TextBoxSignFont;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.PictureBox PictureBoxSignWriting;
    }
}

