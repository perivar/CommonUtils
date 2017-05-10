using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Extended; // Rounded Rectangles
using System.Drawing.Imaging;
using System.Diagnostics; // Debug

using CommonUtils;
using CommonUtils.Audio;

namespace CommonUtils.MathLib.FFT
{
	/// <summary>
	/// Most of the Audio2Midi class originally comes from the spectrotune processing.org project
	/// Performs pitch detection on a polyphonic audio source and outputs to MIDI
	/// https://github.com/corbanbrook/spectrotune
	/// </summary>
	public class Audio2Midi : IDSPPlugin
	{
		// using iZotope Spectrogram Demo.exe
		// which can be found at http://audio.rightmark.org/lukin/pub/aes_adapt/
		// the following seems like the best settings for:
		
		// Normal STFT Spectogram
		// FFTSize: 2048
		// FFT Zero Padding: 4 x Padding
		// FFT Overlap: 50%
		// Window: Hann

		// Multiresolution STFT Spectogram
		// FFTSize: 512 (or 1024)
		// FFT Zero Padding: None
		// FFT Overlap: None
		// Window: Hann
		
		// const int bufferSize = 32768;
		// const int bufferSize = 16384;
		// const int bufferSize = 8192;
		// const int bufferSize = 4096;
		public const int bufferSize = 2048;
		//const int bufferSize = 1024;
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
		Smoothing[] peak = new Smoothing[fftSize];
		double[][] spectrogram;

		const int PEAK_THRESHOLD = 50; // default peak threshold (default = 50)

		// MIDI notes span from 0 - 128, octaves -1 -> 9. Specify start and end for piano
		const int keyboardStart = 12; // 12 is octave C0
		const int keyboardEnd = 108;
		// 108 - 12 = 96 keys in total
		
		// fftBins span 8 octaves
		int[] fftBinStart = new int[8];
		int[] fftBinEnd = new int[8];

		BassProxy audioSystem = BassProxy.Instance;
		bool isTrackLoaded = false;

		string loadedAudioFilePath;
		double sampleRate;
		double audioLength; // length in milliseconds
		int audioChannels;
		int audioPosition; // position in milliseconds
		
		FFTWindow window;

		int frames; // total horizontal audio frames
		int frameNumber = -1; // current audio frame

		int cuePosition; // cue position in milliseconds

		double[][] pcp; // pitch class profile

		List<Note>[] notes;

		Note[] notesOpen = new Note[128];

		const double linearEQIntercept = 1f; // default no eq boost
		const double linearEQSlope = 0f; // default no slope boost

		// octave toggle determines if any octaves should be disabled
		bool[] OCTAVE_ACTIVE = {true, true, true, true, true, true, true, true};
		
		// octave channel determines what midi channel midi event within the octave should output to
		// set all octaves to channel 0 (0-indexed channel 1)
		int[] OCTAVE_OUTPUT_CHANNEL = {0,0,0,0,0,0,0,0};

		// Toggles and their defaults
		bool isLinearEQActive = false;
		bool isPCPActive = true;
		bool isHarmonicsActive = true;
		bool isMIDIActive = false;

		// default fft bin weighting is uniform
		FFTBinWeighting weightType = FFTBinWeighting.UNIFORM;

		// UI Images
		Image bg;
		Image whiteKey;
		Image blackKey;
		Image octaveBtn;
		
		// Font
		Font textFont = new Font("Arial", 8.0f, FontStyle.Regular);
		
		// render size
		int TOTAL_WIDTH;
		int TOTAL_HEIGHT;
		int LEFT_MARGIN;

		#region Enums
		// Smoothing
		public enum Smoothing {
			PEAK,
			HARMONIC
		}

		// Types of FFT bin weighting algorithms
		public enum FFTBinWeighting {
			UNIFORM,
			DISCRETE,
			LINEAR,
			QUADRATIC,
			EXPONENTIAL
		}
		
		public enum RenderType {
			FFTWindow,
			FFTSpectrum,
			MidiPeaks
		}
		#endregion
		
		#region Properties

		public bool IsLinearEQActive {
			get {
				return isLinearEQActive;
			}
			set {
				isLinearEQActive = value;
			}
		}

