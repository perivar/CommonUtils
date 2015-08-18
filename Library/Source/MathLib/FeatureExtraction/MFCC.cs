using System;

using CommonUtils;
using CommonUtils.MathLib.MatrixLib;
using CommonUtils.MathLib.Wavelets.Compress;
using CommonUtils.MathLib.Wavelets;
using CommonUtils.MathLib.Filters;

namespace CommonUtils.MathLib.FeatureExtraction
{
	/// <summary>
	/// Mfcc method copied from the Mirage project:
	/// Mirage - High Performance Music Similarity and Automatic Playlist Generator
	/// http://hop.at/mirage
	///
	/// Copyright (C) 2007 Dominik Schnitzer, dominik@schnitzer.at
	/// Changed and enhanced by Per Ivar Nerseth, perivar@nerseth.com
	///
	/// This program is free software; you can redistribute it and/or
	/// modify it under the terms of the GNU General Public License
	/// as published by the Free Software Foundation; either version 2
	/// of the License, or (at your option) any later version.
	/// </summary>
	public class MFCC
	{
		public Matrix filterWeights;
		public Matrix dct;
		
		private int numberCoefficients; 			// number of MFCC COEFFICIENTS. E.g. 20
		private int[] melScaleFreqsIndex; 			// store the mel scale indexes
		private double[] melScaleTriangleHeights; 	// store the mel filter triangle heights
		private const int numberWaveletTransforms = 2; 	// number of wavelet transform iterations, 3?
		
		/// <summary>
		/// Create a Mfcc object
		/// This method is not optimized in the sense that the Mel Filter Bands
		/// and the DCT is created here (and not read in)
		/// Only support an overlap of half the window size
		/// </summary>
		/// <param name="winsize">window size</param>
		/// <param name="srate">sample rate</param>
		/// <param name="numberFilters">number of filters (MEL COEFFICIENTS). E.g. 36 (SPHINX-III uses 40)</param>
		/// <param name="numberCoefficients">number of MFCC COEFFICIENTS. E.g. 20</param>
		public MFCC(int winsize, int srate, int numberFilters, int numberCoefficients)
		{
			this.numberCoefficients = numberCoefficients;
			
			// Compute the Mel Frequencey Filters
			var melFilter = new MelFilter(winsize, srate, numberFilters, 20);
			this.melScaleFreqsIndex = melFilter.MelScaleFreqsIndex;
			this.melScaleTriangleHeights = melFilter.MelScaleTriangleHeights;
			this.filterWeights = melFilter.FilterWeights;
			
			// Compute the DCT
			dct = new DctMatrix(numberCoefficients, numberFilters).Matrix;

			#if DEBUG
			dct.WriteAscii("dct-mirage-orig.ascii");
			dct.DrawMatrixGraph("dct-mirage-orig.png");
			#endif
		}
		
		/// <summary>
		/// Apply internal DCT and Mel Filterbands
		/// This method is faster than ApplyComirvaWay since it uses fewer loops.
		/// </summary>
		/// <param name="m">matrix (stftdata)</param>
		/// <returns>matrix mel scaled and dct'ed</returns>
		public Matrix ApplyMelScaleDCT(ref Matrix m)
		{
			using (new DebugTimer("ApplyMelScaleDCT(m)")) {
				
				// 4. Mel Scale Filterbank
				// Mel-frequency is proportional to the logarithm of the linear frequency,
				// reflecting similar effects in the human's subjective aural perception)
				Matrix mel = filterWeights * m;
				
				// 5. Take Logarithm
				for (int i = 0; i < mel.Rows; i++) {
					for (int j = 0; j < mel.Columns; j++) {
						mel.MatrixData[i][j] = (mel.MatrixData[i][j] < 1.0 ? 0 : (20.0 * Math.Log10(mel.MatrixData[i][j])));
					}
				}
				
				// 6. DCT (Discrete Cosine Transform)
				Matrix mfcc = dct * mel;
				
				return mfcc;
			}
		}

