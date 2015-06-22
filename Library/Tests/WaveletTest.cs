using System;
using NUnit.Framework;

using System.Drawing;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using CommonUtils.CommonMath.Comirva;
using CommonUtils.CommonMath.Wavelets.HaarCSharp;
using CommonUtils.CommonMath.Wavelets.Compress;
using CommonUtils.CommonMath.Wavelets;

namespace CommonUtils.Tests
{
	public enum WaveletMethod : int {
		Haar = 2,
		HaarTransformTensor = 3,
		HaarWaveletDecompositionTensor = 4,
		HaarCSharp = 5,
		NonStandardHaarWaveletDecomposition = 6,
		HaarWaveletCompress = 7
	}
	
	[TestFixture]
	public class WaveletTest
	{
		[Test]
		public void TestMethod()
		{
			TestHaar1D();
			TestHaarWaveletTransform1D();
			TestHaarCSharp1D();
			TestHaar2D();
			TestHaarWaveletTransform2D();
			TestHaarCSharp2D();
			TestHaarWaveletDecomposition();
			TestHaarTransform();
		}
		
		#region Simple Wavelet Test Methods
		
		/// <summary>
		/// Return 1D test data vector
		/// </summary>
		/// <returns>test vector</returns>
		private static double[] Get1DTestData() {
			// https://unix4lyfe.org/haar/
			
			var input = new double[8] {
				1.000, 2.000, 3.000, 1.000, 2.000, 3.000, 4.000, 0.000
			};
			return input;
		}
		
		/// <summary>
		/// Return 1D test data result vector
		/// </summary>
		/// <returns>test result vector</returns>
		private static double[] Get1DResultData() {
			// https://unix4lyfe.org/haar/
			
			var input = new double[8] {
				5.657, -0.707, -0.500, 0.500, -0.707, 1.414, -0.707, 2.828
			};
			return input;
		}

		/// <summary>
		/// Return 1D test data result vector
		/// </summary>
		/// <returns>test result vector</returns>
		private static double[] Get1DResultDataFirstIteration() {
			// https://unix4lyfe.org/haar/
			
			var input = new double[8] {
				2.121, 2.828, 3.536, 2.828, -0.707, 1.414, -0.707, 2.828
			};
			return input;
		}
		
		/// <summary>
		/// Return 2D test data matrix
		/// mat = [5, 6, 1, 2; 4, 2, 5, 5; 3, 1, 7, 1; 6, 3, 5, 1]
		/// </summary>
		/// <returns>test matrix</returns>
		private static double[][] Get2DTestData() {
			
			var mat = new double[4][];
			for(int m = 0; m < 4; m++) {
				mat[m] = new double[4];
			}
			mat[0][0] = 5;
			mat[0][1] = 6;
			mat[0][2] = 1;
			mat[0][3] = 2;

			mat[1][0] = 4;
			mat[1][1] = 2;
			mat[1][2] = 5;
			mat[1][3] = 5;

			mat[2][0] = 3;
			mat[2][1] = 1;
			mat[2][2] = 7;
			mat[2][3] = 1;

			mat[3][0] = 6;
			mat[3][1] = 3;
			mat[3][2] = 5;
			mat[3][3] = 1;
			
			return mat;
		}

		public static void TestHaar1D() {
			
			Console.WriteLine();
			Console.WriteLine("The 1D Haar Transform as tensor (I.e. all iterations)");
			
			double[] data = Get1DTestData();
			Haar.Haar1D(data, 8);

			IOUtils.Print(Console.Out, data);
			
			// check if it's the same as
			double[] result = Get1DResultData();
			Assert.That(data, Is.EqualTo(result).AsCollection.Within(0.001), "fail at [0]");
		}
		
		public static void TestHaar2D() {
			
			Console.WriteLine();
			Console.WriteLine("The 2D Haar Transform as tensor (I.e. all iterations)");
			
			double[][] mat = Get2DTestData();
			Haar.Haar2D(mat, 4, 4);

			var result = new Matrix(mat);
			result.PrintPretty();
		}
		
