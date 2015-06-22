using System.Drawing;

namespace CommonUtils.CommonMath.Wavelets.HaarCSharp
{
	/// <summary>
	/// Created by Hovhannes Bantikyan under the The Code Project Open License
	/// </summary>
	public class SafeColorChannels : ColorChannels
	{
		public SafeColorChannels(int width, int height)
			: base(width, height)
		{
		}

		public override void MergeColors(Bitmap bmp)
		{
			for (var j = 0; j < bmp.Height; j++)
			{
				for (var i = 0; i < bmp.Width; i++)
				{
					bmp.SetPixel(i, j,
					             Color.FromArgb(
					             	(int)Scale(-1, 1, 0, 255, Red[i][j]),
					             	(int)Scale(-1, 1, 0, 255, Green[i][j]),
					             	(int)Scale(-1, 1, 0, 255, Blue[i][j])));
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