		/// <summary>
		/// Apply internal DCT and Mel Filterbands utilising the Comirva Matrix methods
		/// </summary>
		/// <param name="m">matrix (stftdata)</param>
		/// <returns>matrix mel scaled and dct'ed</returns>
		public Matrix ApplyMelScaleDCTComirva(ref Matrix m)
		{
			using (new DebugTimer("ApplyMelScaleDCTComirva(m)")) {
				
				// 4. Mel Scale Filterbank
				// Mel-frequency is proportional to the logarithm of the linear frequency,
				// reflecting similar effects in the human's subjective aural perception)
				m = filterWeights * m;

				// 5. Take Logarithm
				// to db
				double log10 = 20 * (1 / Math.Log(10)); // log for base 10 and scale by factor 10
				m.ThrunkAtLowerBoundary(1);
				m.LogEquals();
				m *= log10;

				// 6. DCT (Discrete Cosine Transform)
				m = dct * m;
				
				return m;
			}
		}
		
		/// <summary>
		/// Perform an inverse mfcc. E.g. perform an idct and inverse Mel Filterbands and return stftdata
		/// </summary>
		/// <param name="mfcc">mfcc matrix</param>
		/// <returns>matrix idct'ed and mel removed (e.g. stftdata)</returns>
		public Matrix InverseMelScaleDCT(ref Matrix mfcc)
		{
			using (new DebugTimer("InverseMelScaleDCT(mfcc)")) {
				
				// 6. Perform the IDCT (Inverse Discrete Cosine Transform)
				Matrix mel = dct.Transpose() * mfcc;
				
				// 5. Take Inverse Logarithm
				for (int i = 0; i < mel.Rows; i++) {
					for (int j = 0; j < mel.Columns; j++) {
						mel.MatrixData[i][j] = Math.Pow(10, (mel.MatrixData[i][j] / 20)) / melScaleTriangleHeights[0];
					}
				}
				
				// 4. Inverse Mel Scale using interpolation
				// i.e. from e.g.
				// mel=Rows: 40, Columns: 165 (average freq, time slice)
				// to
				// m=Rows: 1024, Columns: 165 (freq, time slice)
				//Matrix m = filterWeights.Transpose() * mel;
				var m = new Matrix(filterWeights.Columns, mel.Columns);
				InverseMelScaling(mel, m);
				
				return m;
			}
		}
		
		/// <summary>
		/// DCT
		/// </summary>
		/// <param name="m">matrix (logSpectrogram)</param>
		/// <returns>matrix dct'ed</returns>
		public Matrix ApplyDCT(ref Matrix m) {
			
			using (new DebugTimer("ApplyDCT(matrix)")) {
				
				// 6. DCT (Discrete Cosine Transform)
				m = dct * m;
				return m;
			}
		}
		
		/// <summary>
		/// Perform an inverse DCT
		/// </summary>
		/// <param name="input">dct matrix</param>
		/// <returns>matrix idct'ed (e.g. logSpectrogram)</returns>
		public Matrix InverseDCT(ref Matrix input) {
			
			using (new DebugTimer("InverseDCT(matrix)")) {
				
				// 6. Perform the IDCT (Inverse Discrete Cosine Transform)
				Matrix m = dct.Transpose() * input;
				return m;
			}
		}
		
