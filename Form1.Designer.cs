
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
            ButtonGenerateScenario = new Button();
            toolTip1 = new ToolTip(components);
            button1 = new Button();
            TextBoxCelestialMinDist = new TextBox();
            TextBoxCelestialMaxDist = new TextBox();
            TextBoxSignMessage = new TextBox();
            label19 = new Label();
            TextBoxSignGateHeight = new TextBox();
            TextBoxSignSegmentLength = new TextBox();
            TextBoxSignSegmentRadius = new TextBox();
            TextBoxPhotoMaxLegDist = new TextBox();
            TextBoxPhotoMinLegDist = new TextBox();
            TextBoxPhotoWindowSize = new TextBox();
            TextBoxPhotoMaxBearingChange = new TextBox();
            TextBoxPhotoHotspotRadius = new TextBox();
            TextBoxCircuitSpeed = new TextBox();
            TextBoxCircuitTurnRate = new TextBox();
            TextBoxCircuitHeightDown = new TextBox();
            TextBoxCircuitHeightUpwind = new TextBox();
            TextBoxCircuitFinal = new TextBox();
            TextBoxCircuitBase = new TextBox();
            TextBoxCircuitHeightBase = new TextBox();
            TextBoxCircuitUpwind = new TextBox();
            buttonAircraft = new Button();
            ButtonHelp = new Button();
            TabPageSettings = new TabPage();
            tableLayoutPanelSettings = new TableLayoutPanel();
            TextBoxP3Dv5Files = new TextBox();
            TabPageCelestial = new TabPage();
            label26 = new Label();
            label25 = new Label();
            TabPageSign = new TabPage();
            TextBoxSignWindowWidth = new TextBox();
            TextBoxSignFont = new TextBox();
            TextBoxSignTilt = new TextBox();
            label24 = new Label();
            label23 = new Label();
            label22 = new Label();
            PictureBoxSignWriting = new PictureBox();
            label21 = new Label();
            label20 = new Label();
            label16 = new Label();
            TabPagePhoto = new TabPage();
            label18 = new Label();
            TextBoxPhotoMaxNoLegs = new TextBox();
            TextBoxPhotoMinNoLegs = new TextBox();
            label17 = new Label();
            label15 = new Label();
            label14 = new Label();
            label13 = new Label();
            label12 = new Label();
            ButtonPhotoTourDefault = new Button();
            label11 = new Label();
            TabPageCircuit = new TabPage();
            tableLayoutPanelCircuit = new TableLayoutPanel();
            label7 = new Label();
            label6 = new Label();
            label9 = new Label();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label10 = new Label();
            label28 = new Label();
            ButtonCircuitDefault = new Button();
            PictureBoxCircuit = new PictureBox();
            TabPageMenu = new TabPage();
            GroupBoxDateTime = new GroupBox();
            DatePicker = new DateTimePicker();
            TextBoxScenarioTitle = new TextBox();
            TimePicker = new DateTimePicker();
            label8 = new Label();
            GroupBoxAircraft = new GroupBox();
            ListBoxAircraft = new ListBox();
            GroupBoxScenario = new GroupBox();
            ListBoxScenarioType = new ListBox();
            TextBoxSelectedScenario = new TextBox();
            GroupBoxRunway = new GroupBox();
            ListBoxRunways = new ListBox();
            ButtonRandRunway = new Button();
            label1 = new Label();
            TextBoxSelectedRunway = new TextBox();
            TextBoxSearchRunway = new TextBox();
            label2 = new Label();
            TabControl = new TabControl();
            TabPageWikiList = new TabPage();
            ListBoxWikiCellName = new ListBox();
            ListBoxWikiAttribute = new ListBox();
            LabelWikiAttribute = new Label();
            LabelWikiCellName = new Label();
            ListBoxWikiTableNames = new ListBox();
            LabelWikiTableNames = new Label();
            LabelWikiURL = new Label();
            TextBoxWikiURL = new TextBox();
            LabelWikiRoute = new Label();
            TextBoxWikiRoute = new TextBox();
            TabPageSettings.SuspendLayout();
            tableLayoutPanelSettings.SuspendLayout();
            TabPageCelestial.SuspendLayout();
            TabPageSign.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBoxSignWriting).BeginInit();
            TabPagePhoto.SuspendLayout();
            TabPageCircuit.SuspendLayout();
            tableLayoutPanelCircuit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBoxCircuit).BeginInit();
            TabPageMenu.SuspendLayout();
            GroupBoxDateTime.SuspendLayout();
            GroupBoxAircraft.SuspendLayout();
            GroupBoxScenario.SuspendLayout();
            GroupBoxRunway.SuspendLayout();
            TabControl.SuspendLayout();
            TabPageWikiList.SuspendLayout();
            SuspendLayout();
            // 
            // ButtonGenerateScenario
            // 
            ButtonGenerateScenario.ImageAlign = ContentAlignment.BottomLeft;
            ButtonGenerateScenario.Location = new Point(348, 493);
            ButtonGenerateScenario.Name = "ButtonGenerateScenario";
            ButtonGenerateScenario.Size = new Size(152, 43);
            ButtonGenerateScenario.TabIndex = 1;
            ButtonGenerateScenario.Text = "Generate Scenario";
            ButtonGenerateScenario.UseVisualStyleBackColor = true;
            ButtonGenerateScenario.Click += ButtonGenerateScenario_Click;
            // 
            // button1
            // 
            button1.Location = new Point(3, 3);
            button1.Name = "button1";
            button1.Size = new Size(120, 23);
            button1.TabIndex = 19;
            button1.Text = "Prepar3D v5 Files";
            toolTip1.SetToolTip(button1, "Select the Prepar3D v5 Files location where scenario will be stored");
            button1.UseVisualStyleBackColor = true;
            button1.Click += ButtonP3Dv5Files_Click;
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
            // buttonAircraft
            // 
            buttonAircraft.Location = new Point(75, 172);
            buttonAircraft.Name = "buttonAircraft";
            buttonAircraft.Size = new Size(120, 23);
            buttonAircraft.TabIndex = 11;
            buttonAircraft.Text = "aircraft.cfg";
            toolTip1.SetToolTip(buttonAircraft, "Select the \"aircraft.cfg\" file for your aircraft and then the variation from the list above");
            buttonAircraft.UseVisualStyleBackColor = true;
            buttonAircraft.Click += ButtonAircraft_Click;
            // 
            // ButtonHelp
            // 
            ButtonHelp.Location = new Point(721, 513);
            ButtonHelp.Name = "ButtonHelp";
            ButtonHelp.Size = new Size(75, 23);
            ButtonHelp.TabIndex = 2;
            ButtonHelp.Text = "Help";
            ButtonHelp.UseVisualStyleBackColor = true;
            ButtonHelp.Click += ButtonHelp_Click;
            // 
            // TabPageSettings
            // 
            TabPageSettings.Controls.Add(tableLayoutPanelSettings);
            TabPageSettings.Location = new Point(4, 24);
            TabPageSettings.Name = "TabPageSettings";
            TabPageSettings.Padding = new Padding(3);
            TabPageSettings.Size = new Size(812, 438);
            TabPageSettings.TabIndex = 5;
            TabPageSettings.Text = "Settings";
            TabPageSettings.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanelSettings
            // 
            tableLayoutPanelSettings.ColumnCount = 2;
            tableLayoutPanelSettings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelSettings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelSettings.Controls.Add(TextBoxP3Dv5Files, 1, 0);
            tableLayoutPanelSettings.Controls.Add(button1, 0, 0);
            tableLayoutPanelSettings.Location = new Point(16, 16);
            tableLayoutPanelSettings.Name = "tableLayoutPanelSettings";
            tableLayoutPanelSettings.RowCount = 2;
            tableLayoutPanelSettings.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanelSettings.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanelSettings.Size = new Size(760, 100);
            tableLayoutPanelSettings.TabIndex = 0;
            // 
            // TextBoxP3Dv5Files
            // 
            TextBoxP3Dv5Files.Enabled = false;
            TextBoxP3Dv5Files.Location = new Point(383, 3);
            TextBoxP3Dv5Files.Name = "TextBoxP3Dv5Files";
            TextBoxP3Dv5Files.Size = new Size(302, 23);
            TextBoxP3Dv5Files.TabIndex = 18;
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
            // TabPageSign
            // 
            TabPageSign.Controls.Add(TextBoxSignSegmentRadius);
            TabPageSign.Controls.Add(TextBoxSignSegmentLength);
            TabPageSign.Controls.Add(TextBoxSignGateHeight);
            TabPageSign.Controls.Add(TextBoxSignWindowWidth);
            TabPageSign.Controls.Add(TextBoxSignFont);
            TabPageSign.Controls.Add(TextBoxSignTilt);
            TabPageSign.Controls.Add(TextBoxSignMessage);
            TabPageSign.Controls.Add(label24);
            TabPageSign.Controls.Add(label23);
            TabPageSign.Controls.Add(label22);
            TabPageSign.Controls.Add(PictureBoxSignWriting);
            TabPageSign.Controls.Add(label21);
            TabPageSign.Controls.Add(label20);
            TabPageSign.Controls.Add(label19);
            TabPageSign.Controls.Add(label16);
            TabPageSign.Location = new Point(4, 24);
            TabPageSign.Name = "TabPageSign";
            TabPageSign.Size = new Size(812, 438);
            TabPageSign.TabIndex = 3;
            TabPageSign.Text = "Sign Writing";
            TabPageSign.UseVisualStyleBackColor = true;
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
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(16, 34);
            label16.Name = "label16";
            label16.Size = new Size(53, 15);
            label16.TabIndex = 0;
            label16.Text = "Message";
            // 
            // TabPagePhoto
            // 
            TabPagePhoto.Controls.Add(label18);
            TabPagePhoto.Controls.Add(TextBoxPhotoHotspotRadius);
            TabPagePhoto.Controls.Add(TextBoxPhotoMaxBearingChange);
            TabPagePhoto.Controls.Add(TextBoxPhotoWindowSize);
            TabPagePhoto.Controls.Add(TextBoxPhotoMinLegDist);
            TabPagePhoto.Controls.Add(TextBoxPhotoMaxNoLegs);
            TabPagePhoto.Controls.Add(TextBoxPhotoMinNoLegs);
            TabPagePhoto.Controls.Add(TextBoxPhotoMaxLegDist);
            TabPagePhoto.Controls.Add(label17);
            TabPagePhoto.Controls.Add(label15);
            TabPagePhoto.Controls.Add(label14);
            TabPagePhoto.Controls.Add(label13);
            TabPagePhoto.Controls.Add(label12);
            TabPagePhoto.Controls.Add(ButtonPhotoTourDefault);
            TabPagePhoto.Controls.Add(label11);
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
            // TextBoxPhotoMaxNoLegs
            // 
            TextBoxPhotoMaxNoLegs.Location = new Point(148, 166);
            TextBoxPhotoMaxNoLegs.Name = "TextBoxPhotoMaxNoLegs";
            TextBoxPhotoMaxNoLegs.Size = new Size(119, 23);
            TextBoxPhotoMaxNoLegs.TabIndex = 14;
            TextBoxPhotoMaxNoLegs.Text = "7";
            TextBoxPhotoMaxNoLegs.TextAlign = HorizontalAlignment.Center;
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
            TextBoxPhotoMinNoLegs.Validating += TextBoxInteger_Validating;
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
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(16, 221);
            label15.Name = "label15";
            label15.Size = new Size(74, 15);
            label15.TabIndex = 21;
            label15.Text = "Window Size";
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
            // TabPageMenu
            // 
            TabPageMenu.Controls.Add(GroupBoxDateTime);
            TabPageMenu.Controls.Add(GroupBoxAircraft);
            TabPageMenu.Controls.Add(GroupBoxScenario);
            TabPageMenu.Controls.Add(GroupBoxRunway);
            TabPageMenu.Location = new Point(4, 24);
            TabPageMenu.Name = "TabPageMenu";
            TabPageMenu.Padding = new Padding(3);
            TabPageMenu.Size = new Size(812, 438);
            TabPageMenu.TabIndex = 6;
            TabPageMenu.Text = "Menu";
            TabPageMenu.UseVisualStyleBackColor = true;
            // 
            // GroupBoxDateTime
            // 
            GroupBoxDateTime.BackColor = Color.FromArgb(192, 192, 255);
            GroupBoxDateTime.Controls.Add(DatePicker);
            GroupBoxDateTime.Controls.Add(TextBoxScenarioTitle);
            GroupBoxDateTime.Controls.Add(TimePicker);
            GroupBoxDateTime.Controls.Add(label8);
            GroupBoxDateTime.Location = new Point(302, 289);
            GroupBoxDateTime.Name = "GroupBoxDateTime";
            GroupBoxDateTime.Size = new Size(478, 115);
            GroupBoxDateTime.TabIndex = 17;
            GroupBoxDateTime.TabStop = false;
            GroupBoxDateTime.Text = "Date and Time";
            // 
            // DatePicker
            // 
            DatePicker.CustomFormat = "";
            DatePicker.Location = new Point(30, 33);
            DatePicker.Name = "DatePicker";
            DatePicker.Size = new Size(206, 23);
            DatePicker.TabIndex = 15;
            // 
            // TextBoxScenarioTitle
            // 
            TextBoxScenarioTitle.Location = new Point(305, 75);
            TextBoxScenarioTitle.Name = "TextBoxScenarioTitle";
            TextBoxScenarioTitle.Size = new Size(156, 23);
            TextBoxScenarioTitle.TabIndex = 13;
            // 
            // TimePicker
            // 
            TimePicker.Format = DateTimePickerFormat.Time;
            TimePicker.Location = new Point(342, 33);
            TimePicker.Name = "TimePicker";
            TimePicker.ShowUpDown = true;
            TimePicker.Size = new Size(119, 23);
            TimePicker.TabIndex = 16;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(30, 78);
            label8.Name = "label8";
            label8.Size = new Size(77, 15);
            label8.TabIndex = 14;
            label8.Text = "Scenario Title";
            // 
            // GroupBoxAircraft
            // 
            GroupBoxAircraft.BackColor = Color.FromArgb(192, 255, 192);
            GroupBoxAircraft.Controls.Add(ListBoxAircraft);
            GroupBoxAircraft.Controls.Add(buttonAircraft);
            GroupBoxAircraft.Location = new Point(515, 23);
            GroupBoxAircraft.Name = "GroupBoxAircraft";
            GroupBoxAircraft.Size = new Size(265, 235);
            GroupBoxAircraft.TabIndex = 13;
            GroupBoxAircraft.TabStop = false;
            GroupBoxAircraft.Text = "Aircraft selection";
            // 
            // ListBoxAircraft
            // 
            ListBoxAircraft.FormattingEnabled = true;
            ListBoxAircraft.ItemHeight = 15;
            ListBoxAircraft.Location = new Point(17, 47);
            ListBoxAircraft.Name = "ListBoxAircraft";
            ListBoxAircraft.Size = new Size(231, 94);
            ListBoxAircraft.TabIndex = 12;
            ListBoxAircraft.SelectedIndexChanged += ListBoxAircraft_SelectedIndexChanged;
            // 
            // GroupBoxScenario
            // 
            GroupBoxScenario.BackColor = Color.FromArgb(192, 255, 255);
            GroupBoxScenario.Controls.Add(ListBoxScenarioType);
            GroupBoxScenario.Controls.Add(TextBoxSelectedScenario);
            GroupBoxScenario.Location = new Point(302, 23);
            GroupBoxScenario.Name = "GroupBoxScenario";
            GroupBoxScenario.Size = new Size(182, 235);
            GroupBoxScenario.TabIndex = 8;
            GroupBoxScenario.TabStop = false;
            GroupBoxScenario.Text = "Scenario Type";
            // 
            // ListBoxScenarioType
            // 
            ListBoxScenarioType.FormattingEnabled = true;
            ListBoxScenarioType.ItemHeight = 15;
            ListBoxScenarioType.Location = new Point(30, 47);
            ListBoxScenarioType.Name = "ListBoxScenarioType";
            ListBoxScenarioType.Size = new Size(120, 94);
            ListBoxScenarioType.TabIndex = 6;
            ListBoxScenarioType.SelectedIndexChanged += ListBoxScenarioType_SelectedIndexChanged;
            // 
            // TextBoxSelectedScenario
            // 
            TextBoxSelectedScenario.Enabled = false;
            TextBoxSelectedScenario.Location = new Point(30, 172);
            TextBoxSelectedScenario.Name = "TextBoxSelectedScenario";
            TextBoxSelectedScenario.Size = new Size(119, 23);
            TextBoxSelectedScenario.TabIndex = 7;
            // 
            // GroupBoxRunway
            // 
            GroupBoxRunway.BackColor = Color.FromArgb(255, 192, 128);
            GroupBoxRunway.Controls.Add(ListBoxRunways);
            GroupBoxRunway.Controls.Add(ButtonRandRunway);
            GroupBoxRunway.Controls.Add(label1);
            GroupBoxRunway.Controls.Add(TextBoxSelectedRunway);
            GroupBoxRunway.Controls.Add(TextBoxSearchRunway);
            GroupBoxRunway.Controls.Add(label2);
            GroupBoxRunway.Location = new Point(31, 23);
            GroupBoxRunway.Name = "GroupBoxRunway";
            GroupBoxRunway.Size = new Size(238, 381);
            GroupBoxRunway.TabIndex = 6;
            GroupBoxRunway.TabStop = false;
            GroupBoxRunway.Text = "Runway selection";
            // 
            // ListBoxRunways
            // 
            ListBoxRunways.FormattingEnabled = true;
            ListBoxRunways.ItemHeight = 15;
            ListBoxRunways.Location = new Point(55, 47);
            ListBoxRunways.Name = "ListBoxRunways";
            ListBoxRunways.Size = new Size(120, 94);
            ListBoxRunways.TabIndex = 0;
            ListBoxRunways.SelectedIndexChanged += ListBoxRunways_SelectedIndexChanged;
            // 
            // ButtonRandRunway
            // 
            ButtonRandRunway.Location = new Point(71, 341);
            ButtonRandRunway.Name = "ButtonRandRunway";
            ButtonRandRunway.Size = new Size(91, 23);
            ButtonRandRunway.TabIndex = 5;
            ButtonRandRunway.Text = "Random Runway";
            ButtonRandRunway.UseVisualStyleBackColor = true;
            ButtonRandRunway.Click += ButtonRandRunway_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(38, 176);
            label1.Name = "label1";
            label1.Size = new Size(42, 15);
            label1.TabIndex = 3;
            label1.Text = "Search";
            // 
            // TextBoxSelectedRunway
            // 
            TextBoxSelectedRunway.Enabled = false;
            TextBoxSelectedRunway.Location = new Point(102, 266);
            TextBoxSelectedRunway.Name = "TextBoxSelectedRunway";
            TextBoxSelectedRunway.Size = new Size(91, 23);
            TextBoxSelectedRunway.TabIndex = 1;
            // 
            // TextBoxSearchRunway
            // 
            TextBoxSearchRunway.Location = new Point(102, 173);
            TextBoxSearchRunway.Name = "TextBoxSearchRunway";
            TextBoxSearchRunway.Size = new Size(91, 23);
            TextBoxSearchRunway.TabIndex = 2;
            TextBoxSearchRunway.TextChanged += TextBoxSearchRunway_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(29, 269);
            label2.Name = "label2";
            label2.Size = new Size(51, 15);
            label2.TabIndex = 4;
            label2.Text = "Selected";
            // 
            // TabControl
            // 
            TabControl.Controls.Add(TabPageMenu);
            TabControl.Controls.Add(TabPageCircuit);
            TabControl.Controls.Add(TabPagePhoto);
            TabControl.Controls.Add(TabPageSign);
            TabControl.Controls.Add(TabPageCelestial);
            TabControl.Controls.Add(TabPageWikiList);
            TabControl.Controls.Add(TabPageSettings);
            TabControl.Location = new Point(12, 12);
            TabControl.Name = "TabControl";
            TabControl.SelectedIndex = 0;
            TabControl.Size = new Size(820, 466);
            TabControl.TabIndex = 0;
            TabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            // 
            // TabPageWikiList
            // 
            TabPageWikiList.Controls.Add(TextBoxWikiRoute);
            TabPageWikiList.Controls.Add(LabelWikiRoute);
            TabPageWikiList.Controls.Add(ListBoxWikiCellName);
            TabPageWikiList.Controls.Add(ListBoxWikiAttribute);
            TabPageWikiList.Controls.Add(LabelWikiAttribute);
            TabPageWikiList.Controls.Add(LabelWikiCellName);
            TabPageWikiList.Controls.Add(ListBoxWikiTableNames);
            TabPageWikiList.Controls.Add(LabelWikiTableNames);
            TabPageWikiList.Controls.Add(LabelWikiURL);
            TabPageWikiList.Controls.Add(TextBoxWikiURL);
            TabPageWikiList.Location = new Point(4, 24);
            TabPageWikiList.Name = "TabPageWikiList";
            TabPageWikiList.Padding = new Padding(3);
            TabPageWikiList.Size = new Size(812, 438);
            TabPageWikiList.TabIndex = 7;
            TabPageWikiList.Text = "Wikipedia List";
            TabPageWikiList.UseVisualStyleBackColor = true;
            // 
            // ListBoxWikiCellName
            // 
            ListBoxWikiCellName.FormattingEnabled = true;
            ListBoxWikiCellName.ItemHeight = 15;
            ListBoxWikiCellName.Items.AddRange(new object[] { "td", "th" });
            ListBoxWikiCellName.Location = new Point(230, 74);
            ListBoxWikiCellName.Name = "ListBoxWikiCellName";
            ListBoxWikiCellName.Size = new Size(120, 34);
            ListBoxWikiCellName.TabIndex = 8;
            // 
            // ListBoxWikiAttribute
            // 
            ListBoxWikiAttribute.FormattingEnabled = true;
            ListBoxWikiAttribute.ItemHeight = 15;
            ListBoxWikiAttribute.Items.AddRange(new object[] { "|", "scope|row" });
            ListBoxWikiAttribute.Location = new Point(525, 74);
            ListBoxWikiAttribute.Name = "ListBoxWikiAttribute";
            ListBoxWikiAttribute.Size = new Size(120, 34);
            ListBoxWikiAttribute.TabIndex = 7;
            // 
            // LabelWikiAttribute
            // 
            LabelWikiAttribute.AutoSize = true;
            LabelWikiAttribute.Location = new Point(405, 77);
            LabelWikiAttribute.Name = "LabelWikiAttribute";
            LabelWikiAttribute.Size = new Size(54, 15);
            LabelWikiAttribute.TabIndex = 6;
            LabelWikiAttribute.Text = "Attribute";
            // 
            // LabelWikiCellName
            // 
            LabelWikiCellName.AutoSize = true;
            LabelWikiCellName.Location = new Point(146, 77);
            LabelWikiCellName.Name = "LabelWikiCellName";
            LabelWikiCellName.Size = new Size(62, 15);
            LabelWikiCellName.TabIndex = 4;
            LabelWikiCellName.Text = "Cell Name";
            // 
            // ListBoxWikiTableNames
            // 
            ListBoxWikiTableNames.FormattingEnabled = true;
            ListBoxWikiTableNames.ItemHeight = 15;
            ListBoxWikiTableNames.Location = new Point(146, 132);
            ListBoxWikiTableNames.Name = "ListBoxWikiTableNames";
            ListBoxWikiTableNames.Size = new Size(613, 49);
            ListBoxWikiTableNames.TabIndex = 3;
            ListBoxWikiTableNames.SelectedIndexChanged += ListBoxWikiTableNames_SelectedIndexChanged;
            // 
            // LabelWikiTableNames
            // 
            LabelWikiTableNames.AutoSize = true;
            LabelWikiTableNames.Location = new Point(37, 132);
            LabelWikiTableNames.Name = "LabelWikiTableNames";
            LabelWikiTableNames.Size = new Size(74, 15);
            LabelWikiTableNames.TabIndex = 2;
            LabelWikiTableNames.Text = "Table Names";
            // 
            // LabelWikiURL
            // 
            LabelWikiURL.AutoSize = true;
            LabelWikiURL.Location = new Point(37, 37);
            LabelWikiURL.Name = "LabelWikiURL";
            LabelWikiURL.Size = new Size(83, 15);
            LabelWikiURL.TabIndex = 1;
            LabelWikiURL.Text = "Wikipedia URL";
            // 
            // TextBoxWikiURL
            // 
            TextBoxWikiURL.Location = new Point(146, 29);
            TextBoxWikiURL.Name = "TextBoxWikiURL";
            TextBoxWikiURL.Size = new Size(613, 23);
            TextBoxWikiURL.TabIndex = 0;
            TextBoxWikiURL.TextChanged += TextBoxWikiURL_TextChanged;
            // 
            // LabelWikiRoute
            // 
            LabelWikiRoute.AutoSize = true;
            LabelWikiRoute.Location = new Point(37, 219);
            LabelWikiRoute.Name = "LabelWikiRoute";
            LabelWikiRoute.Size = new Size(83, 15);
            LabelWikiRoute.TabIndex = 9;
            LabelWikiRoute.Text = "Visit Sequence";
            // 
            // TextBoxWikiRoute
            // 
            TextBoxWikiRoute.Location = new Point(146, 211);
            TextBoxWikiRoute.Name = "TextBoxWikiRoute";
            TextBoxWikiRoute.Size = new Size(613, 23);
            TextBoxWikiRoute.TabIndex = 10;
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
            TabPageSettings.ResumeLayout(false);
            tableLayoutPanelSettings.ResumeLayout(false);
            tableLayoutPanelSettings.PerformLayout();
            TabPageCelestial.ResumeLayout(false);
            TabPageCelestial.PerformLayout();
            TabPageSign.ResumeLayout(false);
            TabPageSign.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBoxSignWriting).EndInit();
            TabPagePhoto.ResumeLayout(false);
            TabPagePhoto.PerformLayout();
            TabPageCircuit.ResumeLayout(false);
            tableLayoutPanelCircuit.ResumeLayout(false);
            tableLayoutPanelCircuit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBoxCircuit).EndInit();
            TabPageMenu.ResumeLayout(false);
            GroupBoxDateTime.ResumeLayout(false);
            GroupBoxDateTime.PerformLayout();
            GroupBoxAircraft.ResumeLayout(false);
            GroupBoxScenario.ResumeLayout(false);
            GroupBoxScenario.PerformLayout();
            GroupBoxRunway.ResumeLayout(false);
            GroupBoxRunway.PerformLayout();
            TabControl.ResumeLayout(false);
            TabPageWikiList.ResumeLayout(false);
            TabPageWikiList.PerformLayout();
            ResumeLayout(false);
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

