using System;
using NUnit.Framework;
using System.Linq;
using CommonUtils;
using CommonUtils.MathLib.FFT;
using CommonUtils.Audio;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

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
		
		Audio2Midi audio2midi;
		
		//[Test]
		public void TestPianoRoll() {
			var audio2Midi = new Audio2Midi();

			var bitmap = new Bitmap(600, 785, PixelFormat.Format32bppArgb );
			audio2Midi.RenderPianoRoll(bitmap, 42, 784);
			bitmap.Save("piano_roll.png");
		}

		void Audio2MidiInitialise(string inputFilepath) {
			
			// init audio system
			var audioSystem = BassProxy.Instance;
			//float[] wavData = BassProxy.ReadMonoFromFile(inputFilepath, (int) sampleRate, 1000, 0);
			float[] wavData = BassProxy.ReadMonoFromFile(inputFilepath, (int) sampleRate);
			
			// calculate number of frames and the duration
			frames = MathUtils.RoundAwayFromZero((double)wavData.Length / (double)bufferSize);
			audioLength = (double)wavData.Length / (double)sampleRate * 1000;
			
			audio2midi = new Audio2Midi();
			audio2midi.IsTrackLoaded = true;
			audio2midi.Initialize(sampleRate, audioChannels, audioLength, frames);
			ProcessWaveform(audio2midi, wavData);
		}
		
		[Test]
		public void TestAudio2MidiRender()
		{
			Audio2MidiInitialise(@"Tests\Passacaglia, Handel-Sine-86bmp.wav");
			
			// render images
			audio2midi.Render(Audio2Midi.RenderType.FFTWindow).Save("fft_window.png");
			
			audio2midi.Render(Audio2Midi.RenderType.MidiSong).Save("midi_song.png");

			audio2midi.Render(Audio2Midi.RenderType.FFTSpectrogram).Save("fft_spectrogram.png");

			return;
			
			for (int i = 0; i < frames - 1; i++) {
				audio2midi.FrameNumber = i;
				audio2midi.Render(Audio2Midi.RenderType.FFTSpectrum).Save("fft_spectrum_" + i + ".png");
				audio2midi.Render(Audio2Midi.RenderType.MidiPeaks).Save("midi_peaks_" + i + ".png");
			}
		}
		
		[Test]
		public void TestAudio2MidiOutput()
		{
			Audio2MidiInitialise(@"Tests\Passacaglia, Handel-Sine-86bmp.wav");
			
			// get midi
			audio2midi.SaveMidiSequence("output.mid");
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
			
			// Start the stopwatch
			Stopwatch sw = Stopwatch.StartNew();

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
					// zero pad the last chunk
					Array.Resize<float>(ref chunkArray, bufferSize);
				}
				audio2midi.Process(chunkArray);
				count++;
			}

			sw.Stop();
			Console.Out.WriteLine("Audio2Midi Processed All: Time used: {0} ms",sw.Elapsed.TotalMilliseconds);
		}
	}
}