		/// <summary>
		/// Mel Scale Haar Wavelet Transform
		/// </summary>
		/// <param name="m">matrix (stftdata)</param>
		/// <returns>matrix mel scaled and wavelet'ed</returns>
		public Matrix ApplyMelScaleWaveletPadding(ref Matrix m) {
			
			using (new DebugTimer("ApplyMelScaleWaveletPadding(m)")) {
				
				// 4. Mel Scale Filterbank
				// Mel-frequency is proportional to the logarithm of the linear frequency,
				// reflecting similar effects in the human's subjective aural perception)
				Matrix mel = filterWeights * m;
				
				// 5. Take Logarithm
				for (int i = 0; i < mel.Rows; i++) {
					for (int j = 0; j < mel.Columns; j++) {
						mel.MatrixData[i][j] = (mel.MatrixData[i][j] < 1.0 ? 0 : (20.0 * Math.Log10(mel.MatrixData[i][j])));
					}
				}
				
				// 6. Wavelet Transform
				// make sure the matrix is square before transforming (by zero padding)
				Matrix resizedMatrix;
				if (!mel.IsSymmetric() || !MathUtils.IsPowerOfTwo(mel.Rows)) {
					int size = (mel.Rows > mel.Columns ? mel.Rows : mel.Columns);
					int sizePow2 = MathUtils.NextPowerOfTwo(size);
					resizedMatrix = mel.Resize(sizePow2, sizePow2);
				} else {
					resizedMatrix = mel;
				}
				Matrix wavelet = WaveletUtils.HaarWaveletTransform2D(resizedMatrix.MatrixData, true);
				
				return wavelet;
			}
		}
		
		/// <summary>
		/// Perform an inverse haar wavelet mel scaled transform. E.g. perform an ihaar2d and inverse Mel Filterbands and return stftdata
		/// </summary>
		/// <param name="wavelet">wavelet matrix</param>
		/// <returns>matrix inverse wavelet'ed and mel removed (e.g. stftdata)</returns>
		public Matrix InverseMelScaleWaveletPadding(ref Matrix wavelet) {
			
			using (new DebugTimer("InverseMelScaleWaveletPadding(wavelet)")) {
				
				// 6. Perform the Inverse Wavelet Transform
				Matrix mel = WaveletUtils.InverseHaarWaveletTransform2D(wavelet.MatrixData);
				
				// Resize (remove padding)
				mel = mel.Resize(melScaleFreqsIndex.Length - 2, wavelet.Columns);
				
				// 5. Take Inverse Logarithm
				// Divide with first triangle height in order to scale properly
				for (int i = 0; i < mel.Rows; i++) {
					for (int j = 0; j < mel.Columns; j++) {
						mel.MatrixData[i][j] = Math.Pow(10, (mel.MatrixData[i][j] / 20)) / melScaleTriangleHeights[0];
					}
				}
				
				// 4. Inverse Mel Scale using interpolation
				// i.e. from e.g.
				// mel=Rows: 40, Columns: 165 (average freq, time slice)
				// to
				// m=Rows: 1024, Columns: 165 (freq, time slice)
				//Matrix m = filterWeights.Transpose() * mel;
				var m = new Matrix(filterWeights.Columns, mel.Columns);
				InverseMelScaling(mel, m);

				return m;
			}
		}
		
		/// <summary>
		/// Haar Wavelet Transform and Compress
		/// </summary>
		/// <param name="m">matrix (logSpectrogram)</param>
		/// <returns>matrix wavelet'ed</returns>
		public Matrix ApplyWaveletCompression(ref Matrix m, out int lastHeight, out int lastWidth) {
			using (new DebugTimer("ApplyWaveletCompression(matrix)")) {
				
				// Wavelet Transform
				Matrix wavelet = m.Copy();
				WaveletCompress.HaarTransform2D(wavelet.MatrixData, numberWaveletTransforms, out lastHeight, out lastWidth);
				
				// Compress
				Matrix waveletCompressed = wavelet.Resize(numberCoefficients, wavelet.Columns);
				
				return waveletCompressed;
			}
		}
		
		/// <summary>
		/// Perform an inverse decompressed haar wavelet transform. E.g. perform an ihaar2d and return logSpectrogram
		/// </summary>
		/// <param name="wavelet">wavelet matrix</param>
		/// <returns>matrix inverse wavelet'ed (e.g. logSpectrogram)</returns>
		public Matrix InverseWaveletCompression(ref Matrix wavelet, int firstHeight, int firstWidth, int rows, int columns) {
			using (new DebugTimer("InverseWaveletCompression(wavelet)")) {
				
				// Resize, e.g. Uncompress
				wavelet = wavelet.Resize(rows, columns);

				// 6. Perform the Inverse Wavelet Transform
				Matrix m = wavelet.Copy();
				WaveletDecompress.Decompress2D(m.MatrixData, numberWaveletTransforms, firstHeight, firstWidth);
				
				return m;
			}
		}
		
