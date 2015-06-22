using System.Drawing;
using System.Drawing.Imaging;

namespace CommonUtils.CommonMath.Wavelets.HaarCSharp
{
	/// <summary>
	/// Created by Hovhannes Bantikyan under the The Code Project Open License
	/// </summary>
	public class UnsafeColorChannels : ColorChannels
	{
		private const int PixelSize = 3;

		private BitmapData bitmapData;

		public UnsafeColorChannels(int width, int height)
			: base(width, height)
		{
		}

		public override void MergeColors(Bitmap bmp)
		{
			unsafe
			{
				for (var j = 0; j < this.bitmapData.Height; j++)
				{
					var row = (byte*)this.bitmapData.Scan0 + (j * this.bitmapData.Stride);
					for (var i = 0; i < this.bitmapData.Width; i++)
					{
						row[i * PixelSize + 2] = (byte)Scale(-1, 1, 0, 255, Red[i][j]);
						row[i * PixelSize + 1] = (byte)Scale(-1, 1, 0, 255, Green[i][j]);
						row[i * PixelSize] = (byte)Scale(-1, 1, 0, 255, Blue[i][j]);
					}
				}
			}

			bmp.UnlockBits(this.bitmapData);
		}

		public override void SeparateColors(Bitmap bmp)
		{
			this.bitmapData = bmp.LockBits(
				new Rectangle(0, 0, bmp.Width, bmp.Height),
				ImageLockMode.ReadWrite,
				PixelFormat.Format24bppRgb);

			unsafe
			{
				for (var j = 0; j < this.bitmapData.Height; j++)
				{
					var row = (byte*)this.bitmapData.Scan0 + (j * this.bitmapData.Stride);
					for (var i = 0; i < this.bitmapData.Width; i++)
					{
						Red[i][j] = Scale(0, 255, -1, 1, row[i * PixelSize + 2]);
						Green[i][j] = Scale(0, 255, -1, 1, row[i * PixelSize + 1]);
						Blue[i][j] = Scale(0, 255, -1, 1, row[i * PixelSize]);
					}
				}
			}
		}
	}
}