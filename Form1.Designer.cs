
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
            components = new System.ComponentModel.Container();
            TabControl = new TabControl();
            TabPageGeneral = new TabPage();
            button1 = new Button();
            TextBoxP3Dv5Files = new TextBox();
            label27 = new Label();
            TimePicker = new DateTimePicker();
            DatePicker = new DateTimePicker();
            label8 = new Label();
            TextBoxScenarioTitle = new TextBox();
            ListBoxAircraft = new ListBox();
            buttonAircraft = new Button();
            TextBoxSelectedScenario = new TextBox();
            ListBoxScenarioType = new ListBox();
            ButtonRandRunway = new Button();
            label2 = new Label();
            label1 = new Label();
            TextBoxSearchRunway = new TextBox();
            TextBoxSelectedRunway = new TextBox();
            ListBoxRunways = new ListBox();
            TabPageCircuit = new TabPage();
            tableLayoutPanelCircuit = new TableLayoutPanel();
            label7 = new Label();
            label6 = new Label();
            TextBoxCircuitUpwind = new TextBox();
            label9 = new Label();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label10 = new Label();
            TextBoxCircuitHeightBase = new TextBox();
            label28 = new Label();
            TextBoxCircuitBase = new TextBox();
            TextBoxCircuitFinal = new TextBox();
            TextBoxCircuitHeightUpwind = new TextBox();
            TextBoxCircuitHeightDown = new TextBox();
            TextBoxCircuitTurnRate = new TextBox();
            TextBoxCircuitSpeed = new TextBox();
            ButtonCircuitDefault = new Button();
            PictureBoxCircuit = new PictureBox();
            TabPagePhoto = new TabPage();
            label18 = new Label();
            TextBoxPhotoHotspotRadius = new TextBox();
            label17 = new Label();
            TextBoxPhotoMaxBearingChange = new TextBox();
            label15 = new Label();
            TextBoxPhotoWindowSize = new TextBox();
            label14 = new Label();
            TextBoxPhotoMinLegDist = new TextBox();
            label13 = new Label();
            label12 = new Label();
            TextBoxPhotoMaxNoLegs = new TextBox();
            TextBoxPhotoMinNoLegs = new TextBox();
            ButtonPhotoTourDefault = new Button();
            label11 = new Label();
            TextBoxPhotoMaxLegDist = new TextBox();
            TabPageSign = new TabPage();
            TextBoxSignSegmentRadius = new TextBox();
            TextBoxSignSegmentLength = new TextBox();
            label24 = new Label();
            label23 = new Label();
            TextBoxSignGateHeight = new TextBox();
            label22 = new Label();
            PictureBoxSignWriting = new PictureBox();
            TextBoxSignWindowWidth = new TextBox();
            TextBoxSignFont = new TextBox();
            label21 = new Label();
            label20 = new Label();
            TextBoxSignTilt = new TextBox();
            label19 = new Label();
            TextBoxSignMessage = new TextBox();
            label16 = new Label();
            TabPageCelestial = new TabPage();
            TextBoxCelestialMaxDist = new TextBox();
            TextBoxCelestialMinDist = new TextBox();
            label26 = new Label();
            label25 = new Label();
            ButtonGenerateScenario = new Button();
            toolTip1 = new ToolTip(components);
            ButtonHelp = new Button();
            TabControl.SuspendLayout();
            TabPageGeneral.SuspendLayout();
            TabPageCircuit.SuspendLayout();
            tableLayoutPanelCircuit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBoxCircuit).BeginInit();
            TabPagePhoto.SuspendLayout();
            TabPageSign.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBoxSignWriting).BeginInit();
            TabPageCelestial.SuspendLayout();
            SuspendLayout();
            // 
            // TabControl
            // 
            TabControl.Controls.Add(TabPageGeneral);
            TabControl.Controls.Add(TabPageCircuit);
            TabControl.Controls.Add(TabPagePhoto);
            TabControl.Controls.Add(TabPageSign);
            TabControl.Controls.Add(TabPageCelestial);
            TabControl.Location = new Point(12, 12);
            TabControl.Name = "TabControl";
            TabControl.SelectedIndex = 0;
            TabControl.Size = new Size(820, 466);
            TabControl.TabIndex = 0;
            TabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            // 
            // TabPageGeneral
            // 
            TabPageGeneral.Controls.Add(button1);
            TabPageGeneral.Controls.Add(TextBoxP3Dv5Files);
            TabPageGeneral.Controls.Add(label27);
            TabPageGeneral.Controls.Add(TimePicker);
            TabPageGeneral.Controls.Add(DatePicker);
            TabPageGeneral.Controls.Add(label8);
            TabPageGeneral.Controls.Add(TextBoxScenarioTitle);
            TabPageGeneral.Controls.Add(ListBoxAircraft);
            TabPageGeneral.Controls.Add(buttonAircraft);
            TabPageGeneral.Controls.Add(TextBoxSelectedScenario);
            TabPageGeneral.Controls.Add(ListBoxScenarioType);
            TabPageGeneral.Controls.Add(ButtonRandRunway);
            TabPageGeneral.Controls.Add(label2);
            TabPageGeneral.Controls.Add(label1);
            TabPageGeneral.Controls.Add(TextBoxSearchRunway);
            TabPageGeneral.Controls.Add(TextBoxSelectedRunway);
            TabPageGeneral.Controls.Add(ListBoxRunways);
            TabPageGeneral.Location = new Point(4, 24);
            TabPageGeneral.Name = "TabPageGeneral";
            TabPageGeneral.Padding = new Padding(3);
            TabPageGeneral.Size = new Size(812, 438);
            TabPageGeneral.TabIndex = 0;
            TabPageGeneral.Text = "General";
            TabPageGeneral.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Location = new Point(343, 399);
            button1.Name = "button1";
            button1.Size = new Size(120, 23);
            button1.TabIndex = 19;
            button1.Text = "Prepar3D v5 Files";
            toolTip1.SetToolTip(button1, "Select the Prepar3D v5 Files location where scenario will be stored");
            button1.UseVisualStyleBackColor = true;
            button1.Click += ButtonP3Dv5Files_Click;
            // 
            // TextBoxP3Dv5Files
            // 
            TextBoxP3Dv5Files.Enabled = false;
            TextBoxP3Dv5Files.Location = new Point(488, 399);
            TextBoxP3Dv5Files.Name = "TextBoxP3Dv5Files";
            TextBoxP3Dv5Files.Size = new Size(302, 23);
            TextBoxP3Dv5Files.TabIndex = 18;
            // 
            // label27
            // 
            label27.Location = new Point(0, 0);
            label27.Name = "label27";
            label27.Size = new Size(100, 23);
            label27.TabIndex = 0;
            // 
            // TimePicker
            // 
            TimePicker.Format = DateTimePickerFormat.Time;
            TimePicker.Location = new Point(112, 303);
            TimePicker.Name = "TimePicker";
            TimePicker.ShowUpDown = true;
            TimePicker.Size = new Size(119, 23);
            TimePicker.TabIndex = 16;
            // 
            // DatePicker
            // 
            DatePicker.CustomFormat = "";
            DatePicker.Location = new Point(112, 350);
            DatePicker.Name = "DatePicker";
            DatePicker.Size = new Size(206, 23);
            DatePicker.TabIndex = 15;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(16, 402);
            label8.Name = "label8";
            label8.Size = new Size(77, 15);
            label8.TabIndex = 14;
            label8.Text = "Scenario Title";
            // 
            // TextBoxScenarioTitle
            // 
            TextBoxScenarioTitle.Location = new Point(112, 399);
            TextBoxScenarioTitle.Name = "TextBoxScenarioTitle";
            TextBoxScenarioTitle.Size = new Size(206, 23);
            TextBoxScenarioTitle.TabIndex = 13;
            // 
            // ListBoxAircraft
            // 
            ListBoxAircraft.FormattingEnabled = true;
            ListBoxAircraft.ItemHeight = 15;
            ListBoxAircraft.Location = new Point(488, 24);
            ListBoxAircraft.Name = "ListBoxAircraft";
            ListBoxAircraft.Size = new Size(250, 94);
            ListBoxAircraft.TabIndex = 12;
            // 
            // buttonAircraft
            // 
            buttonAircraft.Location = new Point(488, 135);
            buttonAircraft.Name = "buttonAircraft";
            buttonAircraft.Size = new Size(120, 23);
            buttonAircraft.TabIndex = 11;
            buttonAircraft.Text = "aircraft.cfg";
            toolTip1.SetToolTip(buttonAircraft, "Select the \"aircraft.cfg\" file for your aircraft and then the variation from the list above");
            buttonAircraft.UseVisualStyleBackColor = true;
            buttonAircraft.Click += ButtonAircraft_Click;
            // 
            // TextBoxSelectedScenario
            // 
            TextBoxSelectedScenario.Enabled = false;
            TextBoxSelectedScenario.Location = new Point(300, 136);
            TextBoxSelectedScenario.Name = "TextBoxSelectedScenario";
            TextBoxSelectedScenario.Size = new Size(119, 23);
            TextBoxSelectedScenario.TabIndex = 7;
            // 
            // ListBoxScenarioType
            // 
            ListBoxScenarioType.FormattingEnabled = true;
            ListBoxScenarioType.ItemHeight = 15;
            ListBoxScenarioType.Location = new Point(300, 24);
            ListBoxScenarioType.Name = "ListBoxScenarioType";
            ListBoxScenarioType.Size = new Size(120, 94);
            ListBoxScenarioType.TabIndex = 6;
            ListBoxScenarioType.SelectedIndexChanged += ListBoxScenarioType_SelectedIndexChanged;
            // 
            // ButtonRandRunway
            // 
            ButtonRandRunway.Location = new Point(112, 233);
            ButtonRandRunway.Name = "ButtonRandRunway";
            ButtonRandRunway.Size = new Size(119, 23);
            ButtonRandRunway.TabIndex = 5;
            ButtonRandRunway.Text = "Random Runway";
            ButtonRandRunway.UseVisualStyleBackColor = true;
            ButtonRandRunway.Click += ButtonRandRunway_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(16, 191);
            label2.Name = "label2";
            label2.Size = new Size(51, 15);
            label2.TabIndex = 4;
            label2.Text = "Selected";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 144);
            label1.Name = "label1";
            label1.Size = new Size(42, 15);
            label1.TabIndex = 3;
            label1.Text = "Search";
            // 
            // TextBoxSearchRunway
            // 
            TextBoxSearchRunway.Location = new Point(112, 136);
            TextBoxSearchRunway.Name = "TextBoxSearchRunway";
            TextBoxSearchRunway.Size = new Size(119, 23);
            TextBoxSearchRunway.TabIndex = 2;
            TextBoxSearchRunway.TextChanged += TextBoxSearchRunway_TextChanged;
            // 
            // TextBoxSelectedRunway
            // 
            TextBoxSelectedRunway.Enabled = false;
            TextBoxSelectedRunway.Location = new Point(112, 183);
            TextBoxSelectedRunway.Name = "TextBoxSelectedRunway";
            TextBoxSelectedRunway.Size = new Size(119, 23);
            TextBoxSelectedRunway.TabIndex = 1;
            // 
            // ListBoxRunways
            // 
            ListBoxRunways.FormattingEnabled = true;
            ListBoxRunways.ItemHeight = 15;
            ListBoxRunways.Location = new Point(112, 24);
            ListBoxRunways.Name = "ListBoxRunways";
            ListBoxRunways.Size = new Size(120, 94);
            ListBoxRunways.TabIndex = 0;
            ListBoxRunways.SelectedIndexChanged += ListBoxRunways_SelectedIndexChanged;
            // 
            // TabPageCircuit
            // 
            TabPageCircuit.Controls.Add(tableLayoutPanelCircuit);
            TabPageCircuit.Controls.Add(ButtonCircuitDefault);
            TabPageCircuit.Controls.Add(PictureBoxCircuit);
            TabPageCircuit.Location = new Point(4, 24);
            TabPageCircuit.Name = "TabPageCircuit";
            TabPageCircuit.Padding = new Padding(3);
            TabPageCircuit.Size = new Size(812, 438);
            TabPageCircuit.TabIndex = 1;
            TabPageCircuit.Text = "Circuit";
            TabPageCircuit.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanelCircuit
            // 
            tableLayoutPanelCircuit.Anchor = AnchorStyles.None;
            tableLayoutPanelCircuit.ColumnCount = 8;
            tableLayoutPanelCircuit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanelCircuit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanelCircuit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanelCircuit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanelCircuit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanelCircuit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanelCircuit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanelCircuit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanelCircuit.Controls.Add(label7, 0, 0);
            tableLayoutPanelCircuit.Controls.Add(label6, 6, 0);
            tableLayoutPanelCircuit.Controls.Add(TextBoxCircuitUpwind, 0, 1);
            tableLayoutPanelCircuit.Controls.Add(label9, 5, 0);
            tableLayoutPanelCircuit.Controls.Add(label5, 4, 0);
            tableLayoutPanelCircuit.Controls.Add(label4, 2, 0);
            tableLayoutPanelCircuit.Controls.Add(label3, 1, 0);
            tableLayoutPanelCircuit.Controls.Add(label10, 3, 0);
            tableLayoutPanelCircuit.Controls.Add(TextBoxCircuitHeightBase, 5, 1);
            tableLayoutPanelCircuit.Controls.Add(label28, 7, 0);
            tableLayoutPanelCircuit.Controls.Add(TextBoxCircuitBase, 1, 1);
            tableLayoutPanelCircuit.Controls.Add(TextBoxCircuitFinal, 2, 1);
            tableLayoutPanelCircuit.Controls.Add(TextBoxCircuitHeightUpwind, 3, 1);
            tableLayoutPanelCircuit.Controls.Add(TextBoxCircuitHeightDown, 4, 1);
            tableLayoutPanelCircuit.Controls.Add(TextBoxCircuitTurnRate, 7, 1);
            tableLayoutPanelCircuit.Controls.Add(TextBoxCircuitSpeed, 6, 1);
            tableLayoutPanelCircuit.Location = new Point(15, 361);
            tableLayoutPanelCircuit.Margin = new Padding(0);
            tableLayoutPanelCircuit.Name = "tableLayoutPanelCircuit";
            tableLayoutPanelCircuit.RowCount = 2;
            tableLayoutPanelCircuit.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanelCircuit.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanelCircuit.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelCircuit.Size = new Size(783, 58);
            tableLayoutPanelCircuit.TabIndex = 17;
            // 
            // label7
            // 
            label7.AllowDrop = true;
            label7.Anchor = AnchorStyles.None;
            label7.AutoSize = true;
            label7.Location = new Point(24, 7);
            label7.Name = "label7";
            label7.Size = new Size(48, 15);
            label7.TabIndex = 10;
            label7.Text = "Upwind";
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.None;
            label6.AutoSize = true;
            label6.Location = new Point(611, 7);
            label6.Name = "label6";
            label6.Size = new Size(39, 15);
            label6.TabIndex = 9;
            label6.Text = "Speed";
            // 
            // TextBoxCircuitUpwind
            // 
            TextBoxCircuitUpwind.Anchor = AnchorStyles.None;
            TextBoxCircuitUpwind.Location = new Point(8, 32);
            TextBoxCircuitUpwind.Name = "TextBoxCircuitUpwind";
            TextBoxCircuitUpwind.Size = new Size(80, 23);
            TextBoxCircuitUpwind.TabIndex = 5;
            TextBoxCircuitUpwind.Text = "1";
            TextBoxCircuitUpwind.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxCircuitUpwind, "Distance between runway and gate 1 in miles");
            TextBoxCircuitUpwind.Validating += TextBoxDouble_Validating;
            // 
            // label9
            // 
            label9.Anchor = AnchorStyles.None;
            label9.AutoSize = true;
            label9.Location = new Point(490, 7);
            label9.Name = "label9";
            label9.Size = new Size(87, 15);
            label9.TabIndex = 15;
            label9.Text = "Height (Gate 8)";
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.None;
            label5.AutoSize = true;
            label5.Location = new Point(395, 7);
            label5.Name = "label5";
            label5.Size = new Size(83, 15);
            label5.TabIndex = 8;
            label5.Text = "Ht. (Gates 3-6)";
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.None;
            label4.AutoSize = true;
            label4.Location = new Point(226, 7);
            label4.Name = "label4";
            label4.Size = new Size(32, 15);
            label4.TabIndex = 7;
            label4.Text = "Final";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.None;
            label3.AutoSize = true;
            label3.Location = new Point(130, 7);
            label3.Name = "label3";
            label3.Size = new Size(31, 15);
            label3.TabIndex = 6;
            label3.Text = "Base";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label10
            // 
            label10.Anchor = AnchorStyles.None;
            label10.AutoSize = true;
            label10.Location = new Point(296, 7);
            label10.Name = "label10";
            label10.Size = new Size(87, 15);
            label10.TabIndex = 16;
            label10.Text = "Height (Gate 1)";
            // 
            // TextBoxCircuitHeightBase
            // 
            TextBoxCircuitHeightBase.Anchor = AnchorStyles.None;
            TextBoxCircuitHeightBase.Location = new Point(493, 32);
            TextBoxCircuitHeightBase.Name = "TextBoxCircuitHeightBase";
            TextBoxCircuitHeightBase.Size = new Size(80, 23);
            TextBoxCircuitHeightBase.TabIndex = 12;
            TextBoxCircuitHeightBase.Text = "500";
            TextBoxCircuitHeightBase.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxCircuitHeightBase, "Height of circuit above runway in feet (gate 8)");
            TextBoxCircuitHeightBase.Validating += TextBoxDouble_Validating;
            // 
            // label28
            // 
            label28.Anchor = AnchorStyles.None;
            label28.AutoSize = true;
            label28.Location = new Point(702, 7);
            label28.Name = "label28";
            label28.Size = new Size(57, 15);
            label28.TabIndex = 18;
            label28.Text = "Turn Rate";
            // 
            // TextBoxCircuitBase
            // 
            TextBoxCircuitBase.Anchor = AnchorStyles.None;
            TextBoxCircuitBase.Location = new Point(105, 32);
            TextBoxCircuitBase.Name = "TextBoxCircuitBase";
            TextBoxCircuitBase.Size = new Size(80, 23);
            TextBoxCircuitBase.TabIndex = 1;
            TextBoxCircuitBase.Text = "0.5";
            TextBoxCircuitBase.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxCircuitBase, "Distance between gates 2 and 3 (6 and 7) in miles");
            TextBoxCircuitBase.Validating += TextBoxDouble_Validating;
            // 
            // TextBoxCircuitFinal
            // 
            TextBoxCircuitFinal.Anchor = AnchorStyles.None;
            TextBoxCircuitFinal.Location = new Point(202, 32);
            TextBoxCircuitFinal.Name = "TextBoxCircuitFinal";
            TextBoxCircuitFinal.Size = new Size(80, 23);
            TextBoxCircuitFinal.TabIndex = 2;
            TextBoxCircuitFinal.Text = "1";
            TextBoxCircuitFinal.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxCircuitFinal, "Distance between gate 8 and runway in miles");
            TextBoxCircuitFinal.Validating += TextBoxDouble_Validating;
            // 
            // TextBoxCircuitHeightUpwind
            // 
            TextBoxCircuitHeightUpwind.Anchor = AnchorStyles.None;
            TextBoxCircuitHeightUpwind.Location = new Point(299, 32);
            TextBoxCircuitHeightUpwind.Name = "TextBoxCircuitHeightUpwind";
            TextBoxCircuitHeightUpwind.Size = new Size(80, 23);
            TextBoxCircuitHeightUpwind.TabIndex = 13;
            TextBoxCircuitHeightUpwind.Text = "500";
            TextBoxCircuitHeightUpwind.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxCircuitHeightUpwind, "Height of circuit above runway in feet (gate 1)");
            TextBoxCircuitHeightUpwind.Validating += TextBoxDouble_Validating;
            // 
            // TextBoxCircuitHeightDown
            // 
            TextBoxCircuitHeightDown.Anchor = AnchorStyles.None;
            TextBoxCircuitHeightDown.Location = new Point(396, 32);
            TextBoxCircuitHeightDown.Name = "TextBoxCircuitHeightDown";
            TextBoxCircuitHeightDown.Size = new Size(80, 23);
            TextBoxCircuitHeightDown.TabIndex = 3;
            TextBoxCircuitHeightDown.Text = "1000";
            TextBoxCircuitHeightDown.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxCircuitHeightDown, "Height of circuit above runway in feet (gates 3 to 6)");
            TextBoxCircuitHeightDown.Validating += TextBoxDouble_Validating;
            // 
            // TextBoxCircuitTurnRate
            // 
            TextBoxCircuitTurnRate.Anchor = AnchorStyles.None;
            TextBoxCircuitTurnRate.Location = new Point(691, 32);
            TextBoxCircuitTurnRate.Name = "TextBoxCircuitTurnRate";
            TextBoxCircuitTurnRate.Size = new Size(80, 23);
            TextBoxCircuitTurnRate.TabIndex = 17;
            TextBoxCircuitTurnRate.Text = "4.0";
            TextBoxCircuitTurnRate.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxCircuitTurnRate, "360 degrees turn rate in minutes");
            // 
            // TextBoxCircuitSpeed
            // 
            TextBoxCircuitSpeed.Anchor = AnchorStyles.None;
            TextBoxCircuitSpeed.Location = new Point(590, 32);
            TextBoxCircuitSpeed.Name = "TextBoxCircuitSpeed";
            TextBoxCircuitSpeed.Size = new Size(80, 23);
            TextBoxCircuitSpeed.TabIndex = 4;
            TextBoxCircuitSpeed.Text = "65";
            TextBoxCircuitSpeed.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxCircuitSpeed, "Cruise speed between gates 1 and 8 in knots");
            TextBoxCircuitSpeed.Validating += TextBoxDouble_Validating;
            // 
            // ButtonCircuitDefault
            // 
            ButtonCircuitDefault.Location = new Point(469, 15);
            ButtonCircuitDefault.Name = "ButtonCircuitDefault";
            ButtonCircuitDefault.Size = new Size(75, 23);
            ButtonCircuitDefault.TabIndex = 11;
            ButtonCircuitDefault.Text = "Default";
            ButtonCircuitDefault.UseVisualStyleBackColor = true;
            ButtonCircuitDefault.Click += ButtonCircuitDefault_Click;
            // 
            // PictureBoxCircuit
            // 
            PictureBoxCircuit.Location = new Point(15, 15);
            PictureBoxCircuit.Name = "PictureBoxCircuit";
            PictureBoxCircuit.Size = new Size(783, 325);
            PictureBoxCircuit.TabIndex = 0;
            PictureBoxCircuit.TabStop = false;
            // 
            // TabPagePhoto
            // 
            TabPagePhoto.Controls.Add(label18);
            TabPagePhoto.Controls.Add(TextBoxPhotoHotspotRadius);
            TabPagePhoto.Controls.Add(label17);
            TabPagePhoto.Controls.Add(TextBoxPhotoMaxBearingChange);
            TabPagePhoto.Controls.Add(label15);
            TabPagePhoto.Controls.Add(TextBoxPhotoWindowSize);
            TabPagePhoto.Controls.Add(label14);
            TabPagePhoto.Controls.Add(TextBoxPhotoMinLegDist);
            TabPagePhoto.Controls.Add(label13);
            TabPagePhoto.Controls.Add(label12);
            TabPagePhoto.Controls.Add(TextBoxPhotoMaxNoLegs);
            TabPagePhoto.Controls.Add(TextBoxPhotoMinNoLegs);
            TabPagePhoto.Controls.Add(ButtonPhotoTourDefault);
            TabPagePhoto.Controls.Add(label11);
            TabPagePhoto.Controls.Add(TextBoxPhotoMaxLegDist);
            TabPagePhoto.Location = new Point(4, 24);
            TabPagePhoto.Name = "TabPagePhoto";
            TabPagePhoto.Size = new Size(812, 438);
            TabPagePhoto.TabIndex = 2;
            TabPagePhoto.Text = "Photo Tour";
            TabPagePhoto.UseVisualStyleBackColor = true;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(16, 314);
            label18.Name = "label18";
            label18.Size = new Size(88, 15);
            label18.TabIndex = 26;
            label18.Text = "Hotspot Radius";
            // 
            // TextBoxPhotoHotspotRadius
            // 
            TextBoxPhotoHotspotRadius.Location = new Point(148, 306);
            TextBoxPhotoHotspotRadius.Name = "TextBoxPhotoHotspotRadius";
            TextBoxPhotoHotspotRadius.Size = new Size(119, 23);
            TextBoxPhotoHotspotRadius.TabIndex = 25;
            TextBoxPhotoHotspotRadius.Text = "1000";
            TextBoxPhotoHotspotRadius.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxPhotoHotspotRadius, "Radius of photo hotspot location in feet");
            TextBoxPhotoHotspotRadius.Validating += TextBoxInteger_Validating;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(16, 266);
            label17.Name = "label17";
            label17.Size = new Size(117, 15);
            label17.TabIndex = 24;
            label17.Text = "Max Bearing Change";
            // 
            // TextBoxPhotoMaxBearingChange
            // 
            TextBoxPhotoMaxBearingChange.Location = new Point(148, 258);
            TextBoxPhotoMaxBearingChange.Name = "TextBoxPhotoMaxBearingChange";
            TextBoxPhotoMaxBearingChange.Size = new Size(119, 23);
            TextBoxPhotoMaxBearingChange.TabIndex = 23;
            TextBoxPhotoMaxBearingChange.Text = "135";
            TextBoxPhotoMaxBearingChange.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxPhotoMaxBearingChange, "Maximum bearing change left or right each leg in degrees");
            TextBoxPhotoMaxBearingChange.Validating += TextBoxInteger_Validating;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(16, 221);
            label15.Name = "label15";
            label15.Size = new Size(74, 15);
            label15.TabIndex = 21;
            label15.Text = "Window Size";
            // 
            // TextBoxPhotoWindowSize
            // 
            TextBoxPhotoWindowSize.Location = new Point(148, 213);
            TextBoxPhotoWindowSize.Name = "TextBoxPhotoWindowSize";
            TextBoxPhotoWindowSize.Size = new Size(119, 23);
            TextBoxPhotoWindowSize.TabIndex = 19;
            TextBoxPhotoWindowSize.Text = "500";
            TextBoxPhotoWindowSize.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxPhotoWindowSize, "Size of leg route window in pixels");
            TextBoxPhotoWindowSize.Validating += TextBoxInteger_Validating;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(16, 34);
            label14.Name = "label14";
            label14.Size = new Size(73, 15);
            label14.TabIndex = 18;
            label14.Text = "Min Leg Dist";
            // 
            // TextBoxPhotoMinLegDist
            // 
            TextBoxPhotoMinLegDist.Location = new Point(148, 26);
            TextBoxPhotoMinLegDist.Name = "TextBoxPhotoMinLegDist";
            TextBoxPhotoMinLegDist.Size = new Size(119, 23);
            TextBoxPhotoMinLegDist.TabIndex = 17;
            TextBoxPhotoMinLegDist.Text = "3";
            TextBoxPhotoMinLegDist.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxPhotoMinLegDist, "Minimum leg distance in miles to next photo");
            TextBoxPhotoMinLegDist.Validating += TextBoxDouble_Validating;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(16, 174);
            label13.Name = "label13";
            label13.Size = new Size(79, 15);
            label13.TabIndex = 16;
            label13.Text = "Max No. Legs";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(16, 126);
            label12.Name = "label12";
            label12.Size = new Size(77, 15);
            label12.TabIndex = 15;
            label12.Text = "Min No. Legs";
            // 
            // TextBoxPhotoMaxNoLegs
            // 
            TextBoxPhotoMaxNoLegs.Location = new Point(148, 166);
            TextBoxPhotoMaxNoLegs.Name = "TextBoxPhotoMaxNoLegs";
            TextBoxPhotoMaxNoLegs.Size = new Size(119, 23);
            TextBoxPhotoMaxNoLegs.TabIndex = 14;
            TextBoxPhotoMaxNoLegs.Text = "7";
            TextBoxPhotoMaxNoLegs.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxPhotoMaxNoLegs, "Maximum leg distance in miles to next photo");
            TextBoxPhotoMaxNoLegs.Validating += TextBoxInteger_Validating;
            // 
            // TextBoxPhotoMinNoLegs
            // 
            TextBoxPhotoMinNoLegs.Location = new Point(148, 118);
            TextBoxPhotoMinNoLegs.Name = "TextBoxPhotoMinNoLegs";
            TextBoxPhotoMinNoLegs.Size = new Size(119, 23);
            TextBoxPhotoMinNoLegs.TabIndex = 13;
            TextBoxPhotoMinNoLegs.Text = "3";
            TextBoxPhotoMinNoLegs.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxPhotoMinNoLegs, "Maximum leg distance in miles to next photo");
            TextBoxPhotoMinNoLegs.Validating += TextBoxInteger_Validating;
            // 
            // ButtonPhotoTourDefault
            // 
            ButtonPhotoTourDefault.Location = new Point(469, 15);
            ButtonPhotoTourDefault.Name = "ButtonPhotoTourDefault";
            ButtonPhotoTourDefault.Size = new Size(75, 23);
            ButtonPhotoTourDefault.TabIndex = 12;
            ButtonPhotoTourDefault.Text = "Default";
            ButtonPhotoTourDefault.UseVisualStyleBackColor = true;
            ButtonPhotoTourDefault.Click += ButtonPhotoTourDefault_Click;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(16, 79);
            label11.Name = "label11";
            label11.Size = new Size(75, 15);
            label11.TabIndex = 4;
            label11.Text = "Max Leg Dist";
            // 
            // TextBoxPhotoMaxLegDist
            // 
            TextBoxPhotoMaxLegDist.Location = new Point(148, 71);
            TextBoxPhotoMaxLegDist.Name = "TextBoxPhotoMaxLegDist";
            TextBoxPhotoMaxLegDist.Size = new Size(119, 23);
            TextBoxPhotoMaxLegDist.TabIndex = 3;
            TextBoxPhotoMaxLegDist.Text = "10";
            TextBoxPhotoMaxLegDist.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxPhotoMaxLegDist, "Maximum leg distance in miles to next photo");
            TextBoxPhotoMaxLegDist.Validating += TextBoxDouble_Validating;
            // 
            // TabPageSign
            // 
            TabPageSign.Controls.Add(TextBoxSignSegmentRadius);
            TabPageSign.Controls.Add(TextBoxSignSegmentLength);
            TabPageSign.Controls.Add(label24);
            TabPageSign.Controls.Add(label23);
            TabPageSign.Controls.Add(TextBoxSignGateHeight);
            TabPageSign.Controls.Add(label22);
            TabPageSign.Controls.Add(PictureBoxSignWriting);
            TabPageSign.Controls.Add(TextBoxSignWindowWidth);
            TabPageSign.Controls.Add(TextBoxSignFont);
            TabPageSign.Controls.Add(label21);
            TabPageSign.Controls.Add(label20);
            TabPageSign.Controls.Add(TextBoxSignTilt);
            TabPageSign.Controls.Add(label19);
            TabPageSign.Controls.Add(TextBoxSignMessage);
            TabPageSign.Controls.Add(label16);
            TabPageSign.Location = new Point(4, 24);
            TabPageSign.Name = "TabPageSign";
            TabPageSign.Size = new Size(812, 438);
            TabPageSign.TabIndex = 3;
            TabPageSign.Text = "Sign Writing";
            TabPageSign.UseVisualStyleBackColor = true;
            // 
            // TextBoxSignSegmentRadius
            // 
            TextBoxSignSegmentRadius.Location = new Point(148, 312);
            TextBoxSignSegmentRadius.Name = "TextBoxSignSegmentRadius";
            TextBoxSignSegmentRadius.Size = new Size(100, 23);
            TextBoxSignSegmentRadius.TabIndex = 14;
            TextBoxSignSegmentRadius.Text = "500";
            TextBoxSignSegmentRadius.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxSignSegmentRadius, "Radius of space between segment ends in feet");
            TextBoxSignSegmentRadius.Validating += TextBoxInteger_Validating;
            // 
            // TextBoxSignSegmentLength
            // 
            TextBoxSignSegmentLength.Location = new Point(148, 262);
            TextBoxSignSegmentLength.Name = "TextBoxSignSegmentLength";
            TextBoxSignSegmentLength.Size = new Size(100, 23);
            TextBoxSignSegmentLength.TabIndex = 13;
            TextBoxSignSegmentLength.Text = "5000";
            TextBoxSignSegmentLength.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxSignSegmentLength, "Length of segment in feet");
            TextBoxSignSegmentLength.Validating += TextBoxInteger_Validating;
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new Point(16, 315);
            label24.Name = "label24";
            label24.Size = new Size(92, 15);
            label24.TabIndex = 12;
            label24.Text = "Segment Radius";
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new Point(16, 265);
            label23.Name = "label23";
            label23.Size = new Size(94, 15);
            label23.TabIndex = 11;
            label23.Text = "Segment Length";
            // 
            // TextBoxSignGateHeight
            // 
            TextBoxSignGateHeight.Location = new Point(148, 214);
            TextBoxSignGateHeight.Name = "TextBoxSignGateHeight";
            TextBoxSignGateHeight.Size = new Size(100, 23);
            TextBoxSignGateHeight.TabIndex = 10;
            TextBoxSignGateHeight.Text = "1000";
            TextBoxSignGateHeight.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxSignGateHeight, "Above ground (feet)");
            TextBoxSignGateHeight.Validating += TextBoxInteger_Validating;
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new Point(16, 217);
            label22.Name = "label22";
            label22.Size = new Size(70, 15);
            label22.TabIndex = 9;
            label22.Text = "Gate Height";
            // 
            // PictureBoxSignWriting
            // 
            PictureBoxSignWriting.Location = new Point(279, 71);
            PictureBoxSignWriting.Name = "PictureBoxSignWriting";
            PictureBoxSignWriting.Size = new Size(500, 241);
            PictureBoxSignWriting.TabIndex = 8;
            PictureBoxSignWriting.TabStop = false;
            // 
            // TextBoxSignWindowWidth
            // 
            TextBoxSignWindowWidth.Location = new Point(148, 166);
            TextBoxSignWindowWidth.Name = "TextBoxSignWindowWidth";
            TextBoxSignWindowWidth.Size = new Size(100, 23);
            TextBoxSignWindowWidth.TabIndex = 7;
            TextBoxSignWindowWidth.Text = "1000";
            TextBoxSignWindowWidth.TextAlign = HorizontalAlignment.Center;
            TextBoxSignWindowWidth.Validating += TextBoxInteger_Validating;
            // 
            // TextBoxSignFont
            // 
            TextBoxSignFont.Enabled = false;
            TextBoxSignFont.Location = new Point(148, 118);
            TextBoxSignFont.Name = "TextBoxSignFont";
            TextBoxSignFont.Size = new Size(100, 23);
            TextBoxSignFont.TabIndex = 6;
            TextBoxSignFont.Text = "Segment 22";
            TextBoxSignFont.TextAlign = HorizontalAlignment.Center;
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new Point(16, 175);
            label21.Name = "label21";
            label21.Size = new Size(86, 15);
            label21.TabIndex = 5;
            label21.Text = "Window Width";
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new Point(16, 126);
            label20.Name = "label20";
            label20.Size = new Size(31, 15);
            label20.TabIndex = 4;
            label20.Text = "Font";
            // 
            // TextBoxSignTilt
            // 
            TextBoxSignTilt.Location = new Point(148, 71);
            TextBoxSignTilt.Name = "TextBoxSignTilt";
            TextBoxSignTilt.Size = new Size(100, 23);
            TextBoxSignTilt.TabIndex = 3;
            TextBoxSignTilt.Text = "10";
            TextBoxSignTilt.TextAlign = HorizontalAlignment.Center;
            TextBoxSignTilt.Validating += TextBoxInteger_Validating;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(16, 79);
            label19.Name = "label19";
            label19.Size = new Size(57, 15);
            label19.TabIndex = 2;
            label19.Text = "Tilt Angle";
            toolTip1.SetToolTip(label19, "Tilt Angle in degrees from horizontal");
            // 
            // TextBoxSignMessage
            // 
            TextBoxSignMessage.Location = new Point(148, 26);
            TextBoxSignMessage.Name = "TextBoxSignMessage";
            TextBoxSignMessage.Size = new Size(631, 23);
            TextBoxSignMessage.TabIndex = 1;
            TextBoxSignMessage.Text = "FNQ Kid";
            toolTip1.SetToolTip(TextBoxSignMessage, "Message consisting only of alphabetic characters");
            TextBoxSignMessage.Validating += TextBoxString_Validating;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(16, 34);
            label16.Name = "label16";
            label16.Size = new Size(53, 15);
            label16.TabIndex = 0;
            label16.Text = "Message";
            // 
            // TabPageCelestial
            // 
            TabPageCelestial.Controls.Add(TextBoxCelestialMaxDist);
            TabPageCelestial.Controls.Add(TextBoxCelestialMinDist);
            TabPageCelestial.Controls.Add(label26);
            TabPageCelestial.Controls.Add(label25);
            TabPageCelestial.Location = new Point(4, 24);
            TabPageCelestial.Name = "TabPageCelestial";
            TabPageCelestial.Size = new Size(812, 438);
            TabPageCelestial.TabIndex = 4;
            TabPageCelestial.Text = "Celestial Navigation";
            TabPageCelestial.UseVisualStyleBackColor = true;
            // 
            // TextBoxCelestialMaxDist
            // 
            TextBoxCelestialMaxDist.Location = new Point(148, 71);
            TextBoxCelestialMaxDist.Name = "TextBoxCelestialMaxDist";
            TextBoxCelestialMaxDist.Size = new Size(100, 23);
            TextBoxCelestialMaxDist.TabIndex = 3;
            TextBoxCelestialMaxDist.Text = "30";
            TextBoxCelestialMaxDist.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxCelestialMaxDist, "Max run home from starting position (miles)");
            TextBoxCelestialMaxDist.Validating += TextBoxInteger_Validating;
            // 
            // TextBoxCelestialMinDist
            // 
            TextBoxCelestialMinDist.Location = new Point(148, 26);
            TextBoxCelestialMinDist.Name = "TextBoxCelestialMinDist";
            TextBoxCelestialMinDist.Size = new Size(100, 23);
            TextBoxCelestialMinDist.TabIndex = 2;
            TextBoxCelestialMinDist.Text = "20";
            TextBoxCelestialMinDist.TextAlign = HorizontalAlignment.Center;
            toolTip1.SetToolTip(TextBoxCelestialMinDist, "Min run home from starting position (miles)");
            TextBoxCelestialMinDist.Validating += TextBoxInteger_Validating;
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Location = new Point(16, 79);
            label26.Name = "label26";
            label26.Size = new Size(109, 15);
            label26.TabIndex = 1;
            label26.Text = "Maximum distance";
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new Point(16, 34);
            label25.Name = "label25";
            label25.Size = new Size(107, 15);
            label25.TabIndex = 0;
            label25.Text = "Minimum distance";
            // 
            // ButtonGenerateScenario
            // 
            ButtonGenerateScenario.ImageAlign = ContentAlignment.BottomLeft;
            ButtonGenerateScenario.Location = new Point(343, 493);
            ButtonGenerateScenario.Name = "ButtonGenerateScenario";
            ButtonGenerateScenario.Size = new Size(152, 43);
            ButtonGenerateScenario.TabIndex = 1;
            ButtonGenerateScenario.Text = "Generate Scenario";
            ButtonGenerateScenario.UseVisualStyleBackColor = true;
            ButtonGenerateScenario.Click += ButtonGenerateScenario_Click;
            // 
            // ButtonHelp
            // 
            ButtonHelp.Location = new Point(753, 513);
            ButtonHelp.Name = "ButtonHelp";
            ButtonHelp.Size = new Size(75, 23);
            ButtonHelp.TabIndex = 2;
            ButtonHelp.Text = "Help";
            ButtonHelp.UseVisualStyleBackColor = true;
            ButtonHelp.Click += ButtonHelp_Click;
            // 
            // Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(844, 553);
            Controls.Add(ButtonHelp);
            Controls.Add(ButtonGenerateScenario);
            Controls.Add(TabControl);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form";
            Text = "P3D Scenario Generator";
            Load += Init;
            TabControl.ResumeLayout(false);
            TabPageGeneral.ResumeLayout(false);
            TabPageGeneral.PerformLayout();
            TabPageCircuit.ResumeLayout(false);
            tableLayoutPanelCircuit.ResumeLayout(false);
            tableLayoutPanelCircuit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBoxCircuit).EndInit();
            TabPagePhoto.ResumeLayout(false);
            TabPagePhoto.PerformLayout();
            TabPageSign.ResumeLayout(false);
            TabPageSign.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBoxSignWriting).EndInit();
            TabPageCelestial.ResumeLayout(false);
            TabPageCelestial.PerformLayout();
            ResumeLayout(false);
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelCircuit;
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
        internal System.Windows.Forms.TextBox TextBoxSignGateHeight;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label23;
        internal System.Windows.Forms.TextBox TextBoxSignSegmentRadius;
        internal System.Windows.Forms.TextBox TextBoxSignSegmentLength;
        private System.Windows.Forms.TabPage TabPageCelestial;
        internal System.Windows.Forms.TextBox TextBoxCelestialMaxDist;
        internal System.Windows.Forms.TextBox TextBoxCelestialMinDist;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label25;
        internal System.Windows.Forms.DateTimePicker DatePicker;
        internal System.Windows.Forms.DateTimePicker TimePicker;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Button button1;
        internal System.Windows.Forms.TextBox TextBoxP3Dv5Files;
        internal TextBox TextBoxCircuitTurnRate;
        private Label label28;
    }
}

