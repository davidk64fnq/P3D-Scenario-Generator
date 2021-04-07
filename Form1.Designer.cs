
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form));
            this.TabControl = new System.Windows.Forms.TabControl();
            this.TabPageGeneral = new System.Windows.Forms.TabPage();
            this.ListBoxAircraft = new System.Windows.Forms.ListBox();
            this.buttonAircraft = new System.Windows.Forms.Button();
            this.buttonSaveLocation = new System.Windows.Forms.Button();
            this.textBoxSaveLocation = new System.Windows.Forms.TextBox();
            this.TextBoxSelectedScenario = new System.Windows.Forms.TextBox();
            this.ListBoxScenarioType = new System.Windows.Forms.ListBox();
            this.ButtonRandRunway = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.TextBoxSearchRunway = new System.Windows.Forms.TextBox();
            this.TextBoxSelectedRunway = new System.Windows.Forms.TextBox();
            this.ListBoxRunways = new System.Windows.Forms.ListBox();
            this.TabPageCircuit = new System.Windows.Forms.TabPage();
            this.ButtonCircuitDefault = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.TextBoxCircuitUpwind = new System.Windows.Forms.TextBox();
            this.TextBoxCircuitSpeed = new System.Windows.Forms.TextBox();
            this.TextBoxCircuitHeight = new System.Windows.Forms.TextBox();
            this.TextBoxCircuitFinal = new System.Windows.Forms.TextBox();
            this.TextBoxCircuitBase = new System.Windows.Forms.TextBox();
            this.PictureBoxCircuit = new System.Windows.Forms.PictureBox();
            this.ButtonGenerateScenario = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.TabControl.SuspendLayout();
            this.TabPageGeneral.SuspendLayout();
            this.TabPageCircuit.SuspendLayout();
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
            this.TabControl.Size = new System.Drawing.Size(820, 492);
            this.TabControl.TabIndex = 0;
            this.TabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl_SelectedIndexChanged);
            // 
            // TabPageGeneral
            // 
            this.TabPageGeneral.Controls.Add(this.ListBoxAircraft);
            this.TabPageGeneral.Controls.Add(this.buttonAircraft);
            this.TabPageGeneral.Controls.Add(this.buttonSaveLocation);
            this.TabPageGeneral.Controls.Add(this.textBoxSaveLocation);
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
            this.TabPageGeneral.Size = new System.Drawing.Size(812, 464);
            this.TabPageGeneral.TabIndex = 0;
            this.TabPageGeneral.Text = "General";
            this.TabPageGeneral.UseVisualStyleBackColor = true;
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
            this.buttonAircraft.Text = "Aircraft";
            this.toolTip1.SetToolTip(this.buttonAircraft, "Select the \".cfg\" file for your aircraft and then the variation from the list abo" +
        "ve");
            this.buttonAircraft.UseVisualStyleBackColor = true;
            this.buttonAircraft.Click += new System.EventHandler(this.ButtonAircraft_Click);
            // 
            // buttonSaveLocation
            // 
            this.buttonSaveLocation.Location = new System.Drawing.Point(16, 422);
            this.buttonSaveLocation.Name = "buttonSaveLocation";
            this.buttonSaveLocation.Size = new System.Drawing.Size(119, 23);
            this.buttonSaveLocation.TabIndex = 10;
            this.buttonSaveLocation.Text = "Save Location";
            this.buttonSaveLocation.UseVisualStyleBackColor = true;
            this.buttonSaveLocation.Click += new System.EventHandler(this.ButtonSaveLocation_Click);
            // 
            // textBoxSaveLocation
            // 
            this.textBoxSaveLocation.Enabled = false;
            this.textBoxSaveLocation.Location = new System.Drawing.Point(169, 422);
            this.textBoxSaveLocation.Name = "textBoxSaveLocation";
            this.textBoxSaveLocation.Size = new System.Drawing.Size(625, 23);
            this.textBoxSaveLocation.TabIndex = 9;
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
            this.TabPageCircuit.Controls.Add(this.ButtonCircuitDefault);
            this.TabPageCircuit.Controls.Add(this.label7);
            this.TabPageCircuit.Controls.Add(this.label6);
            this.TabPageCircuit.Controls.Add(this.label5);
            this.TabPageCircuit.Controls.Add(this.label4);
            this.TabPageCircuit.Controls.Add(this.label3);
            this.TabPageCircuit.Controls.Add(this.TextBoxCircuitUpwind);
            this.TabPageCircuit.Controls.Add(this.TextBoxCircuitSpeed);
            this.TabPageCircuit.Controls.Add(this.TextBoxCircuitHeight);
            this.TabPageCircuit.Controls.Add(this.TextBoxCircuitFinal);
            this.TabPageCircuit.Controls.Add(this.TextBoxCircuitBase);
            this.TabPageCircuit.Controls.Add(this.PictureBoxCircuit);
            this.TabPageCircuit.Location = new System.Drawing.Point(4, 24);
            this.TabPageCircuit.Name = "TabPageCircuit";
            this.TabPageCircuit.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageCircuit.Size = new System.Drawing.Size(812, 464);
            this.TabPageCircuit.TabIndex = 1;
            this.TabPageCircuit.Text = "Circuit";
            this.TabPageCircuit.UseVisualStyleBackColor = true;
            // 
            // ButtonCircuitDefault
            // 
            this.ButtonCircuitDefault.Location = new System.Drawing.Point(521, 15);
            this.ButtonCircuitDefault.Name = "ButtonCircuitDefault";
            this.ButtonCircuitDefault.Size = new System.Drawing.Size(75, 23);
            this.ButtonCircuitDefault.TabIndex = 11;
            this.ButtonCircuitDefault.Text = "Default";
            this.ButtonCircuitDefault.UseVisualStyleBackColor = true;
            this.ButtonCircuitDefault.Click += new System.EventHandler(this.ButtonCircuitDefault_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(727, 367);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 15);
            this.label7.TabIndex = 10;
            this.label7.Text = "Upwind";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(557, 367);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(39, 15);
            this.label6.TabIndex = 9;
            this.label6.Text = "Speed";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(387, 367);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 15);
            this.label5.TabIndex = 8;
            this.label5.Text = "Height";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(216, 367);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 15);
            this.label4.TabIndex = 7;
            this.label4.Text = "Final";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(46, 367);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "Base";
            // 
            // TextBoxCircuitUpwind
            // 
            this.TextBoxCircuitUpwind.Location = new System.Drawing.Point(697, 407);
            this.TextBoxCircuitUpwind.Name = "TextBoxCircuitUpwind";
            this.TextBoxCircuitUpwind.Size = new System.Drawing.Size(100, 23);
            this.TextBoxCircuitUpwind.TabIndex = 5;
            this.TextBoxCircuitUpwind.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitUpwind, "Distance between runway and gate 1 in miles");
            this.TextBoxCircuitUpwind.TextChanged += new System.EventHandler(this.TextBoxCircuitUpwind_TextChanged);
            // 
            // TextBoxCircuitSpeed
            // 
            this.TextBoxCircuitSpeed.Location = new System.Drawing.Point(527, 407);
            this.TextBoxCircuitSpeed.Name = "TextBoxCircuitSpeed";
            this.TextBoxCircuitSpeed.Size = new System.Drawing.Size(100, 23);
            this.TextBoxCircuitSpeed.TabIndex = 4;
            this.TextBoxCircuitSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitSpeed, "Cruise speed between gates 1 and 8 in knots");
            this.TextBoxCircuitSpeed.TextChanged += new System.EventHandler(this.TextBoxCircuitSpeed_TextChanged);
            // 
            // TextBoxCircuitHeight
            // 
            this.TextBoxCircuitHeight.Location = new System.Drawing.Point(356, 407);
            this.TextBoxCircuitHeight.Name = "TextBoxCircuitHeight";
            this.TextBoxCircuitHeight.Size = new System.Drawing.Size(100, 23);
            this.TextBoxCircuitHeight.TabIndex = 3;
            this.TextBoxCircuitHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitHeight, "Height of circuit above runway in feet");
            this.TextBoxCircuitHeight.TextChanged += new System.EventHandler(this.TextBoxCircuitHeight_TextChanged);
            // 
            // TextBoxCircuitFinal
            // 
            this.TextBoxCircuitFinal.Location = new System.Drawing.Point(185, 407);
            this.TextBoxCircuitFinal.Name = "TextBoxCircuitFinal";
            this.TextBoxCircuitFinal.Size = new System.Drawing.Size(100, 23);
            this.TextBoxCircuitFinal.TabIndex = 2;
            this.TextBoxCircuitFinal.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitFinal, "Distance between gate 8 and runway in miles");
            this.TextBoxCircuitFinal.TextChanged += new System.EventHandler(this.TextBoxCircuitFinal_TextChanged);
            // 
            // TextBoxCircuitBase
            // 
            this.TextBoxCircuitBase.Location = new System.Drawing.Point(14, 407);
            this.TextBoxCircuitBase.Name = "TextBoxCircuitBase";
            this.TextBoxCircuitBase.Size = new System.Drawing.Size(100, 23);
            this.TextBoxCircuitBase.TabIndex = 1;
            this.TextBoxCircuitBase.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip1.SetToolTip(this.TextBoxCircuitBase, "Distance between gates 2 and 3 (6 and 7) in miles");
            this.TextBoxCircuitBase.TextChanged += new System.EventHandler(this.TextBoxCircuitBase_TextChanged);
            // 
            // PictureBoxCircuit
            // 
            this.PictureBoxCircuit.Image = ((System.Drawing.Image)(resources.GetObject("PictureBoxCircuit.Image")));
            this.PictureBoxCircuit.InitialImage = ((System.Drawing.Image)(resources.GetObject("PictureBoxCircuit.InitialImage")));
            this.PictureBoxCircuit.Location = new System.Drawing.Point(15, 15);
            this.PictureBoxCircuit.Name = "PictureBoxCircuit";
            this.PictureBoxCircuit.Size = new System.Drawing.Size(783, 325);
            this.PictureBoxCircuit.TabIndex = 0;
            this.PictureBoxCircuit.TabStop = false;
            // 
            // ButtonGenerateScenario
            // 
            this.ButtonGenerateScenario.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.ButtonGenerateScenario.Location = new System.Drawing.Point(344, 531);
            this.ButtonGenerateScenario.Name = "ButtonGenerateScenario";
            this.ButtonGenerateScenario.Size = new System.Drawing.Size(152, 43);
            this.ButtonGenerateScenario.TabIndex = 1;
            this.ButtonGenerateScenario.Text = "Generate Scenario";
            this.ButtonGenerateScenario.UseVisualStyleBackColor = true;
            this.ButtonGenerateScenario.Click += new System.EventHandler(this.ButtonGenerateScenario_Click);
            // 
            // Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(844, 606);
            this.Controls.Add(this.ButtonGenerateScenario);
            this.Controls.Add(this.TabControl);
            this.Name = "Form";
            this.Text = "P3D Scenario Generator";
            this.TabControl.ResumeLayout(false);
            this.TabPageGeneral.ResumeLayout(false);
            this.TabPageGeneral.PerformLayout();
            this.TabPageCircuit.ResumeLayout(false);
            this.TabPageCircuit.PerformLayout();
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
        private System.Windows.Forms.Button buttonSaveLocation;
        private System.Windows.Forms.Button buttonAircraft;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button ButtonCircuitDefault;
        internal System.Windows.Forms.TextBox textBoxSaveLocation;
        internal System.Windows.Forms.ListBox ListBoxRunways;
        internal System.Windows.Forms.TextBox TextBoxSelectedRunway;
        internal System.Windows.Forms.TextBox TextBoxSelectedScenario;
        internal System.Windows.Forms.ListBox ListBoxScenarioType;
        internal System.Windows.Forms.TextBox TextBoxCircuitUpwind;
        internal System.Windows.Forms.TextBox TextBoxCircuitSpeed;
        internal System.Windows.Forms.TextBox TextBoxCircuitHeight;
        internal System.Windows.Forms.TextBox TextBoxCircuitFinal;
        internal System.Windows.Forms.TextBox TextBoxCircuitBase;
        internal System.Windows.Forms.ListBox ListBoxAircraft;
    }
}

