using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;

using CommonUtils;
using CommonUtils.Audio;

namespace CommonUtils.CommonMath.FFT
{
	/// <summary>
	/// Most of the Audio2Midi class originally comes from the spectrotune processing.org project
	/// Performs pitch detection on a polyphonic audio source and outputs to MIDI
	/// https://github.com/corbanbrook/spectrotune
	/// </summary>
	public class Audio2Midi : IDSPPlugin
	{
		
		// const int bufferSize = 32768;
		// const int bufferSize = 16384;
		// const int bufferSize = 8192;
		// const int bufferSize = 4096;
		const int bufferSize = 2048;
		// const int bufferSize = 1024;
		// const int bufferSize = 512;

		// since we are dealing with small buffer sizes (1024) but are trying to detect peaks at low frequency ranges
		// octaves 0 .. 2 for example, zero padding is nessessary to improve the interpolation resolution of the FFT
		// otherwise FFT bins will be quite large making it impossible to distinguish between low octave notes which
		// are seperated by only a few Hz in these ranges.
		
		const int ZERO_PAD_MULTIPLIER = 4; // zero padding adds interpolation resolution to the FFT, it also dilutes the magnitude of the bins

		const int fftBufferSize = bufferSize * ZERO_PAD_MULTIPLIER;
		const int fftSize = fftBufferSize/2;

		double[] bufferPadded = new double[fftBufferSize];
		double[] spectrum = new double[fftSize];
		int[] peak = new int[fftSize];
		double[][] spectrogram;

		const int PEAK_THRESHOLD = 20; // default peak threshold (default = 50)

		// MIDI notes span from 0 - 128, octaves -1 -> 9. Specify start and end for piano
		const int keyboardStart = 12; // 12 is octave C0
		const int keyboardEnd = 108;

		BassProxy audioSystem = BassProxy.Instance;
		bool TRACK_LOADED = false;
		string loadedAudioFile;
		double sampleRate;
		double audioLength;
		int audioChannels;
		int audioPosition;
		
		Window window = new Window();

		int frames; // total horizontal audio frames
		int frameNumber = -1; // current audio frame

		int cuePosition; // cue position in miliseconds

		double[][] pcp; // pitch class profile

		List<Note>[] notes;

		Note[] notesOpen = new Note[128];

		int[] fftBinStart = new int[8];
		int[] fftBinEnd = new int[8];

		const double linearEQIntercept = 1f; // default no eq boost
		const double linearEQSlope = 0f; // default no slope boost

		bool[] OCTAVE_TOGGLE = {true, true, true, true, true, true, true, true};
		//bool[] OCTAVE_TOGGLE = {false, true, true, true, true, true, true, true};
		int[] OCTAVE_CHANNEL = {0,0,0,0,0,0,0,0}; // set all octaves to channel 0 (0-indexed channel 1)

		// Toggles and their defaults
		bool LINEAR_EQ_TOGGLE = false;
		bool PCP_TOGGLE = true;
		bool HARMONICS_TOGGLE = true;
		bool MIDI_TOGGLE = false; // true

		// smoothing
		const int PEAK = 1;
		const int HARMONIC = 3;

		// Types of FFT bin weighting algorithms
		const int UNIFORM = 0;
		const int DISCRETE = 1;
		const int LINEAR = 2;
		const int QUADRATIC = 3;
		const int EXPONENTIAL = 4;

		int WEIGHT_TYPE = UNIFORM; // default

		// UI Images
		Image bg;
		Image whiteKey;
		Image blackKey;
		Image octaveBtn;
		
		// Font
		Font textFont = new Font("Arial", 10.0f, FontStyle.Regular);
		
		// render size
		int TOTAL_WIDTH;
		int TOTAL_HEIGHT;
		int LEFT_MARGIN;

		#region Properties
		public int FrameNumber {
			get {
				return frameNumber;
			}
			set {
				frameNumber = value;
			}
		}
		#endregion

