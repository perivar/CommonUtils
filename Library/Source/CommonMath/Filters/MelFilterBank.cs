// Copyright (c) 2011 Sebastian Böhm sebastian@sometimesfood.org
//                    Heinrich Fink hf@hfink.eu
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Collections.Generic;
using CommonUtils.CommonMath.Comirva;

namespace CommonUtils.CommonMath.Filters
{
	/// <summary>
	/// This class represents the filters necessary to warp an audio spectrum
	/// into Mel-Frequency scaling. It is basically a collection of properly
	/// placed TriangleFilters.
	/// </summary>
	public class MelFilterBank
	{
		private double minFreq;
		private double maxFreq;
		private int numMelBands;
		private int numBins;
		private int sampleRate;
		private readonly bool doNormalizeFilterArea;
		private List<TriangleFilter> filters = new List<TriangleFilter>();
		
		/// <summary>
		/// Return a list of the triangle filters
		/// </summary>
		public List<TriangleFilter> Filters {
			get {
				return filters;
			}
		}
		
		/// <summary>
		/// Creates a new MelFilterBank and default to normalized TriangleFilter area
		/// </summary>
		/// <param name="minFreq">The minimum frequency in hz to be considered, i.e.
		/// the left edge of the first TriangleFilter.</param>
		/// <param name="maxFreq">The maximum frequency in hz to be considered, i.e.
		/// the right edge of the last TriangleFilter.</param>
		/// <param name="numMelBands">The number of Mel bands to be calculated, i.e.
		/// the number of TriangleFilters to be applied.</param>
		/// <param name="numBins">The number of bins that are present in the fft_buffer
		/// that will be passed to the MelFilterBank.Apply method. This is also
		/// required to properly configure the TriangleFilter instances which
		/// operate on array indices only.</param>
		/// <param name="sampleRate">The original sample rate the FFT buffer which will
		/// be passed to MelFilterBank.Apply is based on.</param>
		public MelFilterBank(double minFreq, double maxFreq, int numMelBands, int numBins, int sampleRate) : this(minFreq, maxFreq, numMelBands, numBins, sampleRate, true)
		{
		}

		/// <summary>
		/// Creates a new MelFilterBank.
		/// </summary>
		/// <param name="minFreq">The minimum frequency in hz to be considered, i.e.
		/// the left edge of the first TriangleFilter.</param>
		/// <param name="maxFreq">The maximum frequency in hz to be considered, i.e.
		/// the right edge of the last TriangleFilter.</param>
		/// <param name="numMelBands">The number of Mel bands to be calculated, i.e.
		/// the number of TriangleFilters to be applied.</param>
		/// <param name="numBins">The number of bins that are present in the fft_buffer
		/// that will be passed to the MelFilterBank.Apply method. This is also
		/// required to properly configure the TriangleFilter instances which
		/// operate on array indices only. (half the window size)</param>
		/// <param name="sampleRate">The original sample rate the FFT buffer which will
		/// be passed to MelFilterBank.Apply is based on.</param>
		/// <param name="doNormalizeFilterArea">If set to "true", the area of the
		/// created TriangleFilter will be normalized, e.g. the height of the
		/// filter's triangle shape will be configured in a way, that the area
		/// of the triangle shape equals one.</param>
		public MelFilterBank(double minFreq, double maxFreq, int numMelBands, int numBins, int sampleRate, bool doNormalizeFilterArea)
		{
			this.minFreq = minFreq;
			this.maxFreq = maxFreq;
			this.numMelBands = numMelBands;
			this.numBins = numBins;
			this.sampleRate = sampleRate;
			this.doNormalizeFilterArea = doNormalizeFilterArea;
			
			// Let's do some argument checking
			if ((minFreq >= maxFreq) || (maxFreq == 0))
			{
				throw new ArgumentException(String.Format("Invalid min/max frequencies for MelFilterBank: min = '{0}' max = '{1}'", minFreq, maxFreq));
			}
			
			if (numMelBands == 0)
			{
				throw new ArgumentException(String.Format("Invalid number of mel bands for MelFilterBank: n = {0}", numMelBands));
			}
			
			if (sampleRate == 0)
			{
				throw new ArgumentException(String.Format("Invalid sample rate for MelFilterBank: s = {0}", sampleRate));
			}
			
			if (numBins == 0)
			{
				throw new ArgumentException(String.Format("Invalid number of bins for MelFilterBank: s = '{0}'", numBins));
			}
			
			// 2 * numBins should be the same as window length
			double deltaFreq = (double)sampleRate / (2 * numBins);
			
			double melMin = MelUtils.LinToMelFreq(minFreq);
			double melMax = MelUtils.LinToMelFreq(maxFreq);
			
			// We divide by #band + 1 as min / max should present the beginning / end
			// of beginng up / ending low slope, i.e. it's not the centers of each
			// band that represent min/max frequency in mel bands.
			double deltaFreqMel = (melMax - melMin) / (numMelBands + 1);
			
			// Fill up equidistant spacing in mel-space
			double melLeft = melMin;
			for (int i = 0; i < numMelBands; i++)
			{
				double melCenter = melLeft + deltaFreqMel;
				double melRight = melCenter + deltaFreqMel;
				
				double leftHz = MelUtils.MelToLinFreq(melLeft);
				double rightHz = MelUtils.MelToLinFreq(melRight);
				
				// align to closest num_bins (round)
				int leftBin = (int)((leftHz / deltaFreq) + 0.5);
				int rightBin = (int)((rightHz / deltaFreq) + 0.5);
				
				// calculate normalized height
				double height = 1.0;
				
				if (doNormalizeFilterArea) {
					height = 2.0 / (rightBin - leftBin);
				}
				
				// Create the actual filter
				var filter = new TriangleFilter(leftBin, rightBin, height);
				filters.Add(filter);
				
				// next left edge is current center
				melLeft = melCenter;
			}
		}

