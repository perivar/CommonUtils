// Sequence -- A sequence of MIDI events
// Copyright (C) 2005 Free Software Foundation, Inc.

using System;
using System.IO;
using System.Collections.Generic;

using gnu.sound.midi.info;

namespace gnu.sound.midi
{
	/// Objects of this type represent sequences of MIDI messages that can be
	/// played back by a Sequencer.
	/// @author Anthony Green (green@redhat.com)
	/// Modified by Per Ivar Nerseth (perivar@nerseth.com)
	public class Sequence
	{
		/// <summary>
		/// The MIDI file type.  This is either 0, 1 or 2.
		/// Type 0 files contain a single track and represents a single song performance.
		/// Type 1 may contain multiple tracks for a single song performance.
		/// Type 2 may contain multiple tracks, each representing a separate song performance.
		/// See http://en.wikipedia.org/wiki/MIDI#MIDI_file_formats for more information.
		/// </summary>
		int midiFileType;

		/// <summary>
		/// The timing division type for this sequence (PPQ or SMPTE*)
		/// </summary>
		float divisionType;

		/// <summary>
		/// The timing resolution in ticks/beat or ticks/frame, depending on the
		/// division type.
		/// </summary>
		int resolution;

		/// <summary>
		/// The MIDI tracks used by this sequence.
		/// </summary>
		List<Track> tracks;

		/// <summary>
		/// Tempo-based timing.  Resolution is specified in ticks per beat.
		/// </summary>
		public const float PPQ = 0.0f;

		/// <summary>
		/// 24 frames/second timing.  Resolution is specific in ticks per frame.
		/// </summary>
		public const float SMPTE_24 = 24.0f;

		/// <summary>
		/// 25 frames/second timing.  Resolution is specific in ticks per frame.
		/// </summary>
		public const float SMPTE_25 = 25.0f;

		/// <summary>
		/// 30 frames/second timing.  Resolution is specific in ticks per frame.
		/// </summary>
		public const float SMPTE_30 = 30.0f;

		/// <summary>
		/// 29.97 frames/second timing.  Resolution is specific in ticks per frame.
		/// </summary>
		public const float SMPTE_30DROP = 29.97f;

		// Private helper class
		private void Init(float divisionType, int resolution, int numTracks, int type)
		{
			if (divisionType != PPQ && divisionType != SMPTE_24 && divisionType != SMPTE_25 && divisionType != SMPTE_30 && divisionType != SMPTE_30DROP)
				throw new InvalidMidiDataException("Invalid division type (" + divisionType + ")");

			this.divisionType = divisionType;
			this.resolution = resolution;
			this.midiFileType = type;

			tracks = new List<Track>(numTracks);
			while (numTracks > 0) {
				--numTracks;
				tracks.Add(new Track());
			}
		}

		/// <summary>
		/// Initialize the MIDI sequence with a copy of the data from another sequence.
		/// </summary>
		/// <param name="source">The source sequence from which to copy.</param>
		public Sequence(Sequence source)
		{
			this.divisionType = source.divisionType;
			this.resolution = source.resolution;
			this.midiFileType = source.midiFileType;
			
			tracks = new List<Track>(source.Tracks.Count);
			foreach (Track t in source.Tracks) {
				tracks.Add(new Track(t));
			}
		}
		
		/// <summary>
		/// Create a MIDI sequence object with no initial tracks.
		/// <param name="divisionType">the division type (must be one of PPQ or SMPTE_*)</param>
		/// <param name="resolution">the timing resolution</param>
		/// <exception cref="InvalidMidiDataException">if the division type is invalid</exception>
		/// </summary>
		public Sequence(float divisionType, int resolution)
		{
			Init(divisionType, resolution, 0, 0);
		}

