using System;
using System.Globalization;

namespace CommonUtils.MathLib.MatrixLib
{
	/// <summary>
	/// Copied from https://github.com/voxsim/discrete-cosine-transform
	/// </summary>
	public static class DctMethods
	{
		/// <summary>
		/// Method that calculate the Discrete Cosine Transform in one dimension.
		/// </summary>
		/// <param name="A">signal array</param>
		/// <returns>returns the one-dimensional discrete cosine transform of A.
		/// The resulting array B is the same size as A and
		/// contains the discrete cosine transform coefficients B(k1,k2).</returns>
		public static double[][] dct(double[][] A)
		{
			return dct(A, 0);
		}
		
		/// <summary>
		/// Method that calculate the Discrete Cosine Transform in one dimension.
		/// </summary>
		/// <param name="A">signal array</param>
		/// <param name="offset">value to add or substract to the signal value before
		/// multiplying with the Cosine Tranform</param>
		/// <returns>returns the one-dimensional discrete cosine transform of A.
		/// The resulting array B is the same size as A and
		/// contains the discrete cosine transform coefficients B(k1,k2).</returns>
		public static double[][] dct(double[][] A, double offset)
		{
			if (A.Length == 0)
				throw new Exception("A empty");

			if (A[0].Length == 0)
				throw new Exception("A row empty");

			int n = A.Length;
			int m = A[0].Length;

			var B = new double[n][];
			for (int i=0;i<n;i++)
				B[i] = new double[m];
			
			double alfa;
			double sum;

			for (int k = 0; k < n; k++)
			{
				for (int l = 0; l < m; l++)
				{
					sum = 0;
					for (int i = 0; i < n; i++)
					{
						sum += (A[i][l] + offset) * Math.Cos((Math.PI * (2.0 * i + 1.0) * k) / (2.0 * n));
					}
					alfa = k == 0 ? 1.0 / Math.Sqrt(n) : Math.Sqrt(2.0 / n);
					B[k][l] = alfa * sum;
				}
			}

			return B;
		}

		/// <summary>
		/// Method that calculate the Inverse Discrete Cosine Transformin one dimension.
		/// </summary>
		/// <param name="A">signal array</param>
		/// <returns>returns the one-dimensional inverse discrete cosine transform (DCT) of A.
		/// The resulting array B is the same size as A and
		/// contains the inverse discrete cosine transform coefficients B(k1,k2).</returns>
		public static double[][] idct(double[][] A)
		{
			return idct(A, 0);
		}
		
		/// <summary>
		/// Method that calculate the Inverse Discrete Cosine Transformin one dimension.
		/// </summary>
		/// <param name="A">signal array</param>
		/// <param name="offset">value to add or substract to the signal value before multiplying
		/// with the Cosine Tranform</param>
		/// <returns>returns the one-dimensional inverse discrete cosine transform (DCT) of A.
		/// The resulting array B is the same size as A and
		/// contains the inverse discrete cosine transform coefficients B(k1,k2).</returns>
		public static double[][] idct(double[][] A, double offset)
		{
			if (A.Length == 0)
				throw new Exception("A empty");

			if (A[0].Length == 0)
				throw new Exception("A row empty");

			int n = A.Length;
			int m = A[0].Length;

			var B = new double[n][];
			for (int i=0;i<n;i++)
				B[i] = new double[m];
			
			double alfa;

			for (int k = 0; k < n; k++)
			{
				for (int l = 0; l < m; l++)
				{
					B[k][l] = 0;
					for (int i = 0; i < n; i++)
					{
						alfa = i == 0 ? 1.0 / Math.Sqrt(n) : Math.Sqrt(2.0 / n);
						B[k][l] += alfa * A[i][l] * Math.Cos((Math.PI * (2.0 * k + 1) * i) / (2.0 * n));
					}
					B[k][l] += offset;
				}
			}

			return B;
		}

		/// <summary>
		/// Method that calculate the Discrete Cosine Transform2 in two dimensions, first
		/// calculate the Discrete Cosine Transform in row and after calculate the
		/// Discrete Cosine Transform in column
		/// </summary>
		/// <param name="A">signal array</param>
		/// <returns>returns the two-dimensional discrete cosine transform of A.
		/// The resulting array B is the same size as A and
		/// contains the discrete cosine transform coefficients B(k1,k2).</returns>
		public static double[][] dct2(double[][] A)
		{
			return dct2(A, 0);
		}
		
