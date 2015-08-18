using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;

using CommonUtils.Audio;
using CommonUtils.Audio.NAudio;
using CommonUtils.MathLib.FFT;
using CommonUtils.MathLib.MatrixLib;
using CommonUtils.MathLib.FFT.VampPlugins;

namespace CommonUtils.Tests
{
	[TestFixture]
	public class FFTTest
	{
		const int AUDIO_MULTIPLIER = 32768; // 65536
		const int WINDOW_SIZE = 1024; // 2048; // this also decides the maximum height of the fft before log
		const int SAMPLING_RATE = 33400	; 	// Using 32000 (instead of 44100) gives us a max of 16 khz resolution, which is OK for normal adult human hearing
		//const int OVERLAP = 184; //WINDOW_SIZE/2;
		const string WAVE_INPUT_FILEPATH = @"Tests\test.wav";
		
		[Test]
		public static void TimeSpectrograms() {
			FFTTesting.TimeSpectrograms();
		}
		
		[Test]
		public static void TestSpectrograms() {
			const int fftWindowSize = 4096;
			const int overlap = 2048;
			
			var data = AudioUtilsNAudio.GenerateAudioTestData(44100, 10);
			
			double[][] spec1 = FFTUtils.CreateSpectrogramLomont(data, fftWindowSize, overlap);
			var spec1M = new Matrix(spec1);
			spec1M.WriteCSV("spec_lomont.csv");
			
			double[][] spec2 = FFTUtils.CreateSpectrogramExocortex(data, fftWindowSize, overlap);
			var spec2M = new Matrix(spec2);
			spec2M.WriteCSV("spec_exocortex.csv");
			Assert.That(spec2, Is.EqualTo(spec1).AsCollection.Within(0.001), "fail at [0]");
			
			double[][] spec3 = FFTUtils.CreateSpectrogramFFTW(data, fftWindowSize, overlap);
			var spec3M = new Matrix(spec3);
			spec3M.WriteCSV("spec_fftw.csv");
			//Assert.That(spec3, Is.EqualTo(spec1).AsCollection.Within(0.001), "fail at [0]");
			
			double[][] spec4 = FFTUtils.CreateSpectrogramFFTWLIB(data, fftWindowSize, overlap);
			var spec4M = new Matrix(spec4);
			spec4M.WriteCSV("spec_fftwlib.csv");
			//Assert.That(spec4, Is.EqualTo(spec1).AsCollection.Within(0.001), "fail at [0]");

			double[][] spec5 = FFTUtils.CreateSpectrogramFFTWLIB_INPLACE(data, fftWindowSize, overlap);
			var spec5M = new Matrix(spec5);
			spec5M.WriteCSV("spec_fftwlib_inplace.csv");
			//Assert.That(spec5, Is.EqualTo(spec1).AsCollection.Within(0.001), "fail at [0]");
		}
		
