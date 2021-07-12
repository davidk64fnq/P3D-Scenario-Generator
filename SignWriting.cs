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
            letterPath['a'] = 3277760;
            letterPath['b'] = 2230208;
            letterPath['c'] = 2228992;
            letterPath['d'] = 2229216;
            letterPath['e'] = 2491200;
            letterPath['f'] = 393984;
            letterPath['g'] = 2491072;
            letterPath['h'] = 133056;
            letterPath['i'] = 192;
            letterPath['j'] = 2097344;
            letterPath['k'] = 395168;
            letterPath['l'] = 2098944;
            letterPath['m'] = 197571;
            letterPath['n'] = 132032;
            letterPath['o'] = 2229184;
            letterPath['p'] = 394048;
            letterPath['q'] = 393920;
            letterPath['r'] = 131840;
            letterPath['s'] = 2491008;
            letterPath['t'] = 196832;
            letterPath['u'] = 2098112;
            letterPath['v'] = 262848;
            letterPath['w'] = 3146691;
            letterPath['x'] = 263104;
            letterPath['y'] = 2360000;
            letterPath['z'] = 2490688;
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
