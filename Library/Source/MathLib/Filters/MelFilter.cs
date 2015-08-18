using System;
using CommonUtils.MathLib.MatrixLib;

namespace CommonUtils.MathLib.Filters
{
	/// <summary>
	/// Description of MelFilter.
	/// </summary>
	public class MelFilter
	{
		readonly Matrix filterWeights;
		int[] melScaleFreqsIndex; 			// store the mel scale indexes
		double[] melScaleTriangleHeights;	// store the mel filter triangle heights

		#region Getters
		public int[] MelScaleFreqsIndex {
			get {
				return melScaleFreqsIndex;
			}
		}

		public double[] MelScaleTriangleHeights {
			get {
				return melScaleTriangleHeights;
			}
		}

		public Matrix FilterWeights {
			get {
				return filterWeights;
			}
		}
		#endregion
		
		/// <summary>
		/// Compute Mel Frequency Filters
		/// </summary>
		/// <param name="winsize">the window size</param>
		/// <param name="sampleRate">the sample rate</param>
		/// <param name="numberFilters">number of filters in the filterbank</param>
		/// <param name="minFreq">lowest frequency in the range of interest</param>
		public MelFilter(int winsize, int sampleRate, int numberFilters, int minFreq)
		{
			// arrays to store the mel frequencies and the herz frequencies
			var mel = new double[sampleRate/2 - minFreq + 1];
			var freq = new double[sampleRate/2 - minFreq + 1];
			
			// Mel Scale from StartFreq to SamplingRate/2, step every 1Hz
			for (int f = minFreq; f <= sampleRate/2; f++) {
				mel[f-minFreq] = MelUtils.LinToMelFreq(f);
				freq[f-minFreq] = f;
			}
			
			// prepare filters
			var freqs = new double[numberFilters + 2];
			melScaleFreqsIndex = new int[numberFilters + 2];
			
			for (int f = 0; f < freqs.Length; f++) {
				double melIndex = 1.0 + ((mel[mel.Length - 1] - 1.0) /
				                         (freqs.Length - 1.0) * f);
				double min = Math.Abs(mel[0] - melIndex);
				freqs[f] = freq[0];
				
				for (int j = 1; j < mel.Length; j++) {
					double cur = Math.Abs(mel[j] - melIndex);
					if (cur < min) {
						min = cur;
						freqs[f] = freq[j];
					}
				}
				
				melScaleFreqsIndex[f] = MathUtils.FreqToIndex(freqs[f], sampleRate, winsize);
			}
			
			// triangle heights
			melScaleTriangleHeights = new double[numberFilters];
			for (int j = 0; j < melScaleTriangleHeights.Length; j++) {
				
				// Let's make the filter area equal to 1.
				// filterHeight = 2.0 / (rightEdge - leftEdge);
				melScaleTriangleHeights[j] = 2.0 / (freqs[j+2] - freqs[j]);
			}
			
			var fftFreq = new double[winsize/2 + 1];
			for (int j = 0; j < fftFreq.Length; j++) {
				fftFreq[j] = ((sampleRate/2)/(fftFreq.Length -1.0)) * j;
			}
			
			// Compute the mel filter Weights
			filterWeights = new Matrix(numberFilters, winsize/2);
			for (int j = 0; j < numberFilters; j++) {
				for (int k = 0; k < fftFreq.Length; k++) {
					if ((fftFreq[k] > freqs[j]) && (fftFreq[k] <= freqs[j+1])) {
						
						filterWeights.MatrixData[j][k] = (melScaleTriangleHeights[j] *
						                                  ((fftFreq[k]-freqs[j])/(freqs[j+1]-freqs[j])));
					}
					if ((fftFreq[k] > freqs[j+1]) &&
					    (fftFreq[k] < freqs[j+2])) {
						
						filterWeights.MatrixData[j][k] += (melScaleTriangleHeights[j] *
						                                   ((freqs[j+2]-fftFreq[k])/(freqs[j+2]-freqs[j+1])));
					}
				}
			}
		}
		
		public void Dump() {
			//filterWeights.WriteAscii("melfilters.ascii");
			filterWeights.WriteCSV("melfilters.csv");
			filterWeights.DrawMatrixGraph("melfilters.png");
		}
	}
}