		/// <summary>
		/// Create a MIDI seqence object.
		/// <param name="divisionType">the division type (must be one of PPQ or SMPTE_*)</param>
		/// <param name="resolution">the timing resolution</param>
		/// <param name="numTracks">the number of initial tracks</param>
		/// <exception cref="InvalidMidiDataException">if the division type is invalid</exception>
		/// </summary>
		public Sequence(float divisionType, int resolution, int numTracks)
		{
			Init(divisionType, resolution, numTracks, 0);
		}

		/// <summary>
		/// Create a MIDI seqence object.
		/// </summary>
		/// <param name="divisionType">the division type (must be one of PPQ or SMPTE_*)</param>
		/// <param name="resolution">the timing resolution</param>
		/// <param name="numTracks">the number of initial tracks</param>
		/// <param name="type">the midi format type</param>
		public Sequence(float divisionType, int resolution, int numTracks, int type)
		{
			Init(divisionType, resolution, numTracks, type);
		}
		
		/// <summary>
		/// Get the MIDI file type (0, 1, or 2).
		/// <returns>the MIDI file type (0, 1, or 2)</returns>
		/// </summary>
		public int MidiFileType {
			get {
				return midiFileType;
			}
			set {
				this.midiFileType = value;
			}
		}
		
		/// <summary>
		/// Get the file division type.
		/// <returns>the file divison type</returns>
		/// </summary>
		public float DivisionType {
			get {
				return divisionType;
			}
		}

		/// <summary>
		/// Get the file timing resolution.  If the division type is PPQ, then this
		/// is value represents ticks per beat, otherwise it's ticks per frame (SMPTE).
		/// <returns>the timing resolution in ticks per beat or ticks per frame</returns>
		/// </summary>
		public int Resolution {
			get {
				return resolution;
			}
			set {
				this.resolution = value;
			}
		}
		
		/// <summary>
		/// An array of MIDI tracks used in this sequence.
		/// <returns>returns a possibly empty array of tracks</returns>
		/// </summary>
		public List<Track> Tracks {
			get {
				return tracks;
			}
		}

		/// <summary>
		/// Create a new empty MIDI track and add it to this sequence.
		/// <returns>the newly create MIDI track</returns>
		/// </summary>
		public Track CreateTrack()
		{
			var track = new Track();
			tracks.Add(track);
			return track;
		}

		/// <summary>
		/// Remove the specified MIDI track from this sequence.
		/// <param name="track">the track to remove</param>
		/// <returns>true if track was removed and false othewise</returns>
		/// </summary>
		public bool DeleteTrack(Track track)
		{
			return tracks.Remove(track);
		}

		/// <summary>
		/// The length of this sequence in microseconds.
		/// <returns>the length of this sequence in microseconds</returns>
		/// </summary>
		public long GetMicrosecondLength()
		{
			long tickLength = GetTickLength();

			if (divisionType == PPQ)
			{
				// FIXME
				// How can this possible be computed?  PPQ is pulses per quarter-note,
				// which is dependent on the tempo of the Sequencer.
				throw new InvalidOperationException("Can't compute PPQ based lengths yet");
			}
			else
			{
				// This is a fixed tick per frame computation
				return (long)((tickLength * 1000000) / (divisionType * resolution));
			}
		}

		/// <summary>
		/// The length of this sequence in MIDI ticks.
		/// <returns>the length of this sequence in MIDI ticks</returns>
		/// </summary>
		public long GetTickLength()
		{
			long length = 0;
			var itr = tracks.GetEnumerator();
			while (itr.MoveNext())
			{
				Track track = itr.Current;
				long trackTicks = track.Ticks();
				if (trackTicks > length) {
					length = trackTicks;
				}
			}
			return length;
		}
		
		/// <summary>
		/// Return a text representation of this Sequence
		/// </summary>
		/// <returns>a textual string representing this sequence</returns>
		public override string ToString()
		{
			return string.Format("[Type={0}, DivisionType={1}, Resolution={2}, Tracks={3}]", midiFileType, divisionType, resolution, tracks.Count);
		}
		
		#region Util methods
		
