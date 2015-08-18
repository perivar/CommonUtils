using System;
using System.Linq;

using CommonUtils.MathLib.MatrixLib;

namespace CommonUtils.MathLib.Wavelets
{
	/// <summary>
	/// Description of Thresholding.
	/// </summary>
	public static class Thresholding
	{
		#region ThrowAway by Emil Mikulic
		public const int SIZE = 256;
		public const int MAX_ITER = 250;
		
		/// <summary>
		/// Returns the percentage of coefficients under <amount>
		/// </summary>
		/// <param name="data">coefficients</param>
		/// <param name="amount">threshold amount</param>
		/// <returns>the percentage of coefficients under amount</returns>
		/// <remarks>
		/// Copyright (c) 2003 Emil Mikulic.
		/// http://dmr.ath.cx/
		/// </remarks>
		public static double PercentUnder(double[][] data, double amount)
		{
			int num_thrown = 0;
			int x;
			int y;

			for (y = 0; y < SIZE; y++)
				for (x = 0; x < SIZE; x++)
					if (Math.Abs(data[y][x]) <= amount)
						num_thrown++;

			return (double)(100 * num_thrown) / (double)(SIZE * SIZE);
		}

		/// <summary>
		/// Throw away weakest % of coefficients
		/// </summary>
		/// <param name="data">coefficients</param>
		/// <param name="percentage">how many percent to throw away</param>
		/// <remarks>
		/// Copyright (c) 2003 Emil Mikulic.
		/// http://dmr.ath.cx/
		/// </remarks>
		public static void ThrowAway(double[][] data, double percentage)
		{
			double low;
			double high;
			double thresh = 0;
			double loss;
			int i;
			int j;

			// find max
			low = high = 0.0;
			for (j =0; j < SIZE; j++)
				for (i =0; i < SIZE; i++)
					if (Math.Abs(data[j][i]) > high)
						high = Math.Abs(data[j][i]);

			// binary search
			for (i = 0; i < MAX_ITER; i++)
			{
				thresh = (low+high)/2.0;
				loss = PercentUnder(data, thresh);

				//Console.Write("binary search: " + "iteration={0,4:D}, thresh={1,4:f}, loss={2,3:f2}%\r", i+1, thresh, loss);
				
				if (loss < percentage) {
					low = thresh;
				} else {
					high = thresh;
				}

				if (Math.Abs(loss - percentage) < 0.01)
					i = MAX_ITER;
				if (Math.Abs(low - high) < 0.0000001)
					i = MAX_ITER;
			}

			// zero out anything too low
			for (j = 0; j < SIZE; j++)
				for (i = 0; i < SIZE; i++)
					if (Math.Abs(data[j][i]) < thresh)
						data[j][i] = 0.0;

		}
		#endregion
		
		/// <summary>
		/// Keep only the s largest coefficients in each column of X
		/// </summary>
		/// <param name="x">data</param>
		/// <param name="s">the number of coefficients to keep</param>
		/// <returns>data after thresholding</returns>
		public static double[][] PerformStrictThresholding(double[][] x, int s) {
			// Copyright (c) 2006 Gabriel Peyre
			// v = sort(abs(x)); v = v(end:-1:1,:);
			// v = v(round(s),:);
			// v = repmat(v, [size(x,1) 1]);
			// X = X .* (abs(x)>=v);

			int rowCount = x.Length;
			int columnCount = x[0].Length;
			var y = new double[rowCount][];
			for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
			{
				double[] row = x[rowIndex];
				
				// Find top S coefficients-indexes by doing an absolute value sort
				int[] topIndexes = (from t in row.Select((n, i) => new { Index = i, Value = n })
				                    orderby Math.Abs(t.Value) descending
				                    select t.Index).Take(s).ToArray();

				// Empty all values except the top S coefficients
				y[rowIndex] = new double[row.Length];
				for (int i = 0; i < row.Length; i++) {
					if (topIndexes.Contains(i)) {
						y[rowIndex][i] = row[i];
					} else {
						y[rowIndex][i] = 0;
					}
				}
			}
			
			return y;
		}

		/// <summary>
		/// Hard thresholding sets any coefficient less than or equal to the threshold to zero.
		/// </summary>
		/// <param name="x">data</param>
		/// <param name="thresh">threshold where coefficient less than or equal will be set to zero</param>
		/// <returns>data after thresholding</returns>
		public static double[][] PerformHardThresholding(double[][] x, double thresh) {
			// Hard thresholding can be described as the usual process of setting to zero the elements whose absolute values are lower than the threshold.
			// The hard threshold signal is x if |x| > t, and is 0 if |x| <= t.

			// http://sfb649.wiwi.hu-berlin.de/fedc_homepage/xplore/tutorials/xlghtmlnode93.html
			// Copyright (c) 2006 Gabriel Peyre
			// t = t(1);
			// y = x .* (abs(x) > t);
			
			double[][] y = x.DeepCopy();
			
			for (int i = 0; i < y.Length; i++) {
				for (int j = 0; j < y[i].Length; j++) {
					if (Math.Abs(y[i][j]) <= thresh) {
						y[i][j] = 0.0;
					}
				}
			}
			return y;
		}