		public bool IsPCPActive {
			get {
				return isPCPActive;
			}
			set {
				isPCPActive = value;
			}
		}

		public bool IsHarmonicsActive {
			get {
				return isHarmonicsActive;
			}
			set {
				isHarmonicsActive = value;
			}
		}

		public bool IsMIDIActive {
			get {
				return isMIDIActive;
			}
			set {
				isMIDIActive = value;
			}
		}
		
		public int FrameNumber {
			get {
				return frameNumber;
			}
			set {
				frameNumber = value;
			}
		}

		public bool IsTrackLoaded {
			get {
				return isTrackLoaded;
			}
			set {
				isTrackLoaded = value;
			}
		}
		#endregion

		public bool IsLoaded() {
			if ( isTrackLoaded && frameNumber > -1 ) {
				return true;
			} else {
				return false;
			}
		}
		
		// constructor
		public Audio2Midi() {

			TOTAL_WIDTH = 1536;
			TOTAL_HEIGHT = 785; // 96 keys, 56 white keys
			LEFT_MARGIN = 41;
			
			// UI Images
			bg = Image.FromFile(@"data\background.png");
			whiteKey = Image.FromFile(@"data\whitekey.png");
			blackKey = Image.FromFile(@"data\blackkey.png");
			octaveBtn = Image.FromFile(@"data\octavebutton.png");
			
			window = new FFTWindow(FFTWindowType.RECTANGULAR, bufferSize);
		}
		
		#region Freq to Pitch or Pitch to Freq
		public static int FreqToPitch(double f) {
			
			int p = MathUtils.RoundAwayFromZero(69.0f + 12.0f *(Math.Log(f/440.0f) / Math.Log(2.0f)));
			if (p > 0 && p < 128) {
				return p;
			} else {
				return 0;
			}
		}

		public static double PitchToFreq(int p) {
			return 440.0f * Math.Pow(2, (p - 69) / 12.0f);
		}
		#endregion

		#region Lowest and Highest Frequencies
		// Find the lowest frequency in an octave range
		public static double OctaveLowRange(int octave) {
			// find C - C0 is MIDI note 12
			return PitchToFreq(12 + octave * 12);
		}

		// Find the highest frequency in an octave range
		public static double OctaveHighRange(int octave) {
			// find B - B0 is MIDI note 23
			return PitchToFreq(23 + octave * 12);
		}
		#endregion

		#region FFT bin weighting
		// Applies FFT bin weighting.
		// x is the distance from a real semi-tone
		public static double BinWeight(FFTBinWeighting weightType, double x) {
			
			switch(weightType) {
				case FFTBinWeighting.DISCRETE:
					return (x <= 0.2f) ? 1.0f : 0.0f;
				case FFTBinWeighting.LINEAR:
					return 1.0f - x;
				case FFTBinWeighting.QUADRATIC:
					return 1.0f - Math.Pow(x, 2);
				case FFTBinWeighting.EXPONENTIAL:
					return Math.Pow(Math.Exp(1.0f), 1.0f - x) / Math.Exp(1.0f);
				case FFTBinWeighting.UNIFORM:
				default:
					return 1.0f;
			}
		}
		#endregion

		// Normalize the pitch class profile
		void NormalizePCP() {
			double pcpMax = MathUtils.Max(pcp[frameNumber]);
			for ( int k = 0; k < 12; k++ ) {
				pcp[frameNumber][k] /= pcpMax;
			}
		}

		void PrecomputeOctaveRegions() {
			
			for (int j = 0; j < 8; j++) {
				fftBinStart[j] = 0;
				fftBinEnd[j] = 0;
				for (int k = 0; k < fftSize; k++) {
					double freq = k / (double)fftBufferSize * sampleRate;
					if (freq >= OctaveLowRange(j) && fftBinStart[j] == 0) {
						fftBinStart[j] = k;
					} else if (freq > OctaveHighRange(j) && fftBinEnd[j] == 0) {
						fftBinEnd[j] = k;
						break;
					}
				}
				
				#if DEBUG
				Debug.WriteLine("PrecomputeOctaveRegion: " + j + ". Start: " + fftBinStart[j] + " End: " + fftBinEnd[j] + " (" + fftSize + " total)");
				#endif
			}
		}
		
