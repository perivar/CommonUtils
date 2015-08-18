using System;

namespace CommonUtils.MathLib.Wavelets
{
	/// <summary>
	/// Standard Haar wavelet decomposition algorithm.
	/// According to Fast Multi-Resolution Image Query paper, Haar wavelet decomposition with standard basis function works better in image querying
	/// </summary>
	/// <remarks>
	/// Implemented according to the algorithm found here http://grail.cs.washington.edu/projects/wavelets/article/wavelet1.pdf
	/// </remarks>
	public class StandardHaarWaveletDecomposition : HaarWaveletDecomposition
	{
		public StandardHaarWaveletDecomposition()  {
		}
		
		#region IWaveletDecomposition Members

		/// <summary>
		/// Apply Haar Wavelet decomposition on the image
		/// </summary>
		/// <param name="matrix">Image to be decomposed</param>
		public override void DecomposeImageInPlace(double[][] matrix)
		{
			DecomposeImageTensor(matrix);
		}

		#endregion

		private void Decomposition(double[] array)
		{
			int h = array.Length;

			// Changed by Per Ivar 20130703: Don't do the square root, because that's not standard
			/*
			for (int i = 0; i < h; i++)
			{
				array[i] /= (double)Math.Sqrt(h);
			}
			 */

			while (h > 1)
			{
				DecompositionStep(array, h);
				h /= 2;
			}
		}

		/// <summary>
		/// The standard 2-dimensional Haar wavelet decomposition involves one-dimensional decomposition of each row
		/// followed by a one-dimensional decomposition of each column of the result.
		/// Tensor corresponds to doing a FWT along each dimension of the matrix in contrast to
		/// the behaviour of the JPEG 2000 standard.
		/// </summary>
		/// <remarks>Copied from the Soundfingerprinting project
		/// Copyright © Soundfingerprinting, 2010-2011
		/// ciumac.sergiu@gmail.com
		/// </remarks>
		/// <param name="matrix">Image to be decomposed</param>
		private void DecomposeImageTensor(double[][] matrix)
		{
			int rows = matrix.Length; /*128*/
			int cols = matrix[0].Length; /*32*/

			// The order of decomposition is reversed because the matrix is 128x32 but we consider it reversed 32x128
			for (int col = 0; col < cols /*32*/; col++)
			{
				var column = new double[rows]; /*Length of each column is equal to number of rows*/
				for (int row = 0; row < rows; row++)
				{
					column[row] = matrix[row][col]; /*Copying Column vector*/
				}

				Decomposition(column); /*Decomposition of each row*/
				for (int row = 0; row < rows; row++)
				{
					matrix[row][col] = column[row];
				}
			}

			for (int row = 0; row < rows /*128*/; row++)
			{
				Decomposition(matrix[row]); /*Decomposition of each row*/
			}
		}
	}
}