		/// <summary>
		/// Creates C# code from a MIDI sequence.
		/// </summary>
		/// <param name="outputFilePath">path to text file to store generated code to</param>
		public void SaveGenerateMidiCode(string outputFilePath) {
			
			// generate code
			int trackno = 1;
			using (var outfile = new StreamWriter(outputFilePath, false)) {

				string midiTypeName = MidiHelper.GetMidiFormatString(midiFileType);
				
				outfile.WriteLine("public static Sequence GenerateMidi() {");
				outfile.WriteLine();
				outfile.WriteLine("\t// Generate midi file");
				outfile.WriteLine("\tconst int resolution = {0};", this.Resolution);
				outfile.WriteLine("\tvar sequence = new Sequence({0}, resolution, 0, (int) MidiHelper.MidiFormat.{1});", this.DivisionType, midiTypeName);
				
				foreach (var track in this.Tracks) {
					outfile.WriteLine("\n\tvar track{0} = sequence.CreateTrack();", trackno);
					foreach(var ev in track.Events) {
						long tick = ev.Tick;
						int beat = (int) tick / this.Resolution;
						int tickRemainder = (int) tick % this.Resolution;

						MidiMessage msg = ev.Message;
						
						// check if this is a meta message
						var mm = msg as MetaMessage;
						if (mm != null) {
							string metaCodeString = mm.CreateMetaEventGeneratedCode(tick, this.Resolution);
							outfile.WriteLine("\ttrack{0}.Add({1});", trackno, metaCodeString);
						}
						
						// check if this is a short message
						var sm = msg as ShortMessage;
						if (sm != null) {
							string shortCodeString = sm.CreateShortEventGeneratedCode(true, tick);
							outfile.WriteLine("\ttrack{0}.Add({1}", trackno, shortCodeString);
						}
						
						// check if this is a sysex message
						var ss = msg as SysexMessage;
						if (ss != null) {
							outfile.WriteLine("\t// We don't support SysexMessage now");
						}
					}
					trackno++;
				}
				
				outfile.WriteLine("\n\treturn sequence;");
				outfile.WriteLine("}");
			}
		}
		
		/// <summary>
		/// Dump the Midi content as a human readable text file
		/// </summary>
		/// <param name="outputFilePath">text file to store dump to</param>
		public void DumpMidi(string outputFilePath) {
			
			int trackno = 1;
			using (var outfile = new StreamWriter(outputFilePath, false)) {
				
				// write header
				string midiTypeName = MidiHelper.GetMidiFormatString(midiFileType);
				outfile.WriteLine("Midi Type: {0} = {1}", this.MidiFileType, midiTypeName);
				
				string divisionTypeString = "";
				if (divisionType == PPQ) {
					divisionTypeString = "PPQ";
				} else if(divisionType == SMPTE_24) {
					divisionTypeString = "SMPTE_24";
				} else if(divisionType == SMPTE_25) {
					divisionTypeString = "SMPTE_25";
				} else if(divisionType == SMPTE_30) {
					divisionTypeString = "SMPTE_30";
				} else if(divisionType == SMPTE_30DROP) {
					divisionTypeString = "SMPTE_30DROP";
				}
				outfile.WriteLine("Division Type: {0} = {1}", divisionTypeString, divisionType);
				outfile.WriteLine("Resolution: {0}", this.Resolution);
				outfile.WriteLine("Tracks: {0}", this.Tracks.Count);
				outfile.WriteLine("Time: {0} ticks", this.GetTickLength());
				outfile.WriteLine();
				
				foreach (var track in this.Tracks) {
					outfile.WriteLine("Track {0}", trackno);
					foreach(var ev in track.Events) {
						long tick = ev.Tick;
						int beat = (int) tick / this.Resolution;
						int tickRemainder = (int) tick % this.Resolution;

						MidiMessage msg = ev.Message;
						outfile.WriteLine("{0:0000}:{1:000} {2}", beat, tickRemainder, msg);
					}
					trackno++;
				}
			}
		}
		
		#endregion
	}
}