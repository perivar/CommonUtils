using System;
using NUnit.Framework;

using CommonUtils.Audio;
using CommonUtils.MathLib.MatrixLib;
using CommonUtils.MathLib.FFT;
using CommonUtils.MathLib.Filters;

namespace CommonUtils.Tests
{
	[TestFixture]
	public class FilterTests
	{
		[Test]
		public void TestMelFrequencyFiltering()
		{
			var audio = BassProxy.Instance;
			
			//const string fileName = @"Tests\Passacaglia, Handel-Saw-86bmp.wav";
			const string fileName = @"Tests\Passacaglia, Handel-Sine-86bmp.wav";

			const int sampleRate = 44100;
			const int fftWindowsSize = 2048;
			const int fftOverlap = 1024;
			const bool colorize = true;
			const int melFilters = 120;
			
			var monoSignal = BassProxy.ReadMonoFromFile(fileName, sampleRate);
			
			double[][] specNormal = FFTUtils.CreateSpectrogramLomont(monoSignal, fftWindowsSize, fftOverlap);
			var specNormalMat = new Matrix(specNormal).Transpose();
			specNormalMat.DrawMatrixImage("spec_normal.png", -1, -1, colorize, true);

			// Mel Scale Filterbank
			// Mel-frequency is proportional to the logarithm of the linear frequency,
			// reflecting similar effects in the human's subjective aural perception)
			var melFilter = new MelFilter(fftWindowsSize, sampleRate, melFilters, 0);

			var specNormalMelMat = melFilter.FilterWeights * specNormalMat;
			specNormalMelMat.DrawMatrixImage("spec_normal_mel.png", -1, -1, colorize, true);
			
			melFilter.FilterWeights.WriteCSV("melfilter_orig.csv");
			melFilter.FilterWeights.DrawMatrixGraph("melfilter_orig.png");
			
			var melFilterBank = new MelFilterBank(0, sampleRate/2, melFilters, fftOverlap, sampleRate);
			var melFilterBankMat = melFilterBank.Matrix;
			melFilterBankMat.WriteCSV("melfilter_new.csv");
			melFilterBankMat.DrawMatrixGraph("melfilter_new.png");
			
			var specNormalMelMatNew = melFilterBankMat * specNormalMat;
			specNormalMelMatNew.DrawMatrixImage("spec_normal_mel_new.png", -1, -1, colorize, true);
		}
	}
}