		// constructor
		public Audio2Midi() {

			TOTAL_WIDTH = 510;
			TOTAL_HEIGHT = 288;
			LEFT_MARGIN = 24;
			
			audioSystem.PropertyChanged += audioSystem_PropertyChanged;
			
			// Create MIDI output interface - select the first found device by default
			//midiOut = RWMidi.getOutputDevices()[0].createOutput();
			
			// UI Images
			bg = Image.FromFile(@"data\background.png");
			whiteKey = Image.FromFile(@"data\whitekey.png");
			blackKey = Image.FromFile(@"data\blackkey.png");
			octaveBtn = Image.FromFile(@"data\octavebutton.png");
			
			window.SetMode(0); // No Window
		}
		
		#region Freq to Pitch or Pitch to Freq
		public static int FreqToPitch(double f)
		{
			int p = MathUtils.RoundAwayFromZero(69.0f + 12.0f *(Math.Log(f/440.0f) / Math.Log(2.0f)));
			if (p > 0 && p < 128)
			{
				return p;
			}
			else
			{
				return 0;
			}
		}

		public static double PitchToFreq(int p)
		{
			return 440.0f * Math.Pow(2, (p - 69) / 12.0f);
		}
		#endregion

		#region Lowest and Highest Frequencies
		// Find the lowest frequency in an octave range
		public static double OctaveLowRange(int octave)
		{
			// find C - C0 is MIDI note 12
			return PitchToFreq(12 + octave * 12);
		}

		// Find the highest frequency in an octave range
		public static double OctaveHighRange(int octave)
		{
			// find B - B0 is MIDI note 23
			return PitchToFreq(23 + octave * 12);
		}
		#endregion

		#region FFT bin weighting
		// Applies FFT bin weighting. x is the distance from a real semi-tone
		public static double BinWeight(int type, double x)
		{
			switch(type)
			{
				case DISCRETE:
					return (x <= 0.2f) ? 1.0f : 0.0f;
				case LINEAR:
					return 1 - x;
				case QUADRATIC:
					return 1.0f - Math.Pow(x, 2);
				case EXPONENTIAL:
					return Math.Pow(Math.Exp(1.0f), 1.0f - x)/Math.Exp(1.0f);
				case UNIFORM:
				default:
					return 1.0f;
			}
		}
		#endregion

		// Normalize the pitch class profile
		public void NormalizePCP()
		{
			double pcpMax = MathUtils.Max(pcp[frameNumber]);
			for ( int k = 0; k < 12; k++ ) {
				pcp[frameNumber][k] /= pcpMax;
			}
		}

		public void ZeroPadBuffer()
		{
			for (int i = 0; i < fftBufferSize; i++)
			{
				bufferPadded[i] = 0f;
			}
		}

		public void PrecomputeOctaveRegions()
		{
			for (int j = 0; j < 8; j++)
			{
				fftBinStart[j] = 0;
				fftBinEnd[j] = 0;
				for (int k = 0; k < fftSize; k++)
				{
					double freq = k / (double)fftBufferSize * sampleRate;
					if (freq >= OctaveLowRange(j) && fftBinStart[j] == 0)
					{
						fftBinStart[j] = k;
					}
					else if (freq > OctaveHighRange(j) && fftBinEnd[j] == 0)
					{
						fftBinEnd[j] = k;
						break;
					}
				}
			}
			Console.WriteLine("Start: " + fftBinStart[0] + " End: " + fftBinEnd[7] + " (" + fftSize + " total)");
		}
		