		public int OpenAudioFile(string audioFilePath) {
			if (isTrackLoaded) {
				audioSystem.Stop();

				loadedAudioFilePath = "";

				//sliderProgress.Value = 0;
				//sliderProgress.setMax(0);

				isTrackLoaded = false;
			}

			audioSystem.PropertyChanged += audioSystem_PropertyChanged;
			
			// Create MIDI output interface - select the first found device by default
			//midiOut = RWMidi.getOutputDevices()[0].createOutput();

			audioSystem.DspPlugin = this;
			audioSystem.OpenFileUsingFileStream(audioFilePath);
			
			#if DEBUG
			Debug.WriteLine("Audio source: {0}", audioFilePath);
			#endif
			
			// read values from the audio system
			double audioLength = audioSystem.ChannelLength * 1000;
			double sampleRate = audioSystem.SampleRate;
			int audioChannels = audioSystem.Channels;
			
			// calculate frames
			int frames = MathUtils.RoundAwayFromZero(audioLength / 1000f * (double)sampleRate / (double)bufferSize);

			// initialise
			Initialize(sampleRate, audioChannels, audioLength, frames);

			//sliderProgress.setMax(audioLength);
			cuePosition = audioPosition;

			frameNumber = -1;

			loadedAudioFilePath = audioFilePath;
			isTrackLoaded = true;
			
			/*
			audioSystem.Play();
			
			while (audioSystem.IsPlaying) {
				System.Threading.Thread.Sleep(100);
			}
			 */
			
			return frameNumber;
		}
		
		public bool Initialize(double sampleRate, int audioChannels, double audioLength, int frames) {
			
			this.sampleRate = sampleRate;
			this.audioChannels = audioChannels;
			this.audioLength = audioLength;
			this.frames = frames;
			
			#if DEBUG
			Debug.WriteLine("Duration: {0:N2} seconds ({1} frames)", audioLength / 1000, frames);
			Debug.WriteLine("Time size: {0} Bytes, Samplerate: {1:N2} kHz", bufferSize, sampleRate / 1000f);
			Debug.WriteLine("FFT bandwidth: {0:N2} Hz", (2.0f / fftBufferSize) * ((double)sampleRate / 2.0f));

			if (audioChannels == 2) {
				Debug.WriteLine("Channels: 2 (STEREO)\n");
			} else {
				Debug.WriteLine("Channels: 1 (MONO)\n");
			}
			#endif
			
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
			
			return true;
		}
		
		#region Midi
		public void OutputMIDINotes() {
			if (isMIDIActive) {
				// send NoteOns
				foreach (var note in notes[frameNumber]) {
					if (OCTAVE_ACTIVE[note.octave] && notesOpen[note.pitch] == null) {
						//midiOut.sendNoteOn(note.channel, note.pitch, note.velocity);
						notesOpen[note.pitch] = note;
					}
				}

				// send NoteOffs
				for (int i = 0; i < notesOpen.Length; i++) {
					bool isOpen = false;
					if (notesOpen[i] != null) {
						for (int j = 0; j < notes[frameNumber].Count; j++) {
							if (notes[frameNumber][j].pitch == i) {
								isOpen = true;
							}
						}
						if (!isOpen) {
							//midiOut.sendNoteOff(notesOpen[i].channel, i, notesOpen[i].velocity);
							notesOpen[i] = null;
						}
					}
				}
			}
		}

		public void CloseMIDINotes() {
			for (int i = 0; i < notesOpen.Length; i++) {
				if (notesOpen[i] != null) {
					//midiOut.sendNoteOff(notesOpen[i].channel, i, notesOpen[i].velocity);
					notesOpen[i] = null;
				}
			}
		}
		#endregion
		
		public void Process(float[] buffer) {
			if (frameNumber < frames - 1) {
				// need to apply the window transform before we zeropad
				window.Apply(buffer); // add window to samples

				Array.Copy(buffer, 0, bufferPadded, 0, buffer.Length);

				//if (audioSystem.IsPlaying)
				if (true) {
					frameNumber++;
					//Console.Write("Processing frame: {0}      \n", frameNumber);
					Analyze();
					OutputMIDINotes();
				}
			} else {
				audioSystem.Pause();
				CloseMIDINotes();
			}
		}

