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
            letterPath['A'] = 3948336;
            letterPath['B'] = 4178723;
            letterPath['C'] = 16131;
            letterPath['D'] = 4178691;
            letterPath['E'] = 16179;
            letterPath['F'] = 16176;
            letterPath['G'] = 3161891;
            letterPath['H'] = 3947568;
            letterPath['I'] = 246531;
            letterPath['J'] = 3944451;
            letterPath['K'] = 2473044;
            letterPath['L'] = 15363;
            letterPath['M'] = 4144896;
            letterPath['N'] = 4045956;
            letterPath['O'] = 3948291;
            letterPath['P'] = 802608;
            letterPath['Q'] = 3981099;
            letterPath['R'] = 2932532;
            letterPath['S'] = 2197383;
            letterPath['T'] = 246528;
            letterPath['U'] = 3947523;
            letterPath['V'] = 3968020;
            letterPath['W'] = 3996675;
            letterPath['X'] = 2467020;
            letterPath['Y'] = 838704;
            letterPath['Z'] = 369483;
        //    letterPath[''] = ;
        }

        static internal bool SegmentIsSet(char letter, int bitPosition)
        {
            string letterBinary = Convert.ToString(letterPath[letter], 2);
            int letterLength = letterBinary.Length;

            if (bitPosition >= letterLength)
            {
                return false;
            }
            else
            {
                if (letterBinary[letterLength - 1 - bitPosition] == '1')
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        static internal double GetSignWritingDistance()
        {
            return 0;
        }
    }
}