		public static void TestHaarWaveletTransform1D() {
			
			Console.WriteLine();
			Console.WriteLine("The Haar Wavelet Transform 1D with only one iteration)");
			
			double[] data = Get1DTestData();
			
			HaarWaveletTransform.HaarTransform1D(data, data.Length);

			IOUtils.Print(Console.Out, data);
			
			// check if it's the same as
			double[] result = Get1DResultDataFirstIteration();
			Assert.That(data, Is.EqualTo(result).AsCollection.Within(0.001), "fail at [0]");
			
			HaarWaveletTransform.InverseHaarTransform1D(data, data.Length);
			
			IOUtils.Print(Console.Out, data);
			
			Assert.That(data, Is.EqualTo(Get1DTestData()).AsCollection.Within(0.001), "fail at [0]");
		}
		
		public static void TestHaarWaveletTransform2D() {
			
			Console.WriteLine();
			Console.WriteLine("The Haar Wavelet Transform 2D with only one iteration)");
			
			double[][] mat = Get2DTestData();
			HaarWaveletTransform.HaarTransform2D(mat, 4, 4);

			var result = new Matrix(mat);
			result.PrintPretty();
			
			HaarWaveletTransform.InverseHaarTransform2D(mat, 4, 4);
			result.PrintPretty();
		}
		
		public static void TestHaarCSharp1D() {
			Console.WriteLine();
			Console.WriteLine("The HaarCSharp1D with only one iteration)");
			
			double[] data = Get1DTestData();
			
			ForwardWaveletTransform.Transform1D(data);

			IOUtils.Print(Console.Out, data);
			
			// check if it's the same as
			double[] result = Get1DResultDataFirstIteration();
			//Assert.That(data, Is.EqualTo(result).AsCollection.Within(0.001), "fail at [0]");
			
			InverseWaveletTransform.Transform1D(data);
			
			IOUtils.Print(Console.Out, data);
			
			//Assert.That(data, Is.EqualTo(Get1DTestData()).AsCollection.Within(0.001), "fail at [0]");
		}
		
		public static void TestHaarCSharp2D() {
			
			Console.WriteLine();
			Console.WriteLine("The TestHaarCSharp2D");
			
			double[][] mat = Get2DTestData();
			ForwardWaveletTransform.Transform2D(mat, 2);

			var result = new Matrix(mat);
			result.PrintPretty();
			
			InverseWaveletTransform.Transform2D(mat, 2);
			result.PrintPretty();
		}
		
		public static void TestHaarWaveletDecomposition() {
			
			Console.WriteLine();
			Console.WriteLine("The Standard 2D HaarWaveletDecomposition method (Tensor)");
			
			var haar = new StandardHaarWaveletDecomposition();
			
			double[][] mat = Get2DTestData();
			
			haar.DecomposeImageInPlace(mat);

			var result = new Matrix(mat);
			result.PrintPretty();

			Console.WriteLine();
			Console.WriteLine("The Non Standard 2D HaarWaveletDecomposition method (JPEG 2000)");
			
			var haarNonStandard = new NonStandardHaarWaveletDecomposition();
			
			mat = Get2DTestData();
			haarNonStandard.DecomposeImageInPlace(mat);

			var resultNonStandard = new Matrix(mat);
			resultNonStandard.PrintPretty();
		}
				
		public static void TestHaarTransform() {
			
			double[][] mat = Get2DTestData();
			var matrix = new Matrix(mat);
			
			double[] packed = matrix.GetColumnPackedCopy();
			HaarTransform.R8MatPrint(matrix.Rows, matrix.Columns, packed, "Input array packed:");

			HaarTransform.Haar2D(matrix.Rows, matrix.Columns, packed);
			HaarTransform.R8MatPrint(matrix.Rows, matrix.Columns, packed, "Transformed array packed:");
			
			double[] w = HaarTransform.R8MatCopyNew(matrix.Rows, matrix.Columns, packed);

			HaarTransform.Haar2DInverse(matrix.Rows, matrix.Columns, w);
			HaarTransform.R8MatPrint(matrix.Rows, matrix.Columns, w, "Recovered array W:");
			
			var m = new Matrix(w, matrix.Rows);
		}
		#endregion
		