		public void Analyze() {
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

			for (int k = 0; k < fftSize; k++) {
				freq[k] = k / (double)fftBufferSize * sampleRate;

				//Console.Write("Processing frame: {0}, {1:N2} Hz\r", frameNumber, freq[k]);
				
				// skip FFT bins that lay outside of octaves 0-9
				if (freq[k] < freqLowRange || freq[k] > freqHighRange) {
					continue;
				}

				// Calculate fft bin distance and apply weighting to spectrum
				double closestFreq = PitchToFreq(FreqToPitch(freq[k])); // Rounds FFT frequency to closest semitone frequency

				// Filter out frequncies from disabled octaves
				bool filterOutFreq = false;
				for (int i = 0; i < 8; i ++) {
					if (!OCTAVE_ACTIVE[i]) {
						if (closestFreq >= OctaveLowRange(i) && closestFreq <= OctaveHighRange(i)) {
							filterOutFreq = true;
							break;
						}
					}
				}

				// Set spectrum
				if (!filterOutFreq) {
					binDistance[k] = 2 * Math.Abs((12 * Math.Log(freq[k]/440.0f) / Math.Log(2)) - (12 * Math.Log(closestFreq/440.0f) / Math.Log(2)));

					spectrum[k] = spectrum_fft_abs[k] * BinWeight(weightType, binDistance[k]);

					if (isLinearEQActive) {
						spectrum[k] *= (linearEQIntercept + k * linearEQSlope);
					}

					// Sum PCP bins
					pcp[frameNumber][FreqToPitch(freq[k]) % 12] += Math.Pow(spectrum_fft_abs[k], 2) * BinWeight(weightType, binDistance[k]);
				}
			}

			NormalizePCP();

			if (isPCPActive) {
				for (int k = 0; k < fftSize; k++) {
					if (freq[k] < freqLowRange || freq[k] > freqHighRange) {
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
			for (int k = 1; k < fftSize -1; k++) {
				if (freq[k] < freqLowRange || freq[k] > freqHighRange) {
					continue;
				}

				sprev = spectrum[k-1];
				scurr = spectrum[k];
				snext = spectrum[k+1];

				if (scurr > sprev && scurr > snext && (scurr > PEAK_THRESHOLD)) {
					// found peak
					// Parobolic Peak Interpolation to estimate the real peak frequency and magnitude
					double ym1 = sprev;
					double y0 = scurr;
					double yp1 = snext;

					double p = (yp1 - ym1) / (2 * (2 * y0 - yp1 - ym1));
					double interpolatedAmplitude = y0 - 0.25f * (ym1 - yp1) * p;
					double a = 0.5f * (ym1 - 2 * y0 + yp1);

					double interpolatedFrequency = (k + p) * sampleRate / fftBufferSize;

					if (FreqToPitch(interpolatedFrequency) != FreqToPitch(freq[k])) {
						freq[k] = interpolatedFrequency;
						spectrum[k] = interpolatedAmplitude;
					}

					bool isHarmonic = false;

					// filter harmonics from peaks
					if (isHarmonicsActive) {
						for (int f = 0; f < foundPeak.Count; f++) {
							
							//TODO: Cant remember why this is here
							/*
							if (foundPeak.Count > 2)
							{
								isHarmonic = true;
								break;
							}
							 */
							// If the current frequencies note has already peaked in a lower octave
							// check to see if its level is lower.
							// if so it's probably a harmonic
							if (FreqToPitch(freq[k]) % 12 == FreqToPitch(foundPeak[f]) % 12
							    && spectrum[k] < foundLevel[f]) {
								isHarmonic = true;
								break;
							}
						}
					}

					if (isHarmonic) {
						peak[k] = Smoothing.HARMONIC;
					} else {
						peak[k] = Smoothing.PEAK;
						
						notes[frameNumber].Add(new Note(this, freq[k], spectrum[k]));

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
			Process(buffer);
		}
		#endregion
		
		#region Render methods
		// draw directly in c# ?
		// https://gist.github.com/Folyd/e59d100900b8720bf17b
		// https://github.com/tranchis/project-blink/blob/master/src/main/java/midi/Piano.java
		// and
		// https://github.com/tranchis/project-blink/blob/master/src/main/java/midi/Key.java
		public Bitmap Render(RenderType type)
		{
			var bitmap = new Bitmap( TOTAL_WIDTH, TOTAL_HEIGHT, PixelFormat.Format32bppArgb );
			
			using(Graphics g = Graphics.FromImage(bitmap)) {
				//g.DrawImage(bg, 0, 0); // Render the background image
				RenderPianoRoll(bitmap, 40, TOTAL_HEIGHT-1);

				/*
				// Render octave toggle buttons for active octaves
				for (int i = 0; i < 8; i++) {
					if (OCTAVE_ACTIVE[i]) {
						g.DrawImage(octaveBtn, 0, bitmap.Height - (i * 36) - 36);
					}
				}
				 */

				if (type == RenderType.FFTWindow) {
					RenderFFTWindow(bitmap);
				} else if (type == RenderType.FFTSpectrum) {
					RenderFFTSpectrum(bitmap);
				} else if (type == RenderType.MidiPeaks) {
					RenderMidiPeaks(bitmap);
				} else {
					RenderMidiPeaks(bitmap);
				}

				// Update progress bar
				if (IsLoaded()) {
					//if (audio.isPlaying())
					if (false) {
						double percentComplete = audioPosition / (double)audioLength * 100;
						//sliderProgress.Value = audioPosition;
						//sliderProgress.setValueLabel(nf(round(percentComplete), 2) + "%");
					}
				} else {
					//sliderProgress.setValueLabel("NO FILE LOADED");
				}
			}
			return bitmap;
		}

		#region Draw Piano Roll
		const int TYPE_PIANO = 56; // 56 white keys = 8 octaves
		const int ROUND_RADIUS = 5;
		float whiteKeyHeight, whiteKeyWidth, blackKeyHeight, blackKeyWidth;

		float ComputeSpecificDividerY(int i) {
			return i * whiteKeyHeight;
		}
		
		public void RenderPianoRoll(Bitmap bitmap, int width, int height) {
			
			var keys = new Dictionary<int, PianoKey>();
			var whiteKeys = new List<PianoKey>();
			var blackKeys = new List<PianoKey>();
			
			int keyWidth = width;
			int keyHeight = 14; // 14 x 56 = 784 pixles
			int transpose = 12;
			int[] whiteIDs = { 0, 2, 4, 5, 7, 9, 11 };
			
			// 8 octaves x 7 white keys per octave = 56 white keys
			for (int i = 0, y = height-keyHeight; i < 8; i++) {
				// 7 white keys per octave
				for (int j = 0; j < 7; j++, y -= keyHeight) {
					int keyNum = i * 12 + whiteIDs[j] + transpose;
					whiteKeys.Add(new PianoKey(0, y, keyWidth, keyHeight, keyNum, false));
				}
			}
			// 8 octaves, add black keys
			int blackKeyShiftY = 3;
			for (int i = 0, y = height; i < 8; i++, y -= keyHeight) {
				int keyNum = i * 12 + transpose;
				blackKeys.Add(new PianoKey(0, (y -= keyHeight)-blackKeyShiftY, keyWidth/2, keyHeight/2, keyNum+1, true));
				blackKeys.Add(new PianoKey(0, (y -= keyHeight)-blackKeyShiftY, keyWidth/2, keyHeight/2, keyNum+3, true));
				y -= keyHeight;
				blackKeys.Add(new PianoKey(0, (y -= keyHeight)-blackKeyShiftY, keyWidth/2, keyHeight/2, keyNum+6, true));
				blackKeys.Add(new PianoKey(0, (y -= keyHeight)-blackKeyShiftY, keyWidth/2, keyHeight/2, keyNum+8, true));
				blackKeys.Add(new PianoKey(0, (y -= keyHeight)-blackKeyShiftY, keyWidth/2, keyHeight/2, keyNum+10, true));
			}
			
			// add keys to dictionary for later lookup by midi note
			foreach (var blackKey in blackKeys) {
				keys.Add(blackKey.MidiNote, blackKey);
			}
			foreach (var whiteKey in whiteKeys) {
				keys.Add(whiteKey.MidiNote, whiteKey);
			}
			
			using(Graphics g = Graphics.FromImage(bitmap)) {
				
				// clear
				g.Clear(Color.White);
				
				// draw all white keys first
				// then the black keys
				// to ensure the black keys overlay the white rectangles
				foreach (var key in whiteKeys) {
					g.DrawRectangle(Pens.DarkGray, key.Rectangle);
					g.DrawString(""+key.MidiNote, textFont, Brushes.Black, key.X+18, key.Y);

					//if (key.Octave % 12 == 0) {
						g.DrawString(""+key.Octave, textFont, Brushes.Black, key.X+41, key.Y);
					//}
				}

				foreach (var key in blackKeys) {
					g.FillRectangle(Brushes.Black, key.Rectangle);
				}
			}
		}
		
		public void RenderPianoRollOld(Bitmap bitmap, int width, int height) {
			
			int whiteKeyCount = TYPE_PIANO;
			
			whiteKeyWidth = width;
			whiteKeyHeight = MathUtils.RoundAwayFromZero((float) (height) / whiteKeyCount);

			float blackWhiteHeightRatio = 13f / 20f;
			blackKeyHeight = MathUtils.RoundAwayFromZero(whiteKeyHeight * blackWhiteHeightRatio);
			
			float blackWhiteWidthRatio = 90f / 150f;
			blackKeyWidth = whiteKeyWidth * blackWhiteWidthRatio;
			
			DrawForPianoKeyboard(bitmap, width, height);
		}
		
		void DrawForPianoKeyboard(Bitmap bitmap, int width, int height) {
			
			using(Graphics g = Graphics.FromImage(bitmap)) {
				g.Clear(Color.White);
			}
			
			int whiteCount = 0; // start with C key
			float baseY = 0;
			
			// 8 octaves
			for (int i = 0; i < 8; i++) {
				// 7 white keys
				float startY = height - (i * 7 * whiteKeyHeight);
				whiteCount = whiteCount + DrawOctave(bitmap, baseY + startY);
				
				using(Graphics g = Graphics.FromImage(bitmap)) {
					// draw octave label
					string octaveLabel = "C" + i;
					SizeF octaveLabelSize = g.MeasureString(octaveLabel, textFont);
					g.DrawString(octaveLabel, textFont, Brushes.Black, whiteKeyWidth-octaveLabelSize.Width, (baseY + startY) - octaveLabelSize.Height);
				}
			}
		}
		
		int DrawOctave(Bitmap bitmap, float baseY) {
			return DrawKeys(bitmap, baseY, 12);
		}

		int DrawKeys(Bitmap bitmap, float baseY, int count) {
			if (count <= 0) {
				return 0;
			}

			if (count > 12) {
				count = 12;
			}

			int whiteCount = 0;
			float x, y, width, height;
			for (int i = 0; i < count; i++) {
				if (i == 1 || i == 3 || i == 6 || i == 8 || i == 10) {
					// Black key
					width = blackKeyWidth;
					height = blackKeyHeight;
					float axis = ComputeSpecificDividerY(whiteCount);
					x = 0;
					y = baseY - (axis + blackKeyHeight / 2) - 1;

					DrawBlackKey(bitmap, x, y, width, height);
				} else {
					// White key
					width = whiteKeyWidth;
					height = whiteKeyHeight;
					x = 0; // don't draw border on left side of the white key, -1
					y = baseY - ComputeSpecificDividerY(whiteCount + 1) - 1;

					DrawWhiteKey(bitmap, x, y, width, height);
					whiteCount++;
				}
			}
			return whiteCount;
		}

		void DrawWhiteKey(Bitmap bitmap, float x, float y, float width, float height) {
			using(Graphics g = Graphics.FromImage(bitmap)) {
				var eg = new ExtendedGraphics(g);
				//Color darkGray = Color.FromArgb(52,52,52);
				//var pen = new Pen(darkGray, 1);
				var pen = new Pen(Color.Black, 1);
				pen.Alignment = PenAlignment.Inset; // draw border on inside

				var rect = new RectangleF(x, y, width, height);
				//bitmap.drawRoundRect(rect, ROUND_X, ROUND_Y, mPaint);
				g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
				//eg.DrawRoundRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height, ROUND_RADIUS);
			}
		}

		void DrawBlackKey(Bitmap bitmap, float x, float y, float width, float height) {
			using(Graphics g = Graphics.FromImage(bitmap)) {
				var eg = new ExtendedGraphics(g);
				var rect = new RectangleF(x, y, width, height);
				//bitmap.drawRoundRect(rect, ROUND_X, ROUND_Y, mPaint);
				g.FillRectangle(Brushes.Black, rect);
				//eg.FillRoundRectangle(Brushes.Black, rect.X, rect.Y, rect.Width, rect.Height, ROUND_RADIUS);
			}
		}
		#endregion
		
		public void RenderFFTWindow(Bitmap bitmap) {
			const int windowX = 50;
			const int windowY = 160;
			const int windowHeight = 80;

			double[] windowCurve = window.DrawCurve();

			using(Graphics g = Graphics.FromImage(bitmap)) {
				for (int i = 0; i < windowCurve.Length - 1; i++) {
					g.DrawLine(Pens.White, i + windowX, (int) (windowY - windowCurve[i] * windowHeight), i+1 + windowX, (int) (windowY - windowCurve[i+1] * windowHeight));
				}
			}
		}

		public void RenderMidiPeaks(Bitmap bitmap) {
			
			using(Graphics g = Graphics.FromImage(bitmap)) {
				int keyHeight = bitmap.Height / (keyboardEnd - keyboardStart);

				if (IsLoaded()) {
					// render key presses for detected peaks
					foreach (var note in notes[frameNumber]) {
						if (note.isWhiteKey()) {
							g.DrawImage(whiteKey, 10, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight + keyHeight));
						} else if (note.isBlackKey()) {
							g.DrawImage(blackKey, 10, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight + keyHeight));
						}
					}

					// render detected peaks
					const int keyLength = 10;
					int scroll = (frameNumber * keyLength > bitmap.Width) ? frameNumber - bitmap.Width/keyLength : 0;

					for (int x = frameNumber; x >= scroll; x--) {
						foreach (var note in notes[x]) {
							Color noteColor;
							int greenHue = 0;
							if (pcp[x][note.pitch % 12] == 1.0f) {
								greenHue = (int) (100 * note.amplitude / 400);
								if (greenHue < 0 || greenHue > 255) {
									greenHue = 100;
								}
								noteColor = Color.FromArgb(255, greenHue, 0);
							} else {
								greenHue = (int) (255 * note.amplitude / 400);
								if (greenHue < 0 || greenHue > 255) {
									greenHue = 255;
								}
								noteColor = Color.FromArgb(0, greenHue, 200); // blue
							}
							/*
							// draw twice to get a shadow effect
							fill(red(noteColor)/4, green(noteColor)/4, blue(noteColor)/4);
        					rect(abs(x - frameNumber) * keyLength + 24, height - ((note.pitch - keyboardStart) * keyHeight), abs(x - frameNumber) * keyLength + keyLength + 25 , height - ((note.pitch - keyboardStart) * keyHeight + keyHeight));
          
        					fill(noteColor);
        					rect(abs(x - frameNumber) * keyLength + 24, height - ((note.pitch - keyboardStart) * keyHeight) - 1, abs(x - frameNumber) * keyLength + keyLength + 24 , height - ((note.pitch - keyboardStart) * keyHeight + keyHeight));
							 */
							var rect = new Rectangle(Math.Abs(x - frameNumber) * keyLength + LEFT_MARGIN, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight), keyLength + LEFT_MARGIN, keyHeight);
							g.FillRectangle(new SolidBrush(noteColor), rect);
						}
					}

					// output semitone text labels
					foreach (var note in notes[frameNumber]) {
						//Color gray = Color.FromArgb(140, 140, 140);
						//g.DrawString(note.label(), textFont, new SolidBrush(gray), LEFT_MARGIN, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight + keyHeight + 6));
						g.DrawString(note.label(), textFont, Brushes.Black, LEFT_MARGIN, bitmap.Height - ((note.pitch - keyboardStart) * keyHeight + keyHeight + 3));
					}
				}
			}
		}
		
		public void RenderFFTSpectrum(Bitmap bitmap)
		{
			using(Graphics g = Graphics.FromImage(bitmap)) {
				int keyHeight = bitmap.Height / (keyboardEnd - keyboardStart);
				Color noteColor = Color.FromArgb(0, 255, 240);
				var amp = new double[128];

				int previousPitch = -1;
				int currentPitch;

				if (IsLoaded()) {
					for (int k = 0; k < spectrogram[frameNumber].Length; k++) {
						double freq = k / (double)fftBufferSize * sampleRate;

						currentPitch = FreqToPitch(freq);

						if (currentPitch == previousPitch) {
							amp[currentPitch] = amp[currentPitch] > spectrogram[frameNumber][k] ? amp[currentPitch] : spectrogram[frameNumber][k];
						} else {
							amp[currentPitch] = spectrogram[frameNumber][k];
							previousPitch = currentPitch;
						}
					}

					for (int i = keyboardStart; i < keyboardEnd + 1; i++) {
						/*
						// draw twice to get a shadow effect
						fill(red(noteColor)/4, green(noteColor)/4, blue(noteColor)/4);
						rect(24, height - ((i - keyboardStart) * keyHeight), 25 + amp[i], height - ((i - keyboardStart) * keyHeight + keyHeight)); // shadow
        
						fill(noteColor);
						rect(24, height - ((i - keyboardStart) * keyHeight) - 1, 24 + amp[i] , height - ((i - keyboardStart) * keyHeight + keyHeight));
						 */
						var rect = new Rectangle(LEFT_MARGIN, bitmap.Height - ((i - keyboardStart) * keyHeight) + 1, LEFT_MARGIN + (int) amp[i], keyHeight - 2);
						g.FillRectangle(new SolidBrush(noteColor), rect);
					}
				}
				
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
					//ProcessWaveform(audioSystem.WaveformData);
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

			public Note(Audio2Midi audio2midi, double frequency, double amplitude)
			{
				this.frequency = frequency;
				this.amplitude = amplitude;
				this.pitch = FreqToPitch(frequency);
				this.octave = this.pitch / 12 - 1;
				this.semitone = this.pitch % 12;
				this.channel = audio2midi.OCTAVE_OUTPUT_CHANNEL[this.octave];
				this.velocity = MathUtils.RoundAwayFromZero((amplitude - PEAK_THRESHOLD) / (255f + PEAK_THRESHOLD) * 128f);

				if (this.velocity > 127) {
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

		internal class PianoKey
		{
			Rectangle rect;
			int midiNote;
			bool isBlack;
			int octave;

			#region properties
			public Rectangle Rectangle {
				get {
					return rect;
				}
				set {
					rect = value;
				}
			}

			public int MidiNote {
				get {
					return midiNote;
				}
				set {
					midiNote = value;
				}
			}

			public bool IsBlack {
				get {
					return isBlack;
				}
				set {
					isBlack = value;
				}
			}

			public int Octave {
				get {
					return octave;
				}
				set {
					octave = value;
				}
			}
			#endregion

			#region getters
			public int X {
				get {
					return rect.X;
				}
			}

			public int Y {
				get {
					return rect.Y;
				}
			}

			public int Width {
				get {
					return rect.Width;
				}
			}

			public int Height {
				get {
					return rect.Height;
				}
			}
			#endregion
			
			public PianoKey(Rectangle rect, int midiNote, bool isBlack) {
				this.rect = rect;
				this.midiNote = midiNote;
				this.isBlack = isBlack;
				this.octave = this.midiNote / 12 - 1;
			}

			public PianoKey(int x, int y, int width, int height, int midiNote, bool isBlack) {
				this.rect = new Rectangle(x, y, width, height);
				this.midiNote = midiNote;
				this.isBlack = isBlack;
				this.octave = this.midiNote / 12 - 1;
			}
			
			public override string ToString() {
				return string.Format("{0}, {1}, black: {2}", midiNote, rect, isBlack);
			}
		}
		#endregion
	}
}
