using System.Drawing;
using System.Runtime.InteropServices;

namespace SchattenclownBot.Model.HelpClasses
{
    internal class ColorMath
    {
        public static Color GetDominantColor(Bitmap bitmap)
        {
            //Used for tally
            int r = 0;
            int g = 0;
            int b = 0;

            int total = 0;

            for (int x = 0; x < (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? bitmap.Width : 0); x++)
            {
                for (int y = 0; y < (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? bitmap.Width : 0); y++)
                {
                    Color clr = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? bitmap.GetPixel(x, y) : Color.Black;

                    r += clr.R;
                    g += clr.G;
                    b += clr.B;

                    total++;
                }
            }

            //Calculate average
            // ReSharper disable once InvertIf
            if (total != 0)
            {
                r /= total;
                g /= total;
                b /= total;
            }

            return Color.FromArgb(r, g, b);
        }
    }
}