using System.Drawing;
using CommonUtils;
using System.Linq;

namespace CommonUtils.MathLib.Wavelets.HaarCSharp
{
	/// <summary>
	/// Created by Hovhannes Bantikyan under the The Code Project Open License
	/// Heavily modified by Per Ivar Nerseth
	/// </summary>
	public class SafeColorChannels : ColorChannels
	{
		public SafeColorChannels(int width, int height)
			: base(width, height)
		{
		}
		
		public override void MergeColors(Bitmap bmp)
		{
			double minRed = MathUtils.Min(Red);
			double maxRed = MathUtils.Max(Red);
			double minGreen = MathUtils.Min(Green);
			double maxGreen = MathUtils.Max(Green);
			double minBlue = MathUtils.Min(Blue);
			double maxBlue = MathUtils.Max(Blue);
			
			double min = MathUtils.Min(new double[] { minRed, minGreen, minBlue });
			double max = MathUtils.Max(new double[] { maxRed, maxGreen, maxBlue });

			for (var j = 0; j < bmp.Height; j++)
			{
				for (var i = 0; i < bmp.Width; i++)
				{
					/*
					bmp.SetPixel(i, j,
					             Color.FromArgb(
					             	(int)Scale(-1, 1, 0, 255, Red[i][j]),
					             	(int)Scale(-1, 1, 0, 255, Green[i][j]),
					             	(int)Scale(-1, 1, 0, 255, Blue[i][j])));
					 */
					/*
					bmp.SetPixel(i, j,
					             Color.FromArgb(
					             	(int)Scale(minRed, maxRed, 0, 255, Red[i][j]),
					             	(int)Scale(minGreen, maxGreen, 0, 255, Green[i][j]),
					             	(int)Scale(minBlue, maxBlue, 0, 255, Blue[i][j])));
					 */
					bmp.SetPixel(i, j,
					             Color.FromArgb(
					             	(int)Scale(min, max, 0, 255, Red[i][j]),
					             	(int)Scale(min, max, 0, 255, Green[i][j]),
					             	(int)Scale(min, max, 0, 255, Blue[i][j])));
					
				}
			}
		}

		public override void SeparateColors(Bitmap bmp)
		{
			for (var j = 0; j < bmp.Height; j++)
			{
				for (var i = 0; i < bmp.Width; i++)
				{
					var c = bmp.GetPixel(i, j);
					Red[i][j] = Scale(0, 255, -1, 1, c.R);
					Green[i][j] = Scale(0, 255, -1, 1, c.G);
					Blue[i][j] = Scale(0, 255, -1, 1, c.B);
				}
			}
		}
	}
}