		#region More Advanced Wavelet Test Methods
		public static void TestHaarInputOutput(string imageInPath) {

			// read image
			Image img = Image.FromFile(imageInPath);
			
			// make sure it's square and power of two
			//int size = (img.Height > img.Width ? img.Height : img.Width);
			//int sizePow2 = MathUtils.NextPowerOfTwo(size);
			//img = ImageUtils.Resize(img, sizePow2, sizePow2, false);
			
			var bmp = new Bitmap(img);
			var image = new double[bmp.Height][];
			for (int i = 0; i < bmp.Height; i++)
			{
				image[i] = new double[bmp.Width];
				for (int j = 0; j < bmp.Width; j++) {
					Color C = bmp.GetPixel(j, i);
					image[i][j] = (C.R + C.G + C.B ) / 3;
				}
			}
			
			var inputMatrix = new Matrix(image);
			//inputMatrix.WriteCSV("haar-before.csv", ";");

			//CommonUtils.CommonMath.Wavelets.Compress.WaveletComDec.CompressDecompress2D(image, 3, 0);
			//inputMatrix.DrawMatrixImage("haar-transform-back.png", -1, -1, false);
			//return;
			
			// Haar Wavelet Transform
			//Matrix haarMatrix = HaarWaveletTransform(inputMatrix.MatrixData);
			
			//CommonUtils.CommonMath.Wavelets.Compress.HaarWaveletTransform.HaarTransform2D(image, inputMatrix.Rows, inputMatrix.Columns);
			int lastHeight = 0;
			int lastWidth = 0;
			int levels = 3;
			//CommonUtils.CommonMath.Wavelets.Compress.WaveletCompress.Compress2D(image, levels, 500, out lastHeight, out lastWidth);
			WaveletCompress.HaarTransform2D(image, levels, out lastHeight, out lastWidth);
			Matrix haarMatrix = inputMatrix.Copy();
			
			//Wavelets.Dwt dwt = new Wavelets.Dwt(2);
			//Matrix haarMatrix = dwt.Transform(normalizedMatrix);
			
			/*
			WaveletInterface wavelet = new Haar02();
			TransformInterface bWave = new FastWaveletTransform(wavelet);
			Transform t = new Transform(bWave); // perform all steps
			double[][] dwtArray = t.forward(normalizedMatrix.MatrixData);
			Matrix haarMatrix = new Matrix(dwtArray);
			 */
			//int oldRows = haarMatrix.Rows;
			//int oldColumns = haarMatrix.Columns;
			//haarMatrix = haarMatrix.Resize(20, oldColumns);
			haarMatrix.WriteCSV("haar.csv", ";");
			
			// Inverse 2D Haar Wavelet Transform
			//Matrix haarMatrixInverse = InverseHaarWaveletTransform(haarMatrix.MatrixData);
			
			Matrix haarMatrixInverse = haarMatrix.Copy();
			//haarMatrixInverse = haarMatrixInverse.Resize(oldRows, oldColumns);
			//CommonUtils.CommonMath.Wavelets.Compress.HaarWaveletTransform.InverseHaarTransform2D(haarMatrixInverse.MatrixData, haarMatrixInverse.Rows, haarMatrixInverse.Columns);
			WaveletDecompress.Decompress2D(haarMatrixInverse.MatrixData, levels, lastHeight, lastWidth);
			
			//Matrix haarMatrixInverse = dwt.TransformBack(haarMatrix);
			
			//double[][] dwtArrayInverse = t.reverse(haarMatrix.MatrixData);
			//Matrix haarMatrixInverse = new Matrix(dwtArrayInverse);
			
			//haarMatrixInverse.WriteCSV("haar-inverse.csv", ";");
			
			// Output the image
			//haarMatrix.DrawMatrixImageLogValues("haar-transform.png");
			haarMatrixInverse.DrawMatrixImage("haar-transform-back.png", -1, -1, false);
		}
		
