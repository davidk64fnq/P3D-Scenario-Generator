using System;
using System.Collections.Generic;
using System.Text;

namespace P3D_Scenario_Generator
{
    internal class SignWriting
    {
        private static readonly int[] letterPath = new int[123];

        static internal void InitLetterPaths()
        {
            letterPath['@'] = 4194303;  // Test character all segments turned on
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
            letterPath['a'] = 3684592;
            letterPath['b'] = 3784931;
            letterPath['c'] = 14531;
            letterPath['d'] = 3784899;
            letterPath['e'] = 14579;
            letterPath['f'] = 14576;
            letterPath['g'] = 2111684;
            letterPath['h'] = 3684400;
            letterPath['i'] = 114883;
            letterPath['j'] = 3678211;
            letterPath['k'] = 2668572;
            letterPath['l'] = 14339;
            letterPath['m'] = 3782848;
            letterPath['n'] = 3717140;
            letterPath['o'] = 3684547;
            letterPath['p'] = 538864;
            letterPath['q'] = 3717355;
            letterPath['r'] = 2668776;
            letterPath['s'] = 2132183;
            letterPath['t'] = 114880;
            letterPath['u'] = 3684355;
            letterPath['v'] = 1853460;
            letterPath['w'] = 3733507;
            letterPath['x'] = 2664508;
            letterPath['y'] = 575536;
            letterPath['z'] = 565483;
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