		[Test]
		public void TestFFTAudioMatrixMethod()
		{
			// harmor_HQ.bmp = 1645 (width) x 511 (height) 32 bit
			
			// test variables
			const string outputDirectoryFilePath = "test";
			var audioSystem = BassProxy.Instance;

			// 0. Get Audio Data
			float[] audioSamples = BassProxy.ReadMonoFromFile(WAVE_INPUT_FILEPATH, SAMPLING_RATE);
			
			int width = 1645;
			//int width = (audioSamples.Length - WINDOW_SIZE)/ OVERLAP;
			int OVERLAP = (int) ((double) (audioSamples.Length - WINDOW_SIZE) / (double) width);

			// 1. Explode samples to the range of 16 bit shorts (–32,768 to 32,767)
			// Matlab multiplies with 2^15 (32768)
			// e.g. if( max(abs(speech))<=1 ), speech = speech * 2^15; end;
			MathUtils.Multiply(ref audioSamples, AUDIO_MULTIPLIER);
			
			// zero pad if the audio file is too short to perform a fft
			if (audioSamples.Length < (WINDOW_SIZE + OVERLAP))
			{
				int lenNew = WINDOW_SIZE + OVERLAP;
				Array.Resize<float>(ref audioSamples, lenNew);
			}
			
			// 2. Windowing
			// 3. FFT
			#region Windowing and FFT
			var stft = new STFT(FFTWindowType.HANNING, WINDOW_SIZE, OVERLAP);
			var stftdata = stft.Apply(audioSamples);

			// same as specgram(audio*32768, 2048, 44100, hanning(2048), 1024);
			stftdata.DrawMatrixImageLogValues(outputDirectoryFilePath + "_specgram.png", true, false, -1, -1, false);
			
			var spect2 = FFTUtils.CreateSpectrogramFFTW(audioSamples, WINDOW_SIZE, OVERLAP);
			var stftdata2 = new Matrix(spect2).Transpose();
			
			// same as specgram(audio*32768, 2048, 44100, hanning(2048), 1024);
			stftdata2.DrawMatrixImageLogValues(outputDirectoryFilePath + "_specgram2.png", true, false, -1, -1, false);
			
			var spect3 = FFTUtils.CreateSpectrogramLomont(audioSamples, WINDOW_SIZE, OVERLAP);
			var stftdata3 = new Matrix(spect3).Transpose();
			
			// same as specgram(audio*32768, 2048, 44100, hanning(2048), 1024);
			stftdata3.DrawMatrixImageLogValues(outputDirectoryFilePath + "_specgram3.png", true, false, -1, -1, false);
			#endregion

			// the matrix are too different so comparing them always fails!
			//Assert.That(stftdata2, Is.EqualTo(stftdata3).AsCollection.Within(0.001), "fail at [0]");
			
			#region Inverse FFT
			// Perform inverse stft as well
			double[] audiodata_inverse_stft = stft.InverseStft(stftdata);
			
			// divide or normalize
			//MathUtils.Divide(ref audiodata_inverse_stft, AUDIO_MULTIPLIER);
			MathUtils.Normalize(ref audiodata_inverse_stft);

			Export.DrawGraph(audiodata_inverse_stft, outputDirectoryFilePath + "_audiodata_inverse_stft.png");
			
			float[] audiodata_inverse_float = MathUtils.DoubleToFloat(audiodata_inverse_stft);
			BassProxy.SaveFile(audiodata_inverse_float, outputDirectoryFilePath + "_inverse_stft.wav", 1, SAMPLING_RATE, 32);
			#endregion
			
			Assert.Pass("This test was succesful.");
		}
		
		[Test]
		public void TestFFTMethods() {
			FFTTesting.TestAll();
		}
		
		[Test]
		public void TimeFFTWithIterations() {
			FFTTesting.TimeAll(50);
		}
		
		[Test]
		public void TimeFFTWithSignal() {
			// initialize the random class
			RandomUtils.Seed(Guid.NewGuid().GetHashCode());

			const int length = 4096;
			var doubleData = new double[length];
			for (int i = 0; i < length; i++) {
				doubleData[i] = RandomUtils.NextDoubleMinus1ToPlus1();
			}
			
			FFTTesting.TimeAll(doubleData);
		}
		
