using System;

namespace CommonUtils.CommonMath.Wavelets.Compress
{
	/// Author: Linzhi Qi
	/// Converted from C++ by perivar@nerseth.com
	/// https://github.com/linzhi/wavelet-compress
	public static class WaveletComDec
	{
		public static void CompressDecompress2D(double[][] data_input, int level, int threshold)
		{
			int temp_level = level;

			int ex_height = data_input.Length;
			int ex_width = data_input[0].Length;
			
			int temp_ex_height = ex_height;
			int temp_ex_width = ex_width;

			while (temp_level > 0 && ex_height > 1 && ex_width > 1)
			{
				HaarWaveletTransform.HaarTransform2D(data_input, ex_height, ex_width);

				if (ex_width > 1)
					ex_width = ex_width / 2;
				if (ex_height > 1)
					ex_height = ex_height / 2;

				temp_level--;
			}

			Quantize.DataQuantize2D(data_input, temp_ex_height, temp_ex_width, threshold);

			while (temp_level < level && ex_height > 1 && ex_width > 1)
			{
				if (ex_width > 1)
					ex_width = ex_width * 2;
				if (ex_height > 1)
					ex_height = ex_height * 2;

				HaarWaveletTransform.InverseHaarTransform2D(data_input, ex_height, ex_width);

				temp_level++;
			}
		}
	}
}

