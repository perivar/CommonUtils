using System;
using System.Drawing;
using NUnit.Framework;

using CommonUtils.Audio;
using CommonUtils.CommonMath.FFT;
using CommonUtils.CommonMath.Comirva;

namespace CommonUtils.Tests
{
	[TestFixture]
	public class FFTTest
	{
		const int AUDIO_MULTIPLIER = 32768; // 65536
		const int WINDOW_SIZE = 1024; // 2048; // this also decides the maximum height of the fft before log
		const int SAMPLING_RATE = 44100; 	// Using 32000 (instead of 44100) gives us a max of 16 khz resolution, which is OK for normal adult human hearing
		const int OVERLAP = 256; //WINDOW_SIZE/2;
		const string WAVE_INPUT_FILEPATH = @"Tests\test.wav";
		
		[Test]
		public void TestFFTAudioMatrixMethod()
		{
			// test variables
			const string outputDirectoryFilePath = "test";
			var audioSystem = BassProxy.Instance;

			// 0. Get Audio Data
			float[] audioSamples = BassProxy.ReadMonoFromFile(WAVE_INPUT_FILEPATH);
			
			// 1. Explode samples to the range of 16 bit shorts (–32,768 to 32,767)
			// Matlab multiplies with 2^15 (32768)
			// e.g. if( max(abs(speech))<=1 ), speech = speech * 2^15; end;
			MathUtils.Multiply(ref audioSamples, AUDIO_MULTIPLIER);
			
			// zero pad if the audio file is too short to perform a fft
			if (audioSamples.Length < (WINDOW_SIZE + OVERLAP))
			{
				const int lenNew = WINDOW_SIZE + OVERLAP;
				Array.Resize<float>(ref audioSamples, lenNew);
			}
			
			// 2. Windowing
			// 3. FFT
			var stft = new STFT(WINDOW_SIZE, OVERLAP, new HannWindow());
			var stftdata = stft.Apply(audioSamples);

			//stftdata.WriteAscii(outputFilePath + "_stftdata.ascii");
			//stftdata.WriteCSV(outputFilePath + "_stftdata.csv", ";");
			
			// same as specgram(audio*32768, 2048, 44100, hanning(2048), 1024);
			stftdata.DrawMatrixImageLogValues(outputDirectoryFilePath + "_specgram.png", true, false, -1, -1, false);
			
			// spec gram with log values for the y axis (frequency)
			stftdata.DrawMatrixImageLogY(outputDirectoryFilePath + "_specgramlog.png", SAMPLING_RATE, 20, SAMPLING_RATE/2, 120, WINDOW_SIZE);
			
			double[] audiodata_inverse_stft = stft.InverseStft(stftdata);
			
			// divide
			//MathUtils.Divide(ref audiodata_inverse_stft, AUDIO_MULTIPLIER);
			MathUtils.Normalize(ref audiodata_inverse_stft);

			//Export.WriteAscii(audiodata_inverse_stft, outputFilePath + "_audiodata_inverse_stft.ascii");
			//Export.WriteF3Formatted(audiodata_inverse_stft, outputFilePath + "_audiodata_inverse_stft.txt");
			Export.DrawGraph(audiodata_inverse_stft, outputDirectoryFilePath + "_audiodata_inverse_stft.png");
			
			float[] audiodata_inverse_float = MathUtils.DoubleToFloat(audiodata_inverse_stft);
			BassProxy.SaveFile(audiodata_inverse_float, outputDirectoryFilePath + "_inverse_stft.wav", 1, SAMPLING_RATE, 32);
			
			Assert.Pass("This test was succesful.");
		}
		
		[Test]
		public void TestFFTMethods() {
			FFTTesting.TestAll();
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
			
			double minFrequency = 27.5;
			double maxFrequency = SAMPLING_RATE / 2;
			int logBins = 512;
			var logFrequenciesIndex = new int[1];
			var logFrequencies = new float[1];

			// find the time
			int numberOfSamples = audioSamples.Length;
			double seconds = numberOfSamples / SAMPLING_RATE;

			float[][] spectrogram = AudioAnalyzer.CreateSpectrogramLomont(audioSamples, WINDOW_SIZE, OVERLAP);
			
			int width = spectrogram.Length + 2*60;
			int height = spectrogram[0].Length + 2*40;
			Bitmap bmp1 = AudioAnalyzer.GetSpectrogramImage(spectrogram, width, height, seconds*1000, SAMPLING_RATE, ColorUtils.ColorPaletteType.BLACK_AND_WHITE, false, null, null);
			bmp1.Save(@"spectrogram-blackwhite.png");
			
		}
	}
}