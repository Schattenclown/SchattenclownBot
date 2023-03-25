using System.Drawing;

namespace SchattenclownBot.Model.HelpClasses
{
   internal class ColorMath
   {
      public static Color GetDominantColor(Bitmap bitmap)
      {
         int r = 0;
         int g = 0;
         int b = 0;

         int total = 0;

         try
         {
            for (int x = 0; x < bitmap.Width; x++)
            {
               for (int y = 0; y < bitmap.Height; y++)
               {
                  Color clr = bitmap.GetPixel(x, y);

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
         catch
         {
            return Color.FromArgb(255, 0, 0);
         }
      }
   }
}