using System;
using System.Linq;

using CommonUtils.MathLib.MatrixLib;

namespace CommonUtils.MathLib.Wavelets
{	
	/// <summary>
	/// Description of WaveletUtils.
	/// </summary>
	public static class WaveletUtils
	{
		/// <summary>
		/// Haar Transform of a 2D image to a Matrix.
		/// This is using the tensor product layout.
		/// Performance is also quite fast.
		/// Note that the input array must be a square matrix of dimension 2n x 2n where n is an integer
		/// </summary>
		/// <param name="image">2D array</param>
		/// <param name="disableMatrixDimensionCheck">True if matrix dimension check should be turned off</param>
		/// <returns>Matrix with haar transform</returns>
		public static Matrix HaarWaveletTransform2D(double[][] image, bool disableMatrixDimensionCheck=false) {
			var imageMatrix = new Matrix(image);
			
			// Check that the input matrix is a square matrix of dimension 2n x 2n (where n is an integer)
			if (!disableMatrixDimensionCheck && !imageMatrix.IsSymmetric() && !MathUtils.IsPowerOfTwo(image.Length)) {
				throw new Exception("Input matrix is not symmetric or has dimensions that are a power of two!");
			}
			
			double[] imagePacked = imageMatrix.GetColumnPackedCopy();
			HaarTransform.Haar2D(imageMatrix.Rows, imageMatrix.Columns, imagePacked);
			var haarMatrix = new Matrix(imagePacked, imageMatrix.Rows);
			return haarMatrix;
		}

		/// <summary>
		/// Inverse Haar Transform of a 2D image to a Matrix.
		/// This is using the tensor product layout.
		/// Performance is also quite fast.
		/// Note that the input array must be a square matrix of dimension 2n x 2n where n is an integer
		/// </summary>
		/// <param name="image">2D array</param>
		/// <param name="disableMatrixDimensionCheck">True if matrix dimension check should be turned off</param>
		/// <returns>Matrix with inverse haar transform</returns>
		public static Matrix InverseHaarWaveletTransform2D(double[][] image, bool disableMatrixDimensionCheck=false) {
			var imageMatrix = new Matrix(image);

			// Check that the input matrix is a square matrix of dimension 2n x 2n (where n is an integer)
			if (!disableMatrixDimensionCheck && !imageMatrix.IsSymmetric() && !MathUtils.IsPowerOfTwo(image.Length)) {
				throw new Exception("Input matrix is not symmetric or has dimensions that are a power of two!");
			}

			double[] imagePacked = imageMatrix.GetColumnPackedCopy();
			HaarTransform.Haar2DInverse(imageMatrix.Rows, imageMatrix.Columns, imagePacked);
			var inverseHaarMatrix = new Matrix(imagePacked, imageMatrix.Rows);
			return inverseHaarMatrix;
		}
	}
}