		public static void TestDenoise(string imageInPath) {
			
			// Read Image
			Image img = Image.FromFile(imageInPath);
			var bmp = new Bitmap(img);
			var image = new double[bmp.Height][];
			for (int i = 0; i < bmp.Height; i++)
			{
				image[i] = new double[bmp.Width];
				for (int j = 0; j < bmp.Width; j++) {
					//image[i][j] = bmp.GetPixel(j, i).ToArgb();
					image[i][j] = bmp.GetPixel(j, i).B; // use only blue channel
				}
			}

			//Matrix imageMatrix = new Matrix(image);
			//imageMatrix.WriteCSV("lena-blue.csv", ";");

			// Normalize the pixel values to the range 0..1.0. It does this by dividing all pixel values by the max value.
			double max = image.Max((b) => b.Max((v) => Math.Abs(v)));
			double[][] imageNormalized = image.Select(i => i.Select(j => j/max).ToArray()).ToArray();
			
			var normalizedMatrix = new Matrix(imageNormalized);
			//normalizedMatrix.WriteCSV("lena-normalized.csv", ";");
			normalizedMatrix.DrawMatrixImage("lena-original.png", -1, -1, false);

			// Add Noise using normally distributed pseudorandom numbers
			// image_noisy = image_normalized + 0.1 * randn(size(image_normalized));
			RandomUtils.Seed(Guid.NewGuid().GetHashCode());
			double[][] imageNoisy = imageNormalized.Select(i => i.Select(j => j + (0.1 * RandomUtils.NextDouble())).ToArray()).ToArray();
			var matrixNoisy = new Matrix(imageNoisy);
			matrixNoisy.DrawMatrixImage("lena-noisy.png", -1, -1, false);

			// Haar Wavelet Transform
			Matrix haarMatrix = WaveletUtils.HaarWaveletTransform2D(imageNoisy);

			// Thresholding
			const double threshold = 0.15; // 0.15 seems to work well with the noise added above, 0.1
			var yHard = Thresholding.PerformHardThresholding(haarMatrix.MatrixData, threshold);
			var ySoft = Thresholding.PerformSoftThresholding(haarMatrix.MatrixData, threshold);
			var ySemisoft = Thresholding.PerformSemisoftThresholding(haarMatrix.MatrixData, threshold, threshold*2);
			var ySemisoft2 = Thresholding.PerformSemisoftThresholding(haarMatrix.MatrixData, threshold, threshold*4);
			var yStrict = Thresholding.PerformStrictThresholding(haarMatrix.MatrixData, 10);
			
			// Inverse 2D Haar Wavelet Transform
			Matrix zHard = WaveletUtils.InverseHaarWaveletTransform2D(yHard);
			Matrix zSoft = WaveletUtils.InverseHaarWaveletTransform2D(ySoft);
			Matrix zSemisoft = WaveletUtils.InverseHaarWaveletTransform2D(ySemisoft);
			Matrix zSemisoft2 = WaveletUtils.InverseHaarWaveletTransform2D(ySemisoft2);
			Matrix zStrict = WaveletUtils.InverseHaarWaveletTransform2D(yStrict);
			
			//zHard.WriteCSV("lena-thresholding-hard.csv", ";");

			// Output the images
			zHard.DrawMatrixImage("lena-thresholding-hard.png", -1, -1, false);
			zSoft.DrawMatrixImage("lena-thresholding-soft.png", -1, -1, false);
			zSemisoft.DrawMatrixImage("lena-thresholding-semisoft.png", -1, -1, false);
			zSemisoft2.DrawMatrixImage("lena-thresholding-semisoft2.png", -1, -1, false);
			zStrict.DrawMatrixImage("lena-thresholding-strict.png", -1, -1, false);
		}

