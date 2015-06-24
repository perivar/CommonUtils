using System;

namespace CommonUtils.CommonMath.Wavelets.Compress
{
	/// Author: Linzhi Qi
	/// Converted from C++ by perivar@nerseth.com
	/// https://github.com/linzhi/wavelet-compress
	public static class Quantize
	{
		/// <summary>
		/// Quantize the data so that any absolute value underneath the threshold is set to zero
		/// </summary>
		/// <param name="data_input">data matrix</param>
		/// <param name="height">height</param>
		/// <param name="width">width</param>
		/// <param name="threshold">threshold where any absolute value less than this is set to zero</param>
		public static void DataQuantize2D(double[][] data_input, int height, int width, int threshold)
		{
			//if (threshold > 255)
			//	threshold = 255;
			if (threshold < 0)
				threshold = 0;

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					double temp = data_input[i][j];
					if (Math.Abs(temp) <= threshold)
						data_input[i][j] = 0.0;
				}
			}
		}

		/// <summary>
		/// Quantize the data so that any absolute value underneath the threshold is set to zero
		/// </summary>
		/// <param name="data_input">data matrix</param>
		/// <param name="length"></param>
		/// <param name="width">width</param>
		/// <param name="height">height</param>
		/// <param name="threshold">threshold where any absolute value less than this is set to zero</param>
		public static void DataQuantize3D(double[][][] data_input, int length, int width, int height, int threshold)
		{
			//if (threshold > 255)
			//	threshold = 255;
			if (threshold < 0)
				threshold = 0;

			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < width; j++)
				{
					for (int k = 0; k < height; k++)
					{
						double temp = data_input[i][j][k];
						if (Math.Abs(temp) <= threshold)
							data_input[i][j][k] = 0.0;
					}
				}
			}
		}
	}
}