		/// <summary>
		/// Method that calculate the Discrete Cosine Transform2 in two dimensions, first
		/// calculate the Discrete Cosine Transform in row and after calculate the
		/// Discrete Cosine Transform in column
		/// </summary>
		/// <param name="A">signal array</param>
		/// <param name="offset">value to add or substract to the signal value before
		/// multiplying with the Cosine Tranform</param>
		/// <returns>returns the two-dimensional discrete cosine transform of A.
		/// The resulting array B is the same size as A and
		/// contains the discrete cosine transform coefficients B(k1,k2).</returns>
		public static double[][] dct2(double[][] A, double offset)
		{
			if (A.Length == 0)
				throw new Exception("A empty");

			if (A[0].Length == 0)
				throw new Exception("A row empty");

			int n = A.Length;
			int m = A[0].Length;

			var B = new double[n][];
			for (int i=0;i<n;i++)
				B[i] = new double[m];
			
			var C = new double[n][];
			for (int i=0;i<n;i++)
				C[i] = new double[m];

			double alfa;
			double sum;

			for (int k = 0; k < n; k++)
			{
				for (int l = 0; l < m; l++)
				{
					sum = 0;
					for (int i = 0; i < n; i++)
					{
						sum += (A[i][l] + offset) * Math.Cos((Math.PI * (2.0 * i + 1.0) * k) / (2.0 * n));
					}
					alfa = k == 0 ? 1.0 / Math.Sqrt(n) : Math.Sqrt(2.0 / n);
					B[k][l] = alfa * sum;
				}
			}

			for (int l = 0; l < m; l++)
			{
				for (int k = 0; k < n; k++)
				{
					sum = 0;
					for (int j = 0; j < m; j++)
					{
						sum += B[k][j] * Math.Cos((Math.PI * (2.0 * j + 1.0) * l) / (2.0 * m));
					}
					alfa = l == 0 ? 1.0 / Math.Sqrt(m) : Math.Sqrt(2.0 / m);
					C[k][l] = alfa * sum;
				}
			}

			return C;
		}

		/// <summary>
		/// Method that calculate idct2 in two dimensions, first
		/// calculate the Inverse Discrete Cosine Transformin row and after calculate the
		/// Inverse Discrete Cosine Transformin column
		/// </summary>
		/// <param name="A">signal array</param>
		/// <returns>returns the two-dimensional inverse discrete cosine transform (DCT) of A.
		/// The resulting array B is the same size as A and
		/// contains the inverse discrete cosine transform coefficients B(k1,k2).</returns>
		public static double[][] idct2(double[][] A)
		{
			return idct2(A, 0);
		}
		
		/// <summary>
		/// Method that calculate idct2 in two dimensions, first
		/// calculate the Inverse Discrete Cosine Transformin row and after calculate the
		/// Inverse Discrete Cosine Transformin column
		/// </summary>
		/// <param name="A">signal array</param>
		/// <param name="offset">value to add or substract to the signal value before
		/// multiplying with the Cosine Tranform</param>
		/// <returns>returns the two-dimensional inverse discrete cosine transform (DCT) of A.
		/// The resulting array B is the same size as A and
		/// contains the inverse discrete cosine transform coefficients B(k1,k2).</returns>
		public static double[][] idct2(double[][] A, double offset)
		{
			if (A.Length == 0)
				throw new Exception("A empty");

			if (A[0].Length == 0)
				throw new Exception("A row empty");

			int n = A.Length;
			int m = A[0].Length;

			var B = new double[n][];
			for (int i=0;i<n;i++)
				B[i] = new double[m];
			
			var C = new double[n][];
			for (int i=0;i<n;i++)
				C[i] = new double[m];

			double alfa;

			for (int k = 0; k < n; k++)
			{
				for (int l = 0; l < m; l++)
				{
					B[k][l] = 0;
					for (int i = 0; i < n; i++)
					{
						alfa = i == 0 ? 1.0 / Math.Sqrt(n) : Math.Sqrt(2.0 / n);
						B[k][l] += alfa * A[i][l] * Math.Cos((Math.PI * (2.0 * k + 1) * i) / (2.0 * n));
					}
				}
			}

			for (int l = 0; l < m; l++)
			{
				for (int k = 0; k < n; k++)
				{
					C[k][l] = 0;
					for (int j = 0; j < m; j++)
					{
						alfa = j == 0 ? 1.0 / Math.Sqrt(m) : Math.Sqrt(2.0 / m);
						C[k][l] += alfa * B[k][j] * Math.Cos((Math.PI * (2.0 * l + 1) * j) / (2.0 * m));
					}
					C[k][l] += offset;
				}
			}

			return C;
		}