		public static void SaveWaveletImage(string imageInPath, string imageOutPath, WaveletMethod waveletMethod) {

			// Read Image
			Image img = Image.FromFile(imageInPath);
			var bmp = new Bitmap(img);
			var image = new double[bmp.Height][];
			for (int i = 0; i < bmp.Height; i++)
			{
				image[i] = new double[bmp.Width];
				for (int j = 0; j < bmp.Width; j++) {
					//image[i][j] = bmp.GetPixel(j, i).ToArgb();
					image[i][j] = bmp.GetPixel(j, i).B; // use only blue channel
				}
			}

			// Normalize the pixel values to the range 0..1.0. It does this by dividing all pixel values by the max value.
			double max = image.Max((b) => b.Max((v) => (v)));
			double[][] imageNormalized = image.Select(i => i.Select(j => j/max).ToArray()).ToArray();
			//Matrix normalizedMatrix = new Matrix(imageNormalized);
			//normalizedMatrix.WriteCSV("ImageNormalized.csv", ";");
			
			Matrix bitmap = GetWaveletTransformedMatrix(imageNormalized, waveletMethod);
			bitmap.DrawMatrixImage(imageOutPath, -1, -1, false);

			img.Dispose();
			bmp.Dispose();
			bitmap = null;
		}
		
		public static Matrix GetWaveletTransformedMatrix(double[][] image, WaveletMethod waveletMethod)
		{
			int width = image[0].Length;
			int height = image.Length;

			Matrix dwtMatrix = null;

			Stopwatch stopWatch = Stopwatch.StartNew();
			long startS = stopWatch.ElapsedTicks;

			switch(waveletMethod) {
				case WaveletMethod.Haar:
					Haar.Haar2D(image, height, width);
					dwtMatrix = new Matrix(image);
					break;
				case WaveletMethod.HaarTransformTensor: // This is using the tensor product layout
					dwtMatrix = WaveletUtils.HaarWaveletTransform2D(image);
					break;
				case WaveletMethod.HaarWaveletDecompositionTensor: // This is using the tensor product layout
					var haar = new StandardHaarWaveletDecomposition();
					haar.DecomposeImageInPlace(image);
					dwtMatrix = new Matrix(image);
					break;
				case WaveletMethod.NonStandardHaarWaveletDecomposition: // JPEG 2000
					var haarNonStandard = new NonStandardHaarWaveletDecomposition();
					haarNonStandard.DecomposeImageInPlace(image);
					dwtMatrix = new Matrix(image);
					break;
				case WaveletMethod.HaarWaveletCompress:
					int lastHeight = 0;
					int lastWidth = 0;
					WaveletCompress.HaarTransform2D(image, 10000, out lastHeight, out lastWidth);
					dwtMatrix = new Matrix(image);
					break;
				default:
					break;
			}

			long endS = stopWatch.ElapsedTicks;
			Console.WriteLine("WaveletMethod: {0} Time in ticks: {1}", Enum.GetName(typeof(WaveletMethod), waveletMethod), (endS - startS));

			//dwtMatrix.WriteCSV("HaarImageNormalized.csv", ";");
			
			// increase all values
			double[][] haarImageNormalized5k = dwtMatrix.MatrixData.Select(i => i.Select(j => j*5000).ToArray()).ToArray();
			//Matrix haarImageNormalized5kMatrix = new Matrix(haarImageNormalized5k);
			//haarImageNormalized5kMatrix.WriteCSV("HaarImageNormalized5k.csv", ";");
			
			// convert to byte values (0 - 255)
			// duplicate the octave/ matlab method uint8
			var uint8 = new double[haarImageNormalized5k.Length][];
			for (int i = 0; i < haarImageNormalized5k.Length; i++) {
				uint8[i] = new double[haarImageNormalized5k.Length];
				for (int j = 0; j < haarImageNormalized5k[i].Length; j++) {
					double v = haarImageNormalized5k[i][j];
					if (v > 255) {
						uint8[i][j] = 255;
					} else if (v < 0) {
						uint8[i][j] = 0;
					} else {
						uint8[i][j] = (byte) haarImageNormalized5k[i][j];
					}
				}
			}
			
			var uint8Matrix = new Matrix(uint8);
			//uint8Matrix.WriteCSV("Uint8HaarImageNormalized5k.csv", ";");
			return uint8Matrix;
		}
		#endregion
		
	}
}
