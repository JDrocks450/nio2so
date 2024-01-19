using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Formats.Util
{
    internal static class GCol2Alph
    {
        /// <summary>
        /// Gets a new Pixel color that has had the <paramref name="AlphaColor"/> subtracted from it
        /// </summary>
        /// <param name="Pixel"></param>
        /// <param name="AlphaColor"></param>
        /// <param name="mA"></param>
        /// <param name="mX"></param>
        /// <returns></returns>
        public static Color ColorToAlpha(this Color Pixel, Color AlphaColor, double mA = 1.0, double mX = 255)
        {
            double pA = Pixel.A / 255.0;
            double p1 = Pixel.R;
            double p2 = Pixel.G;
            double p3 = Pixel.B;

            double r1 = AlphaColor.R;
            double r2 = AlphaColor.G;
            double r3 = AlphaColor.B;

            double aA, a1, a2, a3;
            // a1 calculation: minimal alpha giving r1 from p1
            if (p1 > r1) a1 = mA * (p1 - r1) / (mX - r1);
            else if (p1 < r1) a1 = mA * (r1 - p1) / r1;
            else a1 = 0.0;
            // a2 calculation: minimal alpha giving r2 from p2
            if (p2 > r2) a2 = mA * (p2 - r2) / (mX - r2);
            else if (p2 < r2) a2 = mA * (r2 - p2) / r2;
            else a2 = 0.0;
            // a3 calculation: minimal alpha giving r3 from p3
            if (p3 > r3) a3 = mA * (p3 - r3) / (mX - r3);
            else if (p3 < r3) a3 = mA * (r3 - p3) / r3;
            else a3 = 0.0;
            // aA calculation: max(a1, a2, a3)
            aA = a1;
            if (a2 > aA) aA = a2;
            if (a3 > aA) aA = a3;
            // apply aA to pixel:
            if (aA >= mA / mX)
            {
                pA = aA * pA / mA;
                p1 = mA * (p1 - r1) / aA + r1;
                p2 = mA * (p2 - r2) / aA + r2;
                p3 = mA * (p3 - r3) / aA + r3;
            }
            else
            {
                pA = 0;
                p1 = 0;
                p2 = 0;
                p3 = 0;
            }
            return Color.FromArgb((int)(pA * 255), (int)(p1), (int)(p2), (int)(p3));
        }
    }
}
