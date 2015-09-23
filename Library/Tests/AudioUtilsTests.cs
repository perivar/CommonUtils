using System;
using System.Drawing;
using NUnit.Framework;

using CommonUtils.MathLib.FFT;
using CommonUtils.Audio;

namespace CommonUtils.Tests
{
	[TestFixture]
	public class AudioUtilsTests
	{
		const string WAVE_INPUT_FILEPATH = @"Tests\test.wav";
		
		[Test]
		public void TestMethod()
		{
			const double silence = 0.0001;
			
			var wavDataSmall = new float[] { 0.0001f, -0.8f, 0.05f, -0.05f, 0.2f, 0.4f, 1.0f, 0.0001f };
			Bitmap png = AudioAnalyzer.DrawWaveformMono(wavDataSmall, new Size(1000, 600), 1, 1, 0, 44100);
			string fileName = String.Format("wave-small-dataset-{0}.png", 1);
			png.Save(fileName);

			// crop
			float[] wavDataSmallCropped = AudioUtils.CropAudioAtSilence(wavDataSmall, silence, false, 0);
			png = AudioAnalyzer.DrawWaveformMono(wavDataSmallCropped, new Size(1000, 600), 1, 1, 0, 44100);
			fileName = String.Format("wave-small-dataset-cropped{0}.png", 1);
			png.Save(fileName);

			// init audio system
			var audioSystem = BassProxy.Instance;
			
			float[] wavDataBig = BassProxy.ReadMonoFromFile(WAVE_INPUT_FILEPATH, 44100, 10, 0);
			png = AudioAnalyzer.DrawWaveformMono(wavDataBig, new Size(1000, 600), 2000, 1, 0, 44100);
			fileName = String.Format("wave-big-dataset-{0}.png", 1);
			png.Save(fileName);

			// crop
			float[] wavDataBigCropped = AudioUtils.CropAudioAtSilence(wavDataBig, silence, false, 0);
			png = AudioAnalyzer.DrawWaveformMono(wavDataBigCropped, new Size(1000, 600), 1, 1, 0, 44100);
			fileName = String.Format("wave-big-dataset-cropped{0}.png", 1);
			png.Save(fileName);
		}
	}
}
