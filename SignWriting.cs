using System;
using System.Collections.Generic;
using System.Text;

namespace P3D_Scenario_Generator
{
    internal class SignWriting
    {
        private static readonly int[] letterPath = new int[122];

        static internal void InitLetterPaths()
        {
            letterPath['A'] = 52284;
            System.Windows.Forms.MessageBox.Show(Convert.ToString(letterPath['A'], 2));
        }

        static internal double GetSignWritingDistance()
        {
            return 0;
        }
    }
}
