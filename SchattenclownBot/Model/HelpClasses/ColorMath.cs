// Copyright (c) Schattenclown

using System.Drawing;

namespace SchattenclownBot.Model.HelpClasses;

internal class ColorMath
{
	public static Color GetDominantColor(Bitmap bitmap)
	{
		var r = 0;
		var g = 0;
		var b = 0;

		var total = 0;

		try
		{
			for (var x = 0; x < bitmap.Width; x++)
			{
				for (var y = 0; y < bitmap.Height; y++)
				{
					var clr = bitmap.GetPixel(x, y);

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