		/// <summary>
		/// Method that calculate the Discrete Cosine Transform2 in two dimensions directly
		/// just as described here:
		/// http://www.mathworks.it/help/toolbox/images/ref/dct2.html
		/// </summary>
		/// <param name="A">signal array</param>
		/// <param name="offset">value to add or substract to the signal value before
		/// multiplying with the Cosine Tranform</param>
		/// <returns>returns the two-dimensional discrete cosine transform of A.
		/// The resulting array B is the same size as A and contains the
		/// discrete cosine transform coefficients B(k1,k2).</returns>
		/// <remarks>Even though this method is supposed to be faster than the dct2 version, it's not?!</remarks>
		public static double[][] dct2in2dimension(double[][] A, double offset)
		{
			if (A.Length == 0)
				throw new Exception("A empty");

			if (A[0].Length == 0)
				throw new Exception("A row empty");

			int n = A.Length;
			int m = A[0].Length;

			var B = new double[n][];
			for (int i=0;i<n;i++)
				B[i] = new double[m];
			
			var alf1 = new double[n];
			var alf2 = new double[m];

			alf1[0] = 1.0 / Math.Sqrt(n);
			for (int k = 1; k < n; k++)
			{
				alf1[k] = Math.Sqrt(2.0 / n);
			}

			alf2[0] = 1.0 / Math.Sqrt(m);
			for (int l = 1; l < m; l++)
			{
				alf2[l] = Math.Sqrt(2.0 / m);
			}

			double sum;
			for (int k = 0; k < n; k++)
			{
				for (int l = 0; l < m; l++)
				{
					sum = 0;
					for (int i = 0; i < n; i++)
					{
						for (int j = 0; j < m; j++)
						{
							sum += (A[i][j] + offset)
								* Math.Cos((Math.PI * (2.0 * i + 1) * k) / (2.0 * n))
								* Math.Cos((Math.PI * (2.0 * j + 1) * l) / (2.0 * m));
						}
					}
					B[k][l] = alf1[k] * alf2[l] * sum;
					//Console.WriteLine(k + " " + l + ": " + sum + "*" + alf1[k] + "*" + alf2[l] + " -> " + c[k][l]);
				}
			}

			return B;
		}
		
		/// <summary>
		/// Method that calculate idct2 in two dimensions directly
		/// just as described here:
		/// http://www.mathworks.it/help/toolbox/images/ref/idct2.html
		/// </summary>
		/// <param name="A">signal array</param>
		/// <param name="offset">value to add or substract to the signal value before
		/// multiplying with the Cosine Tranform</param>
		/// <returns>returns the two-dimensional inverse discrete cosine transform (DCT) of A.
		/// The resulting array B is the same size as A and
		/// contains the inverse discrete cosine transform coefficients B(k1,k2).</returns>
		/// <remarks>Even though this method is supposed to be faster than the dct2 version, it's not?!</remarks>
		public static double[][] idct2in2dimension(double[][] A, double offset)
		{
			if (A.Length == 0)
				throw new Exception("A empty");

			if (A[0].Length == 0)
				throw new Exception("A row empty");

			int n = A.Length;
			int m = A[0].Length;

			var B = new double[n][];
			for (int i=0;i<n;i++)
				B[i] = new double[m];
			
			var alf1 = new double[n];
			var alf2 = new double[m];

			alf1[0] = 1.0 / Math.Sqrt(n);
			for (int k = 1; k < n; k++)
			{
				alf1[k] = Math.Sqrt(2.0 / n);
			}

			alf2[0] = 1.0 / Math.Sqrt(m);
			for (int l = 1; l < m; l++)
			{
				alf2[l] = Math.Sqrt(2.0 / m);
			}

			for (int k = 0; k < n; k++)
			{
				for (int l = 0; l < m; l++)
				{
					B[k][l] = 0;
					for (int i = 0; i < n; i++)
					{
						for (int j = 0; j < m; j++)
						{
							B[k][l] += alf1[i] * alf2[j] * A[i][j]
								* Math.Cos((Math.PI * (2.0 * k + 1) * i) / (2.0 * n))
								* Math.Cos((Math.PI * (2.0 * l + 1) * j) / (2.0 * m));
						}
					}
					B[k][l] += offset;
					//Console.WriteLine(k + " " + l + ": " + c[k][l]);
				}
			}

			return B;
		}
		