		public int OpenAudioFile(string audioFile)
		{
			if (TRACK_LOADED)
			{
				audioSystem.Stop();

				loadedAudioFile = "";

				//sliderProgress.Value = 0;
				//sliderProgress.setMax(0);

				TRACK_LOADED = false;
			}

			//audio = minim.loadFile(sketchPath + "/music/" + audioFile, bufferSize);
			audioSystem.DspPlugin = this;
			audioSystem.OpenFileUsingFileStream(audioFile);
			audioLength = audioSystem.ChannelLength * 1000;
			sampleRate = audioSystem.SampleRate;
			audioChannels = audioSystem.Channels;
			
			frames = MathUtils.RoundAwayFromZero(audioLength / 1000f * (double)sampleRate / (double)bufferSize);

			Console.WriteLine();
			Console.WriteLine("Audio source: {0}", audioFile);
			Console.WriteLine("{0:N2} seconds ({1} frames)", audioLength / 1000, frames);
			Console.WriteLine("Time size: {0} Bytes, Samplerate: {1:N2} kHz", bufferSize, sampleRate / 1000f);
			Console.WriteLine("FFT bandwidth: {0:N2} Hz", (2.0f / fftBufferSize) * ((double)sampleRate / 2.0f));

			if (audioChannels == 2)
			{
				Console.WriteLine("Channels: 2 (STEREO)\n");
			}
			else
			{
				Console.WriteLine("Channels: 1 (MONO)\n");
			}

			// Setup Arrays
			notes = new List<Note>[frames];
			for (int i = 0; i < frames; i++) {
				notes[i] = new List<Note>();
			}
			
			pcp = new double[frames][];
			for (int i = 0; i < frames; i++) {
				pcp[i] = new double[12];
			}
			
			spectrogram = new double[frames][];
			for (int i = 0; i < frames; i++) {
				spectrogram[i] = new double[fftSize];
			}
			
			PrecomputeOctaveRegions();

			//sliderProgress.setMax(audioLength);
			cuePosition = audioPosition;

			frameNumber = -1;

			loadedAudioFile = audioFile;
			TRACK_LOADED = true;
			
			audioSystem.Play();
			
			while (audioSystem.IsPlaying) {
				System.Threading.Thread.Sleep(100);
			}
			
			return frameNumber;
		}
		
		#region Midi
		public void OutputMIDINotes()
		{
			if (MIDI_TOGGLE)
			{
				// send NoteOns
				foreach (var note in notes[frameNumber]) {
					if (OCTAVE_TOGGLE[note.octave] && notesOpen[note.pitch] == null) {
						//midiOut.sendNoteOn(note.channel, note.pitch, note.velocity);
						notesOpen[note.pitch] = note;
					}
				}

				// send NoteOffs
				for (int i = 0; i < notesOpen.Length; i++)
				{
					bool isOpen = false;
					if (notesOpen[i] != null)
					{
						for (int j = 0; j < notes[frameNumber].Count; j++)
						{
							if (notes[frameNumber][j].pitch == i)
							{
								isOpen = true;
							}
						}
						if (!isOpen)
						{
							//midiOut.sendNoteOff(notesOpen[i].channel, i, notesOpen[i].velocity);
							notesOpen[i] = null;
						}
					}
				}
			}
		}

		public void CloseMIDINotes()
		{
			for (int i = 0; i < notesOpen.Length; i++)
			{
				if (notesOpen[i] != null)
				{
					//midiOut.sendNoteOff(notesOpen[i].channel, i, notesOpen[i].velocity);
					notesOpen[i] = null;
				}
			}
		}
		#endregion
		
