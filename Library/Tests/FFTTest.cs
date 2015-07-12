using System;
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
		const int WINDOW_SIZE = 2048;
		const int SAMPLING_RATE = 44100; 	// Using 32000 (instead of 44100) gives us a max of 16 khz resolution, which is OK for normal adult human hearing
		const int OVERLAP = WINDOW_SIZE/2;
		
		[Test]
		public void TestFFTAudioMethod()
		{
			// test variables
			string inputFilePath = @"Tests\test.wav";
			string outputFilePath = "test";
			var audioSystem = BassProxy.Instance;

			// 0. Get Audio Data
			//float[] audioSamples = MathUtils.DoubleToFloat(FFTTesting.GetSignalTestData());
			float[] audioSamples = BassProxy.ReadMonoFromFile(inputFilePath);
			
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
			stftdata.DrawMatrixImageLogValues(outputFilePath + "_specgram.png", true);
			
			// spec gram with log values for the y axis (frequency)
			stftdata.DrawMatrixImageLogY(outputFilePath + "_specgramlog.png", SAMPLING_RATE, 20, SAMPLING_RATE/2, 120, WINDOW_SIZE);
			
			double[] audiodata_inverse_stft = stft.InverseStft(stftdata);
			
			// divide
			//MathUtils.Divide(ref audiodata_inverse_stft, AUDIO_MULTIPLIER);
			MathUtils.Normalize(ref audiodata_inverse_stft);

			//Export.WriteAscii(audiodata_inverse_stft, outputFilePath + "_audiodata_inverse_stft.ascii");
			//Export.WriteF3Formatted(audiodata_inverse_stft, outputFilePath + "_audiodata_inverse_stft.txt");
			Export.DrawGraph(audiodata_inverse_stft, outputFilePath + "_audiodata_inverse_stft.png");
			
			float[] audiodata_inverse_float = MathUtils.DoubleToFloat(audiodata_inverse_stft);
			BassProxy.SaveFile(audiodata_inverse_float, outputFilePath + "_inverse_stft.wav", 1, SAMPLING_RATE, 32);
			
			Assert.Pass("This test was succesful.");
		}
		
		[Test]
		public void TestFFTMethods() {
			FFTTesting.TestAll();
		}
	}
}