		/// <summary>
		/// Apply all filters on the incoming FFT data, and write out the results
		/// into an array.
		/// </summary>
		/// <param name="fftData">The incoming FFT data on which the triangle filters
		/// will be applied on.</param>
		/// <param name="melBands">The caller is responsible that the passed array
		/// accomodates at least num_mel_bands elements. On output this array
		/// will be filled with the resulting Mel-Frequency warped spectrum.</param>
		public void Apply(double[] fftData, double[] melBands)
		{
			// we assume the caller passes arrays with appropriates sizes
			for (int i = 0; i < numMelBands; ++i) {
				melBands[i] = filters[i].Apply(fftData);
			}
		}

		/// <summary>
		/// Used for debugging. Prints out a detailed descriptions of the configured filters.
		/// </summary>
		public void Print() {
			Print(Console.Out);
		}
		
		/// <summary>
		/// Used for debugging. Prints out a detailed descriptions of the configured filters.
		/// </summary>
		public void Print(TextWriter writer)
		{
			writer.Write("mel_filters = [");
			for (int i = 0; i < filters.Count; ++i)
			{
				if (i != 0)
				{
					writer.WriteLine(";");
				}
				
				writer.Write(filters[i]);
			}
			writer.Write("];");
			writer.WriteLine();
			writer.WriteLine();
		}
		
		/// <summary>
		/// Return the filter bank as a Matrix
		/// Note this is calculated every time!
		/// </summary>
		public Matrix Matrix {
			get {
				int numberFilters = Filters.Count;
				
				// create the filter bank matrix
				var matrix = new double[numberFilters][];

				// fill each row of the filter bank matrix with one triangular mel filter
				for(int i = 0; i < numberFilters; i++)
				{
					TriangleFilter triFilter = Filters[i];
					int leftEdge = triFilter.LeftEdge;
					int rightEdge = triFilter.RightEdge;
					int size = triFilter.Size;
					double[] filterdata = triFilter.FilterData;

					var filter = new double[numBins];
					
					// zero pad before the filter starts
					for (int j = 0; j < leftEdge; j++)
					{
						filter[j] = 0;
					}

					// and insert filterdata
					for (int j = 0; j < size - 1; j++)
					{
						filter[j + leftEdge] = filterdata[j];
					}
					
					matrix[i] = filter;
				}

				// return the filter bank
				return new Matrix(matrix);
			}
		}
	}
}