		/// <summary>
		/// Soft thresholding sets any coefficient less than or equal to the threshold to zero.
		/// The threshold is subtracted from any coefficient that is greater than the threshold.
		/// This moves the time series toward zero.
		/// </summary>
		/// <param name="x">data</param>
		/// <param name="thresh">threshold</param>
		/// <returns>data after thresholding</returns>
		public static double[][] PerformSoftThresholding(double[][] x, double thresh) {
			// Soft thresholding is an extension of hard thresholding, first setting to zero the elements whose absolute values are lower than the threshold,
			// and then shrinking the nonzero coefficients towards 0
			// The soft threshold signal is sign(x)(|x| - t) if |x| > t and is 0 if |x| <= t.

			// http://sfb649.wiwi.hu-berlin.de/fedc_homepage/xplore/tutorials/xlghtmlnode93.html
			// Copyright (c) 2006 Gabriel Peyre
			// t = t(1);
			// s = abs(x) - t;
			// s = (s + abs(s))/2;
			// y = sign(x) .* s;

			double[][] y = x.DeepCopy();
			
			for (int i = 0; i < y.Length; i++) {
				for (int j = 0; j < y[i].Length; j++) {
					if (Math.Abs(y[i][j]) <= thresh) {
						y[i][j] = 0.0;
					} else {
						if (y[i][j] > 0) {
							y[i][j] = y[i][j] - thresh;
						} else if (y[i][j] < 0) {
							y[i][j] = y[i][j] + thresh;
						} else {
							y[i][j] = 0;
						}
					}
				}
			}
			return y;
		}
		
		/// <summary>
		/// Semi-soft thresholding is a family of non-linearities that interpolates between soft and hard thresholding.
		/// It uses both a main threshold T and a secondary threshold T1=mu*T.
		/// When mu=1, the semi-soft thresholding performs a hard thresholding,
		/// whereas when mu=infinity, it performs a soft thresholding.
		/// </summary>
		/// <param name="x">data</param>
		/// <param name="thresh1">main threshold</param>
		/// <param name="thresh2">secondary threshold</param>
		/// <returns>data after thresholding</returns>
		public static double[][] PerformSemisoftThresholding(double[][] x, double thresh1, double thresh2) {
			// Semi-soft thresholding is a family of non-linearities that interpolates between soft and hard thresholding.
			// It uses both a main threshold T and a secondary threshold T1=mu*T.
			// When mu=1, the semi-soft thresholding performs a hard thresholding,
			// whereas when mu=infinity, it performs a soft thresholding.
			
			// Copyright (c) 2006 Gabriel Peyre
			// if length(t)==1
			//     t = [t 2*t];
			// end
			// t = sort(t);
			// 
			// y = x;
			// y(abs(x) < t(1)) = 0;
			// I = find( abs(x) >= t(1) & abs(x) < t(2) );
			// y( I ) = sign(x(I)) .* t(2)/(t(2)-t(1)) .* (abs(x(I)) - t(1));
			
			double[][] y = x.DeepCopy();
			
			for (int i = 0; i < y.Length; i++) {
				for (int j = 0; j < y[i].Length; j++) {
					if (Math.Abs(y[i][j]) < thresh1) {
						y[i][j] = 0.0;
					}
				}
			}
			
			int rowCount = x.Length;
			int columnCount = x[0].Length;
			var I = new int[rowCount][];
			for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
			{
				double[] row = x[rowIndex];
				
				// find indices for those values that are between thresh1 and thresh2
				int[] indices = Enumerable.Range(0, row.Length)
					.Where(index => Math.Abs(row[index]) >= thresh1 && Math.Abs(row[index]) < thresh2 )
					.ToArray();
				
				I[rowIndex] = indices;
			}

			double threshold = thresh2/(thresh2-thresh1);
			
			for (int rowIndex = 0; rowIndex < I.Length; rowIndex++) {
				for (int columnIndex = 0; columnIndex < I[rowIndex].Length; columnIndex++) {
					int index = I[rowIndex][columnIndex];
					
					double xIndexValue = x[rowIndex][index];
					double fraction = (Math.Abs(xIndexValue) - thresh1);
					double interpolatedValue = threshold * fraction;

					if (xIndexValue > 0) {
						y[rowIndex][index] = interpolatedValue;
					} else if (xIndexValue < 0) {
						y[rowIndex][index] = interpolatedValue * -1;
					} else {
						y[rowIndex][index] = 0;
					}
				}
			}
			
			return y;
		}
	}
}
