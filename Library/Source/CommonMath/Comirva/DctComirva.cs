using System;

namespace CommonUtils.CommonMath.Comirva
{
	/// <summary>
	/// Discrete Cosine Transform using Comirva.
	/// </summary>
	public class DctComirva
	{
		private Matrix dctMatrix;
		private int rows;
		private int columns;
		
		public Matrix DCTMatrix {
			get { return dctMatrix; }
		}
		
		public DctComirva(int rows, int columns)
		{
			this.rows = rows;
			this.columns = columns;
			
			// Compute the DCT
			// This whole section is copied from GetDCTMatrix() from CoMirva package
			dctMatrix = new Matrix(rows, columns);
			
			// Compute constants for DCT
			// http://unix4lyfe.org/dct/
			double k1 = Math.PI/columns;
			double w1 = 1.0/(Math.Sqrt(columns));
			double w2 = Math.Sqrt(2.0/columns);

			// Generate 1D DCT-II matrix
			for(int i = 0; i < rows; i++) {
				for(int j = 0; j < columns; j++) {
					if (i == 0) {
						dctMatrix.Set(i, j, w1 * Math.Cos(k1 * i * (j + 0.5d)));
					} else {
						dctMatrix.Set(i, j, w2 * Math.Cos(k1 * i * (j + 0.5d)));
					}
				}
			}
		}
		
		public double[][] Dct(double[][] f)
		{
			// convert two dimensional data to a Comirva Matrix
			var mat = new Matrix(f, rows, columns);
			#if DEBUG
			mat.WriteText("dct-before.txt");
			#endif
			
			// Perform the DCT (Discrete Cosine Transform)
			Matrix dctResult = dctMatrix * mat;
			
			#if DEBUG
			dctResult.WriteText("dct-after.txt");
			#endif

			// return as two dimensional array
			return dctResult.MatrixData;
		}
		
		public double[][] InverseDct(double[][] F)
		{
			// convert two dimensional data to a Comirva Matrix
			var mat = new Matrix(F, rows, columns);
			#if DEBUG
			mat.WriteText("idct-before.txt");
			#endif

			// Perform the IDCT (Inverse Discrete Cosine Transform)
			Matrix iDctResult = dctMatrix.Transpose() * mat;
			
			#if DEBUG
			iDctResult.WriteText("idct-after.txt");
			#endif

			// return as two dimensional array
			return iDctResult.MatrixData;
		}
	}
}