		/// <summary>
		/// Compress the matrix with the threshold percentage
		/// </summary>
		/// <param name="A">signal</param>
		/// <param name="threshold">threshold</param>
		/// <returns>the signal where some values has been zeroed out</returns>
		public static double[][] Filter(double[][] A, double threshold)
		{
			if (A.Length == 0)
				throw new Exception("A empty");

			if (A[0].Length == 0)
				throw new Exception("A row empty");

			int n = A.Length;
			int m = A[0].Length;

			int i =((int) Math.Round(n*threshold, MidpointRounding.AwayFromZero));
			int j =((int) Math.Round(m*threshold, MidpointRounding.AwayFromZero));
			
			Console.WriteLine("Zeroing out from {0}:{1} to {2}:{3}", i+1, n, j+1, m);

			for(int x = i; x < n; x++)
			{
				for(int y = j; y < m; y++)
				{
					A[x][y] = 0.0;
				}
			}

			return A;
		}

		/// <summary>
		/// Cut Least Significant Coefficients
		/// </summary>
		/// <param name="A">signal</param>
		/// <returns>the signal where some values has been zeroed out</returns>
		public static double[][] CutLeastSignificantCoefficients(double[][] A) {
			
			if (A.Length == 0)
				throw new Exception("A empty");

			if (A[0].Length == 0)
				throw new Exception("A row empty");

			int n = A.Length;
			int m = A[0].Length;

			// remove least significant components (last half diagonally)
			for (int i = n - 1; i >= 0; i--) {
				for (int j = m - 1; j > (n-i-2); j--) {
					A[i][j] = 0.0;
				}
			}
			
			return A;
		}
		
		/// <summary>
		/// Method that calculate the Discrete Cosine Transform2 in two dimensions, first
		/// calculate the Discrete Cosine Transform in row and after calculate the
		/// Discrete Cosine Transform in column
		/// </summary>
		/// <param name="A">signal array</param>
		/// <returns>returns the two-dimensional discrete cosine transform of A.
		/// The resulting array B is the same size as A and
		/// contains the discrete cosine transform coefficients B(k1,k2).</returns>
		public static double[,] dct2(double[,] A) {
			
			int rows = A.GetLength(0);
			int columns = A.GetLength(1);

			var fArray = new double[rows][];
			for (int i = 0; i < rows; i++) {
				fArray[i] = new double[columns];
				for (int j = 0; j < columns; j++) {
					fArray[i][j] = A[i, j];
				}
			}
			
			var dctArray = dct2(fArray);
			
			var B = new double[rows, columns];
			for (int i = 0; i < rows; i++) {
				for (int j = 0; j < columns; j++) {
					B[i,j] = dctArray[i][j];
				}
			}

			return B;
		}
		
		/// <summary>
		/// Method that calculate idct2 in two dimensions, first
		/// calculate the Inverse Discrete Cosine Transformin row and after calculate the
		/// Inverse Discrete Cosine Transformin column
		/// </summary>
		/// <param name="A">signal array</param>
		/// <returns>returns the two-dimensional inverse discrete cosine transform (DCT) of A.
		/// The resulting array B is the same size as A and
		/// contains the inverse discrete cosine transform coefficients B(k1,k2).</returns>
		public static double[,] idct2(double[,] A) {
			
			int rows = A.GetLength(0);
			int columns = A.GetLength(1);

			var FArray = new double[rows][];
			for (int i = 0; i < rows; i++) {
				FArray[i] = new double[columns];
				for (int j = 0; j < columns; j++) {
					FArray[i][j] = A[i, j];
				}
			}
			
			var dctArray = idct2(FArray);
			
			var B = new double[rows, columns];
			for (int i = 0; i < rows; i++) {
				for (int j = 0; j < columns; j++) {
					B[i,j] = dctArray[i][j];
				}
			}
			
			return B;
		}

		/// <summary>
		/// Method for printing a Matrix to Console.Out for debug purpose
		/// </summary>
		/// <param name="z"></param>
		public static void PrintMatrix(double[][] z)
		{
			int n = z.Length;
			int m = z[0].Length;

			NumberFormatInfo format = new CultureInfo("en-US", false).NumberFormat;
			format.NumberDecimalDigits = 4;
			int width = 9;

			Console.Write("[");
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < m; j++)
				{
					// round to better printable precision
					decimal d = (decimal) z[i][j];
					decimal rounded = Math.Round(d, ((NumberFormatInfo)format).NumberDecimalDigits);
					string s = rounded.ToString("G29", format);
					Console.Write(s.PadRight(width));
				}
				Console.WriteLine();
			}
			Console.WriteLine("]");
		}
	}
}