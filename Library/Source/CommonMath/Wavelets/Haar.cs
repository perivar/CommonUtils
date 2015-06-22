using System;

namespace CommonUtils.CommonMath.Wavelets
{
	// c++ version can be found here:
	// http://www.cs.ucf.edu/~mali/haar/haar.cpp
	public static class Haar
	{
		static double SQRT2 = Math.Sqrt(2.0);
		
		/// <summary>
		/// The 1D Haar Transform
		/// </summary>
		/// <param name="vec">vector</param>
		/// <param name="n">length</param>
		public static void Haar1D(double[] vec, int n)
		{
			int i = 0;
			int w = n;
			var vecp = new double[n];

			while( w > 1 )
			{
				w/=2;
				for(i = 0; i < w; i++)
				{
					vecp[i] = (vec[2 * i] + vec[2 * i+1]) / SQRT2;;
					vecp[i+w] = (vec[2 * i] - vec[2 * i+1]) / SQRT2;;
				}

				for(i = 0; i < (w * 2); i++)
				{
					vec[i] = vecp[i];
				}
			}

			vecp = null;
		}

		/// <summary>
		/// A Modified version of 1D Haar Transform, used by the 2D Haar Transform function
		/// </summary>
		/// <param name="vec">vector</param>
		/// <param name="n">n</param>
		/// <param name="w">w</param>
		private static void Haar1DModified(double[] vec, int n, int w)
		{
			int i = 0;
			var vecp = new double[n];

			w/=2;
			for(i = 0; i < w; i++)
			{
				vecp[i] = (vec[2 * i] + vec[2 * i+1]) / SQRT2;;
				vecp[i+w] = (vec[2 * i] - vec[2 * i +1]) / SQRT2;;
			}

			for(i = 0; i < (w * 2); i++) {
				vec[i] = vecp[i];
			}

			vecp = null;
		}

		/// <summary>
		/// The 2D Haar Transform
		/// </summary>
		/// <param name="matrix">matrix</param>
		/// <param name="rows">rows</param>
		/// <param name="cols">columns</param>
		public static void Haar2D(double[][] matrix, int rows, int cols)
		{
			var temp_row = new double[cols];
			var temp_col = new double[rows];

			int i = 0;
			int j = 0;
			int w = cols;
			int h = rows;
			while(w > 1 || h > 1)
			{
				if(w > 1)
				{
					for(i = 0; i < h; i++)
					{
						for(j = 0; j < cols; j++) {
							temp_row[j] = matrix[i][j];
						}

						Haar1DModified(temp_row, cols, w);

						for(j = 0; j < cols; j++) {
							matrix[i][j] = temp_row[j];
						}
					}
				}

				if(h > 1)
				{
					for(i = 0; i < w; i++)
					{
						for(j = 0; j < rows; j++) {
							temp_col[j] = matrix[j][i];
						}
						
						Haar1DModified(temp_col, rows, h);
						
						for(j = 0; j < rows; j++) {
							matrix[j][i] = temp_col[j];
						}
					}
				}

				if(w > 1) {
					w/=2;
				}
				if(h > 1) {
					h/=2;
				}
			}

			temp_row = null;
			temp_col = null;
		}
	}
}