		public bool IsLoaded() {
			if ( TRACK_LOADED && frameNumber > -1 ) {
				return true;
			} else {
				return false;
			}
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
		
		private void ProcessWaveform(float[] waveform) {
			// stereo waveform
			float[] monoSignal = BassProxy.GetMonoSignal(waveform, audioChannels, BassProxy.MonoSummingType.Mix);
			//float[] monoSignal = ReadTestSignal();
			
			// divide it into chunks of bufferSize
			var chunks = monoSignal.Split(bufferSize);
			//var chunks = MathUtils.Split(monoSignal, bufferSize);
			
			int chunkLength = chunks.Count();
			Console.WriteLine("Chunk count: {0}", chunkLength);
			
			int count = 1;
			foreach (var chunk in chunks) {
				//Console.Write("Processing chunk: {0}      \r", count);
				Process(chunk.ToArray());
				count++;
			}
			
			Render(RenderType.Windowing).Save("windowing.png");
			for (int i = 0; i < frames; i++) {
				FrameNumber = i;
				Render(RenderType.FFT).Save("fft_" + i + ".png");
				Render(RenderType.MidiPeaks).Save("peaks_" + i + ".png");
			}
		}
		
		public void Process(float[] buffer)
		{
			if (frameNumber < frames - 1)
			{
				// need to apply the window transform before we zeropad
				window.Transform(buffer); // add window to samples

				Array.Copy(buffer, 0, bufferPadded, 0, buffer.Length);

				//if (audioSystem.IsPlaying)
				if (true)
				{
					frameNumber++;
					Console.Write("Processing frame: {0}      \n", frameNumber);
					Analyze();
					OutputMIDINotes();
				}
			}
			else
			{
				audioSystem.Pause();
				CloseMIDINotes();
			}
		}

		public void Analyze()
		{
			//fft = new FFT(fftBufferSize, audio.sampleRate());
			//fft.forward(bufferPadded); // run fft on the buffer

			int N = bufferPadded.Length;
			double[] din = bufferPadded;
			var dout = new double[N];
			
			// perform the FFT
			FFTUtils.FFTW_FFT_R2R(ref din, ref dout, N, FFTUtils.FFTMethod.DFT);

			// get the result
			double[] complexDout = FFTUtils.HC2C(dout);
			var spectrum_fft_abs = FFTUtils.Abs(complexDout);
			
			//Export.ExportCSV("audio_buffer_padded.csv", din);
			//Export.ExportCSV("spectrum_fft_abs.csv", spectrum_fft_abs, fftSize);
			
			var binDistance = new double[fftSize];
			var freq = new double[fftSize];

			double freqLowRange = OctaveLowRange(0);
			double freqHighRange = OctaveHighRange(7);

			for (int k = 0; k < fftSize; k++)
			{
				freq[k] = k / (double)fftBufferSize * sampleRate;

				//Console.Write("Processing frame: {0}, {1:N2} Hz\r", frameNumber, freq[k]);
				
				// skip FFT bins that lay outside of octaves 0-9
				if (freq[k] < freqLowRange || freq[k] > freqHighRange)
				{
					continue;
				}

				// Calculate fft bin distance and apply weighting to spectrum
				double closestFreq = PitchToFreq(FreqToPitch(freq[k])); // Rounds FFT frequency to closest semitone frequency

				// Filter out frequncies from disabled octaves
				bool filterOutFreq = false;
				for (int i = 0; i < 8; i ++)
				{
					if (!OCTAVE_TOGGLE[i])
					{
						if (closestFreq >= OctaveLowRange(i) && closestFreq <= OctaveHighRange(i))
						{
							filterOutFreq = true;
							break;
						}
					}
				}

				// Set spectrum
				if (!filterOutFreq)
				{
					binDistance[k] = 2 * Math.Abs((12 * Math.Log(freq[k]/440.0f) / Math.Log(2)) - (12 * Math.Log(closestFreq/440.0f) / Math.Log(2)));

					spectrum[k] = spectrum_fft_abs[k] * BinWeight(WEIGHT_TYPE, binDistance[k]);

					if (LINEAR_EQ_TOGGLE)
					{
						spectrum[k] *= (linearEQIntercept + k * linearEQSlope);
					}

					// Sum PCP bins
					pcp[frameNumber][FreqToPitch(freq[k]) % 12] += Math.Pow(spectrum_fft_abs[k], 2) * BinWeight(WEIGHT_TYPE, binDistance[k]);
				}
			}

			NormalizePCP();

			if (PCP_TOGGLE)
			{
				for (int k = 0; k < fftSize; k++)
				{
					if (freq[k] < freqLowRange || freq[k] > freqHighRange)
					{
						continue;
					}

					spectrum[k] *= pcp[frameNumber][FreqToPitch(freq[k]) % 12];
				}
			}

			double sprev = 0;
			double scurr = 0;
			double snext = 0;

			var foundPeak = new List<double>();
			var foundLevel = new List<double>();
			
			// find the peaks and valleys
			for (int k = 1; k < fftSize -1; k++)
			{
				if (freq[k] < freqLowRange || freq[k] > freqHighRange)
				{
					continue;
				}

				sprev = spectrum[k-1];
				scurr = spectrum[k];
				snext = spectrum[k+1];

				if (scurr > sprev && scurr > snext && (scurr > PEAK_THRESHOLD)) // peak
				{
					// Parobolic Peak Interpolation to estimate the real peak frequency and magnitude
					double ym1 = sprev;
					double y0 = scurr;
					double yp1 = snext;

					double p = (yp1 - ym1) / (2 * (2 * y0 - yp1 - ym1));
					double interpolatedAmplitude = y0 - 0.25f * (ym1 - yp1) * p;
					double a = 0.5f * (ym1 - 2 * y0 + yp1);

					double interpolatedFrequency = (k + p) * sampleRate / fftBufferSize;

					if (FreqToPitch(interpolatedFrequency) != FreqToPitch(freq[k]))
					{
						freq[k] = interpolatedFrequency;
						spectrum[k] = interpolatedAmplitude;
					}

					bool isHarmonic = false;

					// filter harmonics from peaks
					if (HARMONICS_TOGGLE)
					{
						for (int f = 0; f < foundPeak.Count; f++)
						{
							//TODO: Cant remember why this is here
							/*
							if (foundPeak.Count > 2)
							{
								isHarmonic = true;
								break;
							}
							 */
							// If the current frequencies note has already peaked in a lower octave check to see if its level is lower probably a harmonic
							if (FreqToPitch(freq[k]) % 12 == FreqToPitch(foundPeak[f]) % 12 && spectrum[k] < foundLevel[f])
							{
								isHarmonic = true;
								break;
							}
						}
					}

					if (isHarmonic)
					{
						peak[k] = HARMONIC;
					}
					else
					{
						peak[k] = PEAK;
						
						notes[frameNumber].Add(new Note(freq[k], spectrum[k]));

						// Track Peaks and Levels in this pass so we can detect harmonics
						foundPeak.Add(freq[k]);
						foundLevel.Add(spectrum[k]);
					}
				}
			}
			
			// add spectrum to spectrogram
			Array.Copy(spectrum, spectrogram[frameNumber], spectrum.Length);
		}

		#region IDSPPlugin implementation
		public void Process(ref float[] buffer)
		{
			//Process(buffer);
		}

		#endregion
		
		#region render methods
		public enum RenderType {
			Windowing,
			FFT,
			MidiPeaks
		}
		
		public Bitmap Render(RenderType type)
		{
			var bitmap = new Bitmap( TOTAL_WIDTH, TOTAL_HEIGHT, PixelFormat.Format32bppArgb );
			using(Graphics g = Graphics.FromImage(bitmap))
			{
				g.DrawImage(bg, 0, 0); // Render the background image

				// Render octave toggle buttons for active octaves
				for (int i = 0; i < 8; i++)
				{
					if (OCTAVE_TOGGLE[i])
					{
						g.DrawImage(octaveBtn, 0, bitmap.Height - (i * 36) - 36);
					}
				}

				if (type == RenderType.Windowing)
				{
					RenderWindowCurve(bitmap);
				}
				else if (type == RenderType.FFT)
				{
					RenderFFTSpectrogram(bitmap);
				}
				else if (type == RenderType.MidiPeaks)
				{
					RenderPeaks(bitmap);
				}
				else
				{
					RenderPeaks(bitmap);
				}

				// Update progress bar
				if (IsLoaded())
				{
					//if (audio.isPlaying())
					if (false)
					{
						double percentComplete = audioPosition / (double)audioLength * 100;
						//sliderProgress.Value = audioPosition;
						//sliderProgress.setValueLabel(nf(round(percentComplete), 2) + "%");
					}
				}
				else
				{
					//sliderProgress.setValueLabel("NO FILE LOADED");
				}
			}
			return bitmap;
		}

		public void RenderPeaks(Bitmap bitmap)
		{
			using(Graphics g = Graphics.FromImage(bitmap))
			{
				int keyHeight = bitmap.Height / (keyboardEnd - keyboardStart);

				if (IsLoaded())
				{
					// render key presses for detected peaks
					foreach (var note in notes[frameNumber]) {
						if (note.isWhiteKey()) {
							g.DrawImage(whiteKey, 10, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight + keyHeight));
						} else if (note.isBlackKey()) {
							g.DrawImage(blackKey, 10, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight + keyHeight));
						}
					}

					// render detected peaks
					const int keyLength = 15;
					int scroll = (frameNumber * keyLength > bitmap.Width) ? frameNumber - bitmap.Width/keyLength: 0;

					for (int x = frameNumber; x >= scroll; x--)
					{
						foreach (var note in notes[x]) {
							Color noteColor;
							if (pcp[x][note.pitch % 12] == 1.0f) {
								noteColor = Color.FromArgb(255, (int) (100 * note.amplitude / 400), 0);
							} else {
								noteColor = Color.FromArgb(0, (int) (255 * note.amplitude / 400), 200); // blue
							}
							/*
							fill(red(noteColor) / 4, green(noteColor) / 4, blue(noteColor) / 4);
							rect(abs(x - frameNumber) * keyLength + LEFT_MARGIN, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight), Math.Abs(x - frameNumber) * keyLength + keyLength + 25, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight + keyHeight));
							
							fill(noteColor);
							rect(abs(x - frameNumber) * keyLength + LEFT_MARGIN, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight) - 1, Math.Abs(x - frameNumber) * keyLength + keyLength + LEFT_MARGIN, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight + keyHeight));
							 */
							//var rect = new Rectangle(Math.Abs(x - frameNumber) * keyLength + LEFT_MARGIN, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight) - 1, Math.Abs(x - frameNumber) * keyLength + keyLength + LEFT_MARGIN, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight + keyHeight));
							var rect = new Rectangle(Math.Abs(x - frameNumber) * keyLength + LEFT_MARGIN, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight) - 3, keyLength + LEFT_MARGIN, keyHeight);
							g.FillRectangle(new SolidBrush(noteColor), rect);
						}
					}

					// output semitone text labels
					foreach (var note in notes[frameNumber]) {
						//fill(20);
						//text(note.label(), LEFT_MARGIN + 1, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight + keyHeight + 1));
						//fill(140);
						//text(note.label(), LEFT_MARGIN, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight + keyHeight + 2));
						Color gray = Color.FromArgb(140, 140, 140);
						g.DrawString(note.label(), textFont, new SolidBrush(gray), LEFT_MARGIN, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight + keyHeight + 6));
					}
				}
			}
		}

		public void RenderWindowCurve(Bitmap bitmap)
		{
			const int windowX = 50;
			const int windowY = 160;
			const int windowHeight = 80;

			float[] windowCurve = window.DrawCurve();

			using(Graphics g = Graphics.FromImage(bitmap))
			{
				for (int i = 0; i < windowCurve.Length - 1; i++)
				{
					//line(i + windowX, windowY - windowCurve[i] * windowHeight, i+1 + windowX, windowY - windowCurve[i+1] * windowHeight);
					g.DrawLine(Pens.White, i + windowX, (int) (windowY - windowCurve[i] * windowHeight), i+1 + windowX, (int) (windowY - windowCurve[i+1] * windowHeight));
				}
			}
		}

		public void RenderFFT(Bitmap bitmap)
		{
			using(Graphics g = Graphics.FromImage(bitmap))
			{
				int keyHeight = bitmap.Height / (keyboardEnd - keyboardStart);
				Color noteColor = Color.FromArgb(0, 255, 240);
				var amp = new double[128];

				int previousPitch = -1;
				int currentPitch;

				if (IsLoaded())
				{
					for (int k = 0; k < spectrum.Length; k++)
					{
						double freq = k / (double)fftBufferSize * sampleRate;

						currentPitch = FreqToPitch(freq);

						if (currentPitch == previousPitch)
						{
							amp[currentPitch] = amp[currentPitch] > spectrum[k] ? amp[currentPitch] : spectrum[k];
						}
						else
						{
							amp[currentPitch] = spectrum[k];
							previousPitch = currentPitch;
						}
					}

					for (int i = keyboardStart; i < keyboardEnd; i++)
					{
						//fill(red(noteColor)/4, green(noteColor)/4, blue(noteColor)/4);
						//rect(LEFT_MARGIN, height - ((i - keyboardStart) * keyHeight), 25 + amp[i], height - ((i - keyboardStart) * keyHeight + keyHeight)); // shadow

						//fill(noteColor);
						//rect(LEFT_MARGIN, height - ((i - keyboardStart) * keyHeight) - 1, LEFT_MARGIN + amp[i], height - ((i - keyboardStart) * keyHeight + keyHeight));
						//var rect = new Rectangle(LEFT_MARGIN, bitmap.Height - ((i - keyboardStart) * keyHeight) - 1, LEFT_MARGIN + (int) amp[i], bitmap.Height - ((i - keyboardStart) * keyHeight + keyHeight));
						var rect = new Rectangle(LEFT_MARGIN, bitmap.Height - ((i - keyboardStart) * keyHeight) - 2, LEFT_MARGIN + (int) amp[i], keyHeight - 1);
						g.FillRectangle(new SolidBrush(noteColor), rect);
					}
				}
				
				//labelThreshold.setPosition(PEAK_THRESHOLD + 26, 60);
				//line(PEAK_THRESHOLD + LEFT_MARGIN, 0, PEAK_THRESHOLD + LEFT_MARGIN, height);
				g.DrawLine(Pens.Black, PEAK_THRESHOLD + LEFT_MARGIN, 0, PEAK_THRESHOLD + LEFT_MARGIN, bitmap.Height);
			}
		}
		
		public void RenderFFTSpectrogram(Bitmap bitmap)
		{
			using(Graphics g = Graphics.FromImage(bitmap))
			{
				int keyHeight = bitmap.Height / (keyboardEnd - keyboardStart);
				Color noteColor = Color.FromArgb(0, 255, 240);
				var amp = new double[128];

				int previousPitch = -1;
				int currentPitch;

				if (IsLoaded())
				{
					for (int k = 0; k < spectrogram[frameNumber].Length; k++)
					{
						double freq = k / (double)fftBufferSize * sampleRate;

						currentPitch = FreqToPitch(freq);

						if (currentPitch == previousPitch)
						{
							amp[currentPitch] = amp[currentPitch] > spectrogram[frameNumber][k] ? amp[currentPitch] : spectrogram[frameNumber][k];
						}
						else
						{
							amp[currentPitch] = spectrogram[frameNumber][k];
							previousPitch = currentPitch;
						}
					}

					for (int i = keyboardStart; i < keyboardEnd; i++)
					{
						//fill(red(noteColor)/4, green(noteColor)/4, blue(noteColor)/4);
						//rect(LEFT_MARGIN, height - ((i - keyboardStart) * keyHeight), 25 + amp[i], height - ((i - keyboardStart) * keyHeight + keyHeight)); // shadow

						//fill(noteColor);
						//rect(LEFT_MARGIN, height - ((i - keyboardStart) * keyHeight) - 1, LEFT_MARGIN + amp[i], height - ((i - keyboardStart) * keyHeight + keyHeight));
						//var rect = new Rectangle(LEFT_MARGIN, bitmap.Height - ((i - keyboardStart) * keyHeight) - 1, LEFT_MARGIN + (int) amp[i], bitmap.Height - ((i - keyboardStart) * keyHeight + keyHeight));
						var rect = new Rectangle(LEFT_MARGIN, bitmap.Height - ((i - keyboardStart) * keyHeight) - 2, LEFT_MARGIN + (int) amp[i], keyHeight - 1);
						g.FillRectangle(new SolidBrush(noteColor), rect);
					}
				}
				
				//labelThreshold.setPosition(PEAK_THRESHOLD + 26, 60);
				//line(PEAK_THRESHOLD + LEFT_MARGIN, 0, PEAK_THRESHOLD + LEFT_MARGIN, height);
				g.DrawLine(Pens.Black, PEAK_THRESHOLD + LEFT_MARGIN, 0, PEAK_THRESHOLD + LEFT_MARGIN, bitmap.Height);
			}
		}
		#endregion
		
		#region Event Handlers
		private void audioSystem_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "WaveformData":
					ProcessWaveform(audioSystem.WaveformData);
					break;
			}
		}
		#endregion

		
		#region internal classes
		internal class Note
		{
			static readonly string[] semitones = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
			static readonly bool[] keyboard = { true, false, true, false, true, true, false, true, false, true, false, true };
			
			public double frequency;
			public double amplitude;

			public int octave;
			public int semitone;

			public int channel;
			public int pitch;
			public int velocity;

			public Note(double frequency, double amplitude)
			{
				this.frequency = frequency;
				this.amplitude = amplitude;
				this.pitch = FreqToPitch(frequency);
				this.octave = this.pitch / 12 - 1;
				this.semitone = this.pitch % 12;
				//this.channel = OCTAVE_CHANNEL[this.octave];
				this.velocity = MathUtils.RoundAwayFromZero((amplitude - PEAK_THRESHOLD) / (255f + PEAK_THRESHOLD) * 128f);

				if (this.velocity > 127)
				{
					this.velocity = 127;
				}
			}

			public string label()
			{
				return semitones[this.semitone] + this.octave;
			}

			public bool isWhiteKey()
			{
				return keyboard[this.pitch % 12];
			}

			public bool isBlackKey()
			{
				return !keyboard[this.pitch % 12];
			}
			
			public override string ToString()
			{
				return string.Format("[{7}] Freq.={0}, Amp.={1}, Oct.={2}, Semit.={3}, Ch.={4}, Pitch={5}, Vel.={6}", frequency, amplitude, octave, semitone, channel, pitch, velocity, label());
			}
		}
		
		internal class Window
		{
			public const int RECTANGULAR = 0; // equivilent to no window
			public const int HAMMING = 1;
			public const int HANN = 2;
			public const int COSINE = 3;
			public const int TRIANGULAR = 4;
			public const int BLACKMAN = 5;
			public const int GAUSS = 6;

			private int mode = RECTANGULAR;
			
			private const double PI = Math.PI;
			private const double TWO_PI = 2.0 * Math.PI;

			public void SetMode(int window)
			{
				this.mode = window;
			}

			public void Transform(float[] samples)
			{
				switch(mode)
				{
					case HAMMING:
						HammingWindow(samples);
						break;
					case HANN:
						HannWindow(samples);
						break;
					case COSINE:
						CosineWindow(samples);
						break;
					case TRIANGULAR:
						TriangularWindow(samples);
						break;
					case BLACKMAN:
						BlackmanWindow(samples);
						break;
					case GAUSS:
						GaussWindow(samples);
						break;
				}
			}

			public float[] DrawCurve()
			{
				var samples = new float[128];

				for(int i = 0; i < samples.Length; i++)
				{
					samples[i] = 1; // 1 out samples
				}

				switch(mode)
				{
					case HAMMING:
						HammingWindow(samples);
						break;
					case HANN:
						HannWindow(samples);
						break;
					case COSINE:
						CosineWindow(samples);
						break;
					case TRIANGULAR:
						TriangularWindow(samples);
						break;
					case BLACKMAN:
						BlackmanWindow(samples);
						break;
					case GAUSS:
						GaussWindow(samples);
						break;
				}
				return samples;
			}

			public static void HammingWindow(float[] samples)
			{
				for(int n = 0; n < samples.Length; n++)
				{
					samples[n] *= (float) (0.54f - 0.46f * Math.Cos(TWO_PI * n / (samples.Length - 1)));
				}
			}

			public static void HannWindow(float[] samples)
			{
				for(int n = 0; n < samples.Length; n++)
				{
					samples[n] *= (float) (0.5f * (1 - Math.Cos(TWO_PI * n / (samples.Length - 1))));
				}
			}

			public static void CosineWindow(float[] samples)
			{
				for(int n = 0; n < samples.Length; n++)
				{
					samples[n] *= (float) Math.Cos((Math.PI * n) / (samples.Length - 1) - (Math.PI / 2));
				}
			}

			public static void TriangularWindow(float[] samples)
			{
				for(int n = 0; n < samples.Length; n++)
				{
					samples[n] *= (float) ((2.0f / samples.Length) * ((samples.Length / 2.0f) - Math.Abs(n - (samples.Length - 1) / 2.0f)));
				}
			}

			public static void BlackmanWindow(float[] samples)
			{
				for(int n = 0; n < samples.Length; n++)
				{
					samples[n] *= (float) ((0.42f - 0.5f * Math.Cos(TWO_PI * n / (samples.Length - 1))) + (0.08f * Math.Cos(4 * PI * n / (samples.Length -1))));
				}
			}

			public static void GaussWindow(float[] samples)
			{
				for (int n = 0; n < samples.Length; n++)
				{
					samples[n] *= (float) Math.Pow(Math.E, -0.5f * Math.Pow((n - (samples.Length - 1) / 2) / (0.1f * (samples.Length - 1) / 2), 2));
				}
			}
		}
		#endregion

	}
}
