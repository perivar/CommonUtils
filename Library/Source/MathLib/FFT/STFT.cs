using System;
using CommonUtils.MathLib.MatrixLib;

namespace CommonUtils.MathLib.FFT
{
	/// <summary>
	/// Short Term Fourier Transformation method copied from the Mirage project:
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
	public class STFT
	{
		int winsize;
		int hopsize;
		FFT fft; // Use the mirage fft class
		
		/// <summary>
		/// Instantiate a new Stft Class
		/// </summary>
		/// <param name="winsize">FFT window size</param>
		/// <param name="hopsize">Value to hop on to the next window</param>
		/// <param name="windowType">Window function to apply to every window processed</param>
		public STFT(FFTWindowType windowType, int winsize, int hopsize)
		{
			this.winsize = winsize;
			this.hopsize = hopsize;
			fft = new FFT(windowType, winsize);
		}
		
		/// <summary>
		/// Apply the STFT on the audiodata
		/// </summary>
		/// <param name="audiodata">Audiodata to apply the STFT on</param>
		/// <returns>A matrix with the result of the STFT</returns>
		public Matrix Apply(float[] audiodata)
		{
			using (new DebugTimer("Apply(audiodata)"))
			{
				int hops = (audiodata.Length - winsize)/ hopsize; // PIN: Removed + 1
				
				// Create a Matrix with "winsize" Rows and "hops" Columns
				// Matrix[Row, Column]
				var stft = new Matrix(winsize/2, hops);
				
				for (int i = 0; i < hops; i++) {
					// Lomont RealFFT seems to be the fastest option
					//fft.ComputeMatrixUsingFftw(ref stft, i, audiodata, i*hopsize);
					//fft.ComputeMatrixUsingLomontTableFFT (ref stft, i, audiodata, i*hopsize);
					fft.ComputeMatrixUsingLomontRealFFT(ref stft, i, audiodata, i*hopsize);
				}
				return stft;
			}
		}
		
		/// <summary>
		/// Perform an inverse STFT and return the audiodata
		/// </summary>
		/// <param name="stft">A matrix with the STFT</param>
		/// <returns>Audio data</returns>
		/// <see cref="http://stackoverflow.com/questions/1230906/reverse-spectrogram-a-la-aphex-twin-in-matlab">Reverse Spectrogram A La Aphex Twin in MATLAB</see>
		public double[] InverseStft(Matrix stft) {
			
			using (new DebugTimer("InverseStft(stft)"))
			{
				// stft is a Matrix with "winsize" Rows and "hops" Columns
				int columns = stft.Columns;

				int signalLengh = winsize + (columns)*hopsize; // PIN: Removed -1 from (columns-1)
				var signal = new double[signalLengh];
				
				// Take the ifft of each column of pixels and piece together the results.
				for (int i = 0; i < columns; i++) {
					fft.ComputeInverseMatrixUsingLomontTableFFT(stft, i, ref signal, winsize, hopsize);
					//fft.ComputeInverseComirvaMatrixUsingLomontRealFFT(stft, i, ref signal, winsize, hopsize);
				}
				return signal;
			}
		}
	}
}
