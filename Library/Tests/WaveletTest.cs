using System;
using NUnit.Framework;

using System.Drawing;
using System.Linq;
using System.Diagnostics;

using CommonUtils.MathLib.MatrixLib;
using CommonUtils.MathLib.Wavelets.HaarCSharp;
using CommonUtils.MathLib.Wavelets.Compress;
using CommonUtils.MathLib.Wavelets;

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
		public void TestWaveletTransforms()
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
		
		[Test]
		public void TestWaveletImageProcessing() {
			string inputPath = @"Tests\lena.png";
			TestHaarInputOutput(inputPath);
			TestDenoise(inputPath);
		}
		
		[Test]
		public void TestWaveletForwardTransformImage() {
			
			string inputPath = @"Tests\lena.png";
			var image = ReadImageGrayscale(inputPath);
			var orig = new Matrix(image);
			
			Matrix m1 = GetWaveletTransformedMatrix(orig.GetArrayCopy(), WaveletMethod.Haar);
			m1.DrawMatrixImage("haar-transform-1.png", -1, -1, false);
			
			Matrix m2 = GetWaveletTransformedMatrix(orig.GetArrayCopy(), WaveletMethod.HaarCSharp);
			m2.DrawMatrixImage("haar-transform-2.png", -1, -1, false);

			Matrix m3 = GetWaveletTransformedMatrix(orig.GetArrayCopy(), WaveletMethod.HaarTransformTensor);
			m3.DrawMatrixImage("haar-transform-3.png", -1, -1, false);

			Matrix m4 = GetWaveletTransformedMatrix(orig.GetArrayCopy(), WaveletMethod.HaarWaveletCompress);
			m4.DrawMatrixImage("haar-transform-4.png", -1, -1, false);

			Matrix m5 = GetWaveletTransformedMatrix(orig.GetArrayCopy(), WaveletMethod.HaarWaveletDecompositionTensor);
			m5.DrawMatrixImage("haar-transform-5.png", -1, -1, false);

			Matrix m6 = GetWaveletTransformedMatrix(orig.GetArrayCopy(), WaveletMethod.NonStandardHaarWaveletDecomposition);
			m6.DrawMatrixImage("haar-transform-6.png", -1, -1, false);
		}
		
		[Test]
		public void TestThresholding() {
			// Run the following matlab test:
			// T = 1; % threshold value
			// v = linspace(-5,5,1024);
			// clf;
			// hold('on');
			// plot(v, perform_thresholding(v,T,'hard'), 'b--');
			// plot(v, perform_thresholding(v,T,'soft'), 'r--');
			// plot(v, perform_thresholding(v,[T 2*T],'semisoft'), 'g');
			// plot(v, perform_thresholding(v,[T 4*T],'semisoft'), 'g:');
			// plot(v, perform_thresholding(v',400,'strict'), 'r:');
			// legend('hard', 'soft', 'semisoft, \mu=2', 'semisoft, \mu=4', 'strict, 400');
			// hold('off');
			
			const double start = -5;
			const double end = 5;
			const double totalCount = 1024;
			var v = MathUtils.Linspace(start, end, totalCount);
			
			// perform thresholding and plot
			const int T = 1;
			
			double[][] hard = Thresholding.PerformHardThresholding(v, T);
			var mHard = new Matrix(hard);
			mHard.DrawMatrixGraph("thresholding-hard.png", false);

			double[][] soft = Thresholding.PerformSoftThresholding(v, T);
			var mSoft = new Matrix(soft);
			mSoft.DrawMatrixGraph("thresholding-soft.png", false);

			double[][] semisoft1 = Thresholding.PerformSemisoftThresholding(v, T, 2*T);
			var mSemiSoft1 = new Matrix(semisoft1);
			mSemiSoft1.DrawMatrixGraph("thresholding-semisoft1.png", false);

			double[][] semisoft2 = Thresholding.PerformSemisoftThresholding(v, T, 4*T);
			var mSemiSoft2 = new Matrix(semisoft2);
			mSemiSoft2.DrawMatrixGraph("thresholding-semisoft2.png", false);
			
			double[][] strict = Thresholding.PerformStrictThresholding(v, 400);
			var mStrict = new Matrix(strict);
			mStrict.DrawMatrixGraph("thresholding-strict.png", false);
		}
		
		#region Test Wavelet Transforms using data arrays (no images)
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
		
		private static double[][] Get2DResultData() {
			
			var mat = new double[4][];
			for(int m = 0; m < 4; m++) {
				mat[m] = new double[4];
			}
			mat[0][0] = 14.250000;
			mat[0][1] = 0.750000;
			mat[0][2] = 2.121320;
			mat[0][3] = 3.181981;

			mat[1][0] = 0.750000;
			mat[1][1] = 1.250000;
			mat[1][2] = -1.414214;
			mat[1][3] = -3.889087;

			mat[2][0] = -0.707107;
			mat[2][1] = 4.242641;
			mat[2][2] = -1.500000;
			mat[2][3] = -0.500000;

			mat[3][0] = -1.060660;
			mat[3][1] = -2.474874;
			mat[3][2] = -0.500000;
			mat[3][3] = 1.000000;
			
			return mat;
		}

		private static double[][] Get2DResultDataFirstIteration() {
			
			var mat = new double[4][];
			for(int m = 0; m < 4; m++) {
				mat[m] = new double[4];
			}
			
			mat[0][0] =  8.500000;
			mat[0][1] =  6.500000;
			mat[0][2] =  0.500000;
			mat[0][3] = -0.500000;

			mat[1][0] =  6.500000;
			mat[1][1] =  7.000000;
			mat[1][2] =  2.500000;
			mat[1][3] =  5.000000;

			mat[2][0] =  2.500000;
			mat[2][1] = -3.500000;
			mat[2][2] = -1.500000;
			mat[2][3] = -0.500000;

			mat[3][0] = -2.500000;
			mat[3][1] =  1.000000;
			mat[3][2] = -0.500000;
			mat[3][3] =  1.000000;
			
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
			Console.WriteLine("The Haar Wavelet Transform 1D (with only one iteration)");
			
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
			Console.WriteLine("The Haar Wavelet Transform 2D (with only one iteration)");
			
			double[][] mat = Get2DTestData();
			HaarWaveletTransform.HaarTransform2D(mat, 4, 4);

			var result = new Matrix(mat);
			result.PrintPretty();
			Assert.That(mat, Is.EqualTo(Get2DResultDataFirstIteration()).AsCollection.Within(0.001), "fail at [0]");
			
			HaarWaveletTransform.InverseHaarTransform2D(mat, 4, 4);
			result.PrintPretty();
			Assert.That(mat, Is.EqualTo(Get2DTestData()).AsCollection.Within(0.001), "fail at [0]");
		}
		
		public static void TestHaarCSharp1D() {
			Console.WriteLine();
			Console.WriteLine("The HaarCSharp1D");
			
			double[] data = Get1DTestData();
			
			ForwardWaveletTransform.Transform1D(data, true);

			IOUtils.Print(Console.Out, data);
			
			// check if it's the same as
			double[] result = Get1DResultData();
			Assert.That(data, Is.EqualTo(result).AsCollection.Within(0.001), "fail at [0]");
			
			InverseWaveletTransform.Transform1D(data, true);
			
			IOUtils.Print(Console.Out, data);
			Assert.That(data, Is.EqualTo(Get1DTestData()).AsCollection.Within(0.001), "fail at [0]");
		}
		
		public static void TestHaarCSharp2D() {
			
			Console.WriteLine();
			Console.WriteLine("The HaarCSharp2D");
			
			double[][] mat = Get2DTestData();
			ForwardWaveletTransform.Transform2D(mat);

			var result = new Matrix(mat);
			result.PrintPretty();
			Assert.That(mat, Is.EqualTo(Get2DResultData()).AsCollection.Within(0.001), "fail at [0]");
			
			InverseWaveletTransform.Transform2D(mat);
			result.PrintPretty();
			Assert.That(mat, Is.EqualTo(Get2DTestData()).AsCollection.Within(0.001), "fail at [0]");
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
		
		#region Wavelet Test Methods using images
		private static double[][] ReadImageGrayscale(string imageInPath) {
			// read image
			Image img = Image.FromFile(imageInPath);
			
			// read image as grayscale
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
			return image;
		}

		public static void TestHaarInputOutput(string imageInPath) {

			var image = ReadImageGrayscale(imageInPath);
			
			#region Wavelet Compress Methods
			// Test HaarWaveletTransform
			var inputMatrix = new Matrix(image);
			inputMatrix.WriteCSV("haar-before.csv");
			HaarWaveletTransform.HaarTransform2D(image, inputMatrix.Rows, inputMatrix.Columns);
			
			Matrix haarMatrixInverse = inputMatrix.Copy();
			HaarWaveletTransform.InverseHaarTransform2D(haarMatrixInverse.MatrixData, haarMatrixInverse.Rows, haarMatrixInverse.Columns);
			haarMatrixInverse.WriteCSV("haar-after.csv");
			haarMatrixInverse.DrawMatrixImage("haar-transform-forward-and-backward.png", -1, -1, false);

			// Test Wavelet Compress and Decompress in one step
			const int compLevels = 8;
			const int compTreshold = 150;

			Matrix haarMatrixCompDecomp = haarMatrixInverse.Copy();
			WaveletComDec.CompressDecompress2D(haarMatrixCompDecomp.MatrixData, compLevels, compTreshold);
			haarMatrixCompDecomp.DrawMatrixImage("haar-compress-and-decompress-combined.png", -1, -1, false);

			// Test Compress and Decompress in two steps
			int lastHeight = 0;
			int lastWidth = 0;
			Matrix haarMatrixComp = haarMatrixInverse.Copy();
			WaveletCompress.Compress2D(haarMatrixComp.MatrixData, compLevels, compTreshold, out lastHeight, out lastWidth);
			WaveletDecompress.Decompress2D(haarMatrixComp.MatrixData, compLevels, lastHeight, lastWidth);
			haarMatrixComp.DrawMatrixImage("haar-compress-and-decompress.png", -1, -1, false);
			#endregion
			
			#region Test using HaarCSharp
			
			// Test HaarCSharp using iterations
			const int haarCSharpIterations = 3;
			Matrix haarMatrixCSharp = haarMatrixInverse.Copy();
			ForwardWaveletTransform.Transform2D(haarMatrixCSharp.MatrixData, false, haarCSharpIterations);
			haarMatrixCSharp.DrawMatrixImage("haar-forward.png", -1, -1, false);
			InverseWaveletTransform.Transform2D(haarMatrixCSharp.MatrixData, false, haarCSharpIterations);
			haarMatrixCSharp.DrawMatrixImage("haar-inverse.png", -1, -1, false);
			
			// Test HaarCSharp using all levels and only 1 iteration
			Matrix haarMatrixCSharpAll = haarMatrixInverse.Copy();
			ForwardWaveletTransform.Transform2D(haarMatrixCSharpAll.MatrixData, true, 1);
			haarMatrixCSharpAll.DrawMatrixImage("haar-forward-all.png", -1, -1, false);
			InverseWaveletTransform.Transform2D(haarMatrixCSharpAll.MatrixData, true, 1);
			haarMatrixCSharpAll.DrawMatrixImage("haar-inverse-all.png", -1, -1, false);
			#endregion
		}
		
		public static void TestDenoise(string imageInPath) {
			
			var image = ReadImageGrayscale(imageInPath);

			// Normalize the pixel values to the range 0..1.0. It does this by dividing all pixel values by the max value.
			double max = image.Max((b) => b.Max((v) => Math.Abs(v)));
			double[][] imageNormalized = image.Select(i => i.Select(j => j/max).ToArray()).ToArray();
			
			var normalizedMatrix = new Matrix(imageNormalized);
			normalizedMatrix.DrawMatrixImage("lena-original.png", -1, -1, false);

			// Add Noise using normally distributed pseudorandom numbers
			// image_noisy = image_normalized + 0.1 * randn(size(image_normalized));
			RandomUtils.Seed(Guid.NewGuid().GetHashCode());
			double[][] imageNoisy = imageNormalized.Select(i => i.Select(j => j + (0.2 * RandomUtils.NextDouble())).ToArray()).ToArray();
			var matrixNoisy = new Matrix(imageNoisy);
			matrixNoisy.DrawMatrixImage("lena-noisy.png", -1, -1, false);

			// Haar Wavelet Transform
			Matrix haarMatrix = WaveletUtils.HaarWaveletTransform2D(imageNoisy);

			// Thresholding
			const double threshold = 0.10; // 0.15 seems to work well with the noise added above, 0.1
			var yHard = Thresholding.PerformHardThresholding(haarMatrix.MatrixData, threshold);
			var ySoft = Thresholding.PerformSoftThresholding(haarMatrix.MatrixData, threshold);
			var ySemisoft = Thresholding.PerformSemisoftThresholding(haarMatrix.MatrixData, threshold, threshold*2);
			var ySemisoft2 = Thresholding.PerformSemisoftThresholding(haarMatrix.MatrixData, threshold, threshold*4);
			var yStrict = Thresholding.PerformStrictThresholding(haarMatrix.MatrixData, 100);
			
			// Inverse 2D Haar Wavelet Transform
			Matrix zHard = WaveletUtils.InverseHaarWaveletTransform2D(yHard);
			Matrix zSoft = WaveletUtils.InverseHaarWaveletTransform2D(ySoft);
			Matrix zSemisoft = WaveletUtils.InverseHaarWaveletTransform2D(ySemisoft);
			Matrix zSemisoft2 = WaveletUtils.InverseHaarWaveletTransform2D(ySemisoft2);
			Matrix zStrict = WaveletUtils.InverseHaarWaveletTransform2D(yStrict);
			
			// Output the images
			zHard.DrawMatrixImage("lena-thresholding-hard.png", -1, -1, false);
			zSoft.DrawMatrixImage("lena-thresholding-soft.png", -1, -1, false);
			zSemisoft.DrawMatrixImage("lena-thresholding-semisoft.png", -1, -1, false);
			zSemisoft2.DrawMatrixImage("lena-thresholding-semisoft2.png", -1, -1, false);
			zStrict.DrawMatrixImage("lena-thresholding-strict.png", -1, -1, false);
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
				case WaveletMethod.HaarCSharp:
					ForwardWaveletTransform.Transform2D(image, false, 2);
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
			
			// increase all values
			const int mul = 50;
			double[][] haarImageNormalized5k = dwtMatrix.MatrixData.Select(i => i.Select(j => j*mul).ToArray()).ToArray();
			
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
			return uint8Matrix;
		}
		#endregion
	}
}