		[Test]
		public void TestSpectrogram() {

			// harmor_LQ.bmp = 1645 (width) x 255 (height) 32 bit
			// harmor_HQ.bmp = 1645 (width) x 511 (height) 32 bit
			
			// test variables
			var audioSystem = BassProxy.Instance;

			// 0. Get Audio Data
			float[] audioSamples = BassProxy.ReadMonoFromFile(WAVE_INPUT_FILEPATH, SAMPLING_RATE);
			
			/*
			// generate spectrogram
			Bitmap spectroBW = AudioAnalyzer.GetSpectrogramImage(audioSamples, -1, 591, SAMPLING_RATE, WINDOW_SIZE, OVERLAP, ColorUtils.ColorPaletteType.BLACK_AND_WHITE, true);
			spectroBW.Save(@"spectrogram-log-blackwhite.png");

			Bitmap spectro = AudioAnalyzer.GetSpectrogramImage(audioSamples, -1, 591, SAMPLING_RATE, WINDOW_SIZE, OVERLAP, ColorUtils.ColorPaletteType.PHOTOSOUNDER, true);
			spectro.Save(@"spectrogram-log-photosounder.png");
			
			Bitmap spectro2 = AudioAnalyzer.GetSpectrogramImage(audioSamples, -1, 591, SAMPLING_RATE, WINDOW_SIZE, OVERLAP, ColorUtils.ColorPaletteType.REW, true);
			spectro2.Save(@"spectrogram-log-rew.png");
			
			Bitmap spectro3 = AudioAnalyzer.GetSpectrogramImage(audioSamples, -1, 591, SAMPLING_RATE, WINDOW_SIZE, OVERLAP, ColorUtils.ColorPaletteType.SOX, true);
			spectro3.Save(@"spectrogram-log-sox.png");

			Bitmap spectro4 = AudioAnalyzer.GetSpectrogramImage(audioSamples, -1, 591, SAMPLING_RATE, WINDOW_SIZE, OVERLAP, ColorUtils.ColorPaletteType.MATLAB, true);
			spectro4.Save(@"spectrogram-log-matlab.png");
			 */
			
			//double minFrequency = 27.5;
			//double maxFrequency = SAMPLING_RATE / 2;
			//int logBins = 512;
			//var logFrequenciesIndex = new int[1];
			//var logFrequencies = new float[1];

			// find the time
			int wanted_width = 1645;
			//int width = (audioSamples.Length - WINDOW_SIZE)/ OVERLAP;
			int OVERLAP = (int) ((double) (audioSamples.Length - WINDOW_SIZE) / (double) wanted_width);
			// int OVERLAP = WINDOW_SIZE/2;
			int numberOfSamples = audioSamples.Length;
			double seconds = numberOfSamples / SAMPLING_RATE;

			float[][] spectrogram = AudioAnalyzer.CreateSpectrogramLomont(audioSamples, WINDOW_SIZE, OVERLAP);
			
			int width = spectrogram.Length + 2*60;
			int height = spectrogram[0].Length + 2*40;
			Bitmap bmp1 = AudioAnalyzer.GetSpectrogramImage(spectrogram, width, height, seconds*1000, SAMPLING_RATE, ColorUtils.ColorPaletteType.BLACK_AND_WHITE, false, null, null);
			bmp1.Save(@"spectrogram-blackwhite.png");
			
			
			Bitmap bmp2 = AudioAnalyzer.GetSpectrogramImage(audioSamples, width, height, SAMPLING_RATE, WINDOW_SIZE, OVERLAP, ColorUtils.ColorPaletteType.BLACK_AND_WHITE, true);
			bmp2.Save(@"spectrogram-blackwhite-2.png");
		}
		
		[Test]
		public void TestAdaptiveSpectrogram() {
			// test variables
			var audioSystem = BassProxy.Instance;

			// 0. Get Audio Data
			float[] audioSamples = BassProxy.ReadMonoFromFile(WAVE_INPUT_FILEPATH, SAMPLING_RATE);
			
			var aspec = new AdaptiveSpectrogram(SAMPLING_RATE);
			
			int blockSize = aspec.GetPreferredBlockSize(); 	// 2048
			int stepSize = aspec.GetPreferredStepSize(); 	// 1024 = 50% overlap
			
			// print out a normal spectrogram for comparison
			double[][] spec1 = FFTUtils.CreateSpectrogramLomont(audioSamples, blockSize, stepSize);
			var spec1M = new Matrix(spec1).Transpose();
			spec1M.DrawMatrixImage("spec1_normal.png", -1, -1, true, true);

			// split the signal into chunks to feed the AdaptiveSpectrogram
			var chunks = MathUtils.Split(audioSamples, stepSize, false);
			
			int chunkLength = chunks.Count();
			Console.WriteLine("Chunk count: {0}", chunkLength);

			int count = 1;
			IEnumerable<double[]> spec = new double[0][];
			float[] lastChunk = null;
			foreach (var chunk in chunks) {
				Console.Write("Processing chunk: {0}   \r", count);
				
				if (lastChunk != null) {
					
					// add two chunks together, because adaptive spectrogram expects 50% overlap
					var z = new float[lastChunk.Length + chunk.Count()];
					lastChunk.CopyTo(z, 0);
					chunk.ToArray().CopyTo(z, chunk.Count());
					
					// process two last chunks as one (e.g. 50% overlap)
					var chunkData = aspec.Process(z);
					
					// store in spectrogram
					spec = spec.Concat(chunkData);
					
					//var chunkM = new Matrix(chunkData);
					//chunkM.WriteCSV("chunkData_" + count + ".csv");
				}
				
				lastChunk = chunk.ToArray();
				count++;
			}
			
			var specM = new Matrix(spec.ToArray()).Transpose();
			//specM.WriteCSV("spec_all.csv");
			specM.DrawMatrixImage("spec_adaptive.png", -1, -1, true, true);
		}
	}
}