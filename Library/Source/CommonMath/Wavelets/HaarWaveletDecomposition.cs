using System;

namespace CommonUtils.CommonMath.Wavelets
{
	public abstract class HaarWaveletDecomposition
	{
		static readonly double SQRT2 = Math.Sqrt(2.0);
		
		public abstract void DecomposeImageInPlace(double[][] image);

		// A Modified version of 1D Haar Transform, used by the 2D Haar Transform function
		protected static void DecompositionStep(double[] array, int h)
		{
			var temp = new double[h];

			h /= 2;
			for (int i = 0; i < h; i++)
			{
				temp[i] = (array[2 * i] + array[(2 * i) + 1]) / SQRT2;
				temp[i + h] = (array[2 * i] - array[(2 * i) + 1]) / SQRT2;
			}

			for (int i = 0; i < (h * 2); i++)
			{
				array[i] = temp[i];
			}
			
			temp = null;
		}
	}
}