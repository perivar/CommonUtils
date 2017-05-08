using System;
using NUnit.Framework;
using System.Linq;
using CommonUtils;
using CommonUtils.MathLib.FFT;
using CommonUtils.Audio;

namespace CommonUtils.Tests
{
	[TestFixture]
	public class Audio2MidiTests
	{
		const int bufferSize = Audio2Midi.bufferSize;
		const double sampleRate = 44100;
		const int audioChannels = 1;
		
		double audioLength;
		int frames; // total horizontal audio frames
		
		[Test]
		public void TestMethod()
		{
			const string inputFilepath = @"Tests\Passacaglia, Handel-Sine-86bmp.wav";
			
			// init audio system
			var audioSystem = BassProxy.Instance;
			float[] wavData = BassProxy.ReadMonoFromFile(inputFilepath, (int) sampleRate, 1000, 0);
		
			// calculate number of frames and the duration			
			frames = MathUtils.RoundAwayFromZero((double)wavData.Length / (double)bufferSize);
			audioLength = (double)wavData.Length / (double)sampleRate * 1000;
			
			var audio2Midi = new Audio2Midi();
			audio2Midi.IsTrackLoaded = true;
			audio2Midi.Initialize(sampleRate, audioChannels, audioLength, frames);
			ProcessWaveform(audio2Midi, wavData);
		}
		
		#region Test Methods
		public static object CsvDoubleParser(string[] splittedLine) {
			// only store the second element (the first is a counter)
			return float.Parse(splittedLine[1]);
		}
		
		private static float[] ReadTestSignal() {
			string filePath = @"C:\Users\perivar.nerseth\Documents\Processing\fft_testing\data\fft.csv";
			
			var objects = IOUtils.ReadCSV(filePath, true, CsvDoubleParser);
			var floats = objects.Cast<float>().ToArray();
			return floats;
		}
		#endregion
		
		private void ProcessWaveform(Audio2Midi audio2midi, float[] waveform) {
			// stereo waveform
			float[] monoSignal = BassProxy.GetMonoSignal(waveform, audioChannels, BassProxy.MonoSummingType.Mix);
			//float[] monoSignal = ReadTestSignal();
			
			// divide it into chunks of bufferSize
			var chunks = monoSignal.Split(bufferSize);
			
			int chunkLength = chunks.Count();
			Console.WriteLine("Chunk count: {0}", chunkLength);
			
			int count = 1;
			foreach (var chunk in chunks) {
				Console.Write("Processing chunk: {0}      \r", count);
				
				var chunkArray = chunk.ToArray();
				if (chunkArray.Length < bufferSize ) {
					// zero pad
					Array.Resize<float>(ref chunkArray, bufferSize);
				}
				audio2midi.Process(chunkArray);
				count++;
			}
			
			audio2midi.Render(Audio2Midi.RenderType.FFTWindow).Save("fft_window.png");
			
			for (int i = 0; i < frames - 1; i++) {
				audio2midi.FrameNumber = i;
				audio2midi.Render(Audio2Midi.RenderType.FFTSpectrum).Save("fft_spectrum_" + i + ".png");
				audio2midi.Render(Audio2Midi.RenderType.MidiPeaks).Save("midi_peaks_" + i + ".png");
			}
		}

	}
}