#endregion
        private System.Windows.Forms.Button ButtonGenerateScenario;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button ButtonHelp;
        private TabPage TabPageSettings;
        private TableLayoutPanel tableLayoutPanelSettings;
        internal TextBox TextBoxP3Dv5Files;
        private Button button1;
        private TabPage TabPageCelestial;
        internal TextBox TextBoxCelestialMaxDist;
        internal TextBox TextBoxCelestialMinDist;
        private Label label26;
        private Label label25;
        private TabPage TabPageSign;
        internal TextBox TextBoxSignSegmentRadius;
        internal TextBox TextBoxSignSegmentLength;
        internal TextBox TextBoxSignGateHeight;
        internal TextBox TextBoxSignWindowWidth;
        internal TextBox TextBoxSignFont;
        internal TextBox TextBoxSignTilt;
        internal TextBox TextBoxSignMessage;
        private Label label24;
        private Label label23;
        private Label label22;
        private PictureBox PictureBoxSignWriting;
        private Label label21;
        private Label label20;
        private Label label19;
        private Label label16;
        private TabPage TabPagePhoto;
        private Label label18;
        internal TextBox TextBoxPhotoHotspotRadius;
        internal TextBox TextBoxPhotoMaxBearingChange;
        internal TextBox TextBoxPhotoWindowSize;
        internal TextBox TextBoxPhotoMinLegDist;
        internal TextBox TextBoxPhotoMaxNoLegs;
        internal TextBox TextBoxPhotoMinNoLegs;
        internal TextBox TextBoxPhotoMaxLegDist;
        private Label label17;
        private Label label15;
        private Label label14;
        private Label label13;
        private Label label12;
        private Button ButtonPhotoTourDefault;
        private Label label11;
        private TabPage TabPageCircuit;
        private TableLayoutPanel tableLayoutPanelCircuit;
        private Label label7;
        private Label label6;
        internal TextBox TextBoxCircuitUpwind;
        private Label label9;
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label10;
        internal TextBox TextBoxCircuitHeightBase;
        private Label label28;
        internal TextBox TextBoxCircuitBase;
        internal TextBox TextBoxCircuitFinal;
        internal TextBox TextBoxCircuitHeightUpwind;
        internal TextBox TextBoxCircuitHeightDown;
        internal TextBox TextBoxCircuitTurnRate;
        internal TextBox TextBoxCircuitSpeed;
        private Button ButtonCircuitDefault;
        private PictureBox PictureBoxCircuit;
        private TabPage TabPageMenu;
        private GroupBox GroupBoxDateTime;
        internal DateTimePicker DatePicker;
        internal TextBox TextBoxScenarioTitle;
        internal DateTimePicker TimePicker;
        private Label label8;
        private GroupBox GroupBoxAircraft;
        internal ListBox ListBoxAircraft;
        private Button buttonAircraft;
        private GroupBox GroupBoxScenario;
        internal ListBox ListBoxScenarioType;
        internal TextBox TextBoxSelectedScenario;
        private GroupBox GroupBoxRunway;
        internal ListBox ListBoxRunways;
        private Button ButtonRandRunway;
        private Label label1;
        internal TextBox TextBoxSelectedRunway;
        private TextBox TextBoxSearchRunway;
        private Label label2;
        private TabControl TabControl;
        private TabPage TabPageWikiList;
        private Label LabelWikiURL;
        internal TextBox TextBoxWikiURL;
        private ListBox ListBoxWikiTableNames;
        private Label LabelWikiTableNames;
        private Label LabelWikiCellName;
        private ListBox ListBoxWikiAttribute;
        private Label LabelWikiAttribute;
        private ListBox ListBoxWikiCellName;
        private TextBox TextBoxWikiRoute;
        private Label LabelWikiRoute;
    }
}