		/// <summary>
		/// Mel Scale Haar Wavelet Transform and Compress
		/// </summary>
		/// <param name="m">matrix (stftdata)</param>
		/// <returns>matrix mel scaled and wavelet'ed</returns>
		public Matrix ApplyMelScaleAndWaveletCompress(ref Matrix m, out int lastHeight, out int lastWidth) {
			using (new DebugTimer("ApplyMelScaleAndWaveletCompress(m)")) {
				
				// 4. Mel Scale Filterbank
				// Mel-frequency is proportional to the logarithm of the linear frequency,
				// reflecting similar effects in the human's subjective aural perception)
				Matrix mel = filterWeights * m;
				
				// 5. Take Logarithm
				for (int i = 0; i < mel.Rows; i++) {
					for (int j = 0; j < mel.Columns; j++) {
						mel.MatrixData[i][j] = (mel.MatrixData[i][j] < 1.0 ? 0 : (20.0 * Math.Log10(mel.MatrixData[i][j])));
					}
				}
				
				// 6. Perform the Wavelet Transform and Compress
				Matrix waveletCompressed = ApplyWaveletCompression(ref mel, out lastHeight, out lastWidth);
				
				return waveletCompressed;
			}
		}

		/// <summary>
		/// Perform an inverse haar wavelet mel scaled transform. E.g. perform an ihaar2d and inverse Mel Filterbands and return stftdata
		/// </summary>
		/// <param name="wavelet">wavelet matrix</param>
		/// <returns>matrix inverse wavelet'ed and mel removed (e.g. stftdata)</returns>
		public Matrix InverseMelScaleAndWaveletCompress(ref Matrix wavelet, int firstHeight, int firstWidth) {
			using (new DebugTimer("InverseMelScaleAndWaveletCompress(wavelet)")) {
				
				// 6. Ucompress and then perform the Inverse Wavelet Transform
				Matrix mel = InverseWaveletCompression(ref wavelet, firstHeight, firstWidth, melScaleFreqsIndex.Length - 2, wavelet.Columns);
				
				// 5. Take Inverse Logarithm
				// Divide with first triangle height in order to scale properly
				for (int i = 0; i < mel.Rows; i++) {
					for (int j = 0; j < mel.Columns; j++) {
						mel.MatrixData[i][j] = Math.Pow(10, (mel.MatrixData[i][j] / 20)) / melScaleTriangleHeights[0];
					}
				}
				
				// 4. Inverse Mel Scale using interpolation
				// i.e. from e.g.
				// mel=Rows: 40, Columns: 165 (average freq, time slice)
				// to
				// m=Rows: 1024, Columns: 165 (freq, time slice)
				//Matrix m = filterWeights.Transpose() * mel;
				var m = new Matrix(filterWeights.Columns, mel.Columns);
				InverseMelScaling(mel, m);

				return m;
			}
		}
		
		/// <summary>
		/// Mel Scale and Log
		/// </summary>
		/// <param name="m">matrix (stftdata)</param>
		/// <returns>matrix mel scaled</returns>
		public Matrix ApplyMelScaleAndLog(ref Matrix m) {
			using (new DebugTimer("ApplyMelScaleAndLog(m)")) {
				
				// 4. Mel Scale Filterbank
				// Mel-frequency is proportional to the logarithm of the linear frequency,
				// reflecting similar effects in the human's subjective aural perception)
				Matrix mel = filterWeights * m;
				
				// 5. Take Logarithm
				for (int i = 0; i < mel.Rows; i++) {
					for (int j = 0; j < mel.Columns; j++) {
						mel.MatrixData[i][j] = (mel.MatrixData[i][j] < 1.0 ? 0 : (20.0 * Math.Log10(mel.MatrixData[i][j])));
						//mel.MatrixData[i][j] = 20.0 * Math.Log10(mel.MatrixData[i][j]);
					}
				}
				
				return mel;
			}
		}
		
		/// <summary>
		/// Perform an inverse mel scale and log.
		/// </summary>
		/// <param name="mel">mel scaled matrix</param>
		/// <returns>matrix mel removed and un-logged (e.g. stftdata)</returns>
		public Matrix InverseMelScaleAndLog(ref Matrix mel) {
			using (new DebugTimer("InverseMelScaleAndLog(mel)"))
			{
				// 5. Take Inverse Logarithm
				// Divide with first triangle height in order to scale properly
				for (int i = 0; i < mel.Rows; i++) {
					for (int j = 0; j < mel.Columns; j++) {
						mel.MatrixData[i][j] = Math.Pow(10, (mel.MatrixData[i][j] / 20)) / melScaleTriangleHeights[0];
					}
				}
				
				// 4. Inverse Mel Scale using interpolation
				// i.e. from e.g.
				// mel=Rows: 40, Columns: 165 (average freq, time slice)
				// to
				// m=Rows: 1024, Columns: 165 (freq, time slice)
				//Matrix m = filterWeights.Transpose() * mel;
				var m = new Matrix(filterWeights.Columns, mel.Columns);
				InverseMelScaling(mel, m);
				
				return m;
			}
		}
		
		/// <summary>
		/// Perform an inverse mel scale using interpolation
		/// i.e. from e.g.
		/// mel=Rows: 40, Columns: 165 (average freq, time slice)
		/// to
		/// m=Rows: 1024, Columns: 165 (freq, time slice)
		/// </summary>
		/// <param name="mel"></param>
		/// <param name="m"></param>
		private void InverseMelScaling(Matrix mel, Matrix m) {

			// for each row, interpolate values to next row according to mel scale
			for (int j = 0; j < mel.Columns; j++) {
				for (int i = 0; i < mel.Rows-1; i++) {
					double startValue = mel.MatrixData[i][j];
					double endValue = mel.MatrixData[i+1][j];
					
					// what indexes in resulting matrix does this row cover?
					//Console.Out.WriteLine("Mel Row index {0} corresponds to Linear Row index {1} - {2} [{3:0.00} - {4:0.00}]", i, freqsIndex[i+1], freqsIndex[i+2]-1, startValue, endValue);

					// add interpolated values
					AddInterpolatedValues(m, melScaleFreqsIndex[i+1], melScaleFreqsIndex[i+2], startValue, endValue, j);
				}

				// last row
				int iLast = mel.Rows - 1;
				double startValueLast = mel.MatrixData[iLast][j];
				double endValueLast = 0.0; // mel.MatrixData[iLast][j];

				// what indexes in resulting matrix does this row cover?
				//Console.Out.WriteLine("Mel Row index {0} corresponds to Linear Row index {1} - {2} [{3:0.00} - {4:0.00}]", iLast, freqsIndex[iLast+1], freqsIndex[iLast+2]-1, startValueLast, endValueLast);

				// add interpolated values
				AddInterpolatedValues(m, melScaleFreqsIndex[iLast+1], melScaleFreqsIndex[iLast+2], startValueLast, endValueLast, j);
			}
			
		}
		
		private void AddInterpolatedValues(Matrix m, int startIndex, int endIndex, double startValue, double endValue, int columnIndex) {
			
			// interpolate and add values
			int partSteps =  endIndex - startIndex;
			for (int step = 0; step < partSteps; step ++) {
				double p = (double) step / (double) partSteps;

				// interpolate
				double val = MathUtils.Interpolate(startValue, endValue, p);
				
				// add to matrix data
				m.MatrixData[startIndex+step][columnIndex] = val;
			}
		}
	}
}
