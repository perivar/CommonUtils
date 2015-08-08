// Sequence.java -- A sequence of MIDI events
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
	/// @since 1.3
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
		
		public override string ToString()
		{
			return string.Format("[Type={0}, DivisionType={1}, Resolution={2}, Tracks={3}]", midiFileType, divisionType, resolution, tracks.Count);
		}
		
		#region Util methods
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
						if (msg is MetaMessage) {
							string metaCodeString = MetaEvent.CreateMetaEventGeneratedCode((MetaMessage) msg, tick, this.Resolution);
							outfile.WriteLine("\ttrack{0}.Add({1});", trackno, metaCodeString);
						} else if (msg is ShortMessage) {
							string shortCodeString = ShortEvent.CreateShortEventGeneratedCode((ShortMessage) msg, true, tick);
							outfile.WriteLine("\ttrack{0}.Add({1}", trackno, shortCodeString);
						} else if (msg is SysexMessage) {
							outfile.WriteLine("\t// We don't support SysexMessage now");
						}
					}
					trackno++;
					
				}
				
				outfile.WriteLine("\n\treturn sequence;");
				outfile.WriteLine("}");
			}
		}
		
		public void DumpMidi(string outputFilePath) {
			
			int trackno = 1;
			using (var outfile = new StreamWriter(outputFilePath, false)) {
				
				// write header
				string midiTypeName = MidiHelper.GetMidiFormatString(midiFileType);
				outfile.WriteLine("Midi Type: {0} = {1}", this.MidiFileType, midiTypeName);
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
		
		#region Useful methods based the excellen MidiSharp package by
		/// <summary>Transposes a MIDI sequence up/down the specified number of half-steps.</summary>
		/// <param name="sequence">The sequence to be transposed.</param>
		/// <param name="steps">The number of steps up(+) or down(-) to transpose the sequence.</param>
		public static void Transpose(Sequence sequence, int steps)
		{
			// Transpose the sequence; do not transpose drum tracks
			Transpose(sequence, steps, false);
		}

		/// <summary>Transposes a MIDI sequence up/down the specified number of half-steps.</summary>
		/// <param name="sequence">The sequence to be transposed.</param>
		/// <param name="steps">The number of steps up(+) or down(-) to transpose the sequence.</param>
		/// <param name="includeDrums">Whether drum tracks should also be transposed.</param>
		/// <remarks>If the step value is too large or too small, notes may wrap.</remarks>
		public static void Transpose(Sequence sequence, int steps, bool includeDrums)
		{
			// Modify each track
			foreach (Track track in sequence.Tracks) {
				// Modify each event
				foreach (MidiEvent ev in track.Events) {
					
					// If the event is not a voice MIDI event but the channel is the
					// drum channel and the user has chosen not to include drums in the
					// transposition (which makes sense), skip this event.
					MidiMessage msg = ev.Message;
					
					if (msg is ShortMessage) {

						// get status code
						int st = msg.GetStatus();
						
						// check if this is a channel message
						if ((st & 0xf0) <= 0xf0)
						{
							var channel = ((ShortMessage)msg).GetChannel();
							
							if (!includeDrums && channel == (byte)MidiHelper.DRUM_CHANNEL)
								continue;

							int cmd = ((ShortMessage)msg).GetCommand();
							
							// If the event is a NoteOn, NoteOff, or Aftertouch
							if (cmd == (int) MidiHelper.MidiEventType.NoteOff
							    || cmd == (int) MidiHelper.MidiEventType.NoteOn
							    || cmd == (int) MidiHelper.MidiEventType.AfterTouchPoly) {
								
								// shift the note according to the supplied number of steps.
								var data1 = ((ShortMessage)msg).GetData1();
								var data2 = ((ShortMessage)msg).GetData2();

								// note number is stored in data[1]
								byte noteTransposed = (byte)((data1 + steps) % 128);
								
								// store the track number as the channel
								((ShortMessage)msg).SetMessage(cmd, channel, noteTransposed, data2);
							}
						}
					}
				}
			}
		}

		/// <summary>Trims a MIDI file to a specified length.</summary>
		/// <param name="sequence">The sequence to be copied and trimmed.</param>
		/// <param name="totalTime">The requested time length of the new MIDI sequence.</param>
		/// <returns>A MIDI sequence with only those events that fell before the requested time limit.</returns>
		public static Sequence Trim(Sequence sequence, long totalTime)
		{
			// Create a new sequence to mimic the old
			var newSequence = new Sequence(sequence.DivisionType, sequence.Resolution, 0, sequence.MidiFileType);

			// Copy each track up to the specified time limit
			foreach (Track track in sequence.Tracks) {
				// Create a new track in the new sequence to match the old track in the old sequence
				var newTrack = newSequence.CreateTrack();

				// Copy over all events that fell before the specified time
				for (int i = 0; i < track.Events.Count && track.Events[i].Tick < totalTime; i++) {
					newTrack.Events.Add(track.Events[i].DeepClone()); // add at the end
					//newTrack.Add(track.Events[i].DeepClone()); // insert at correct timing
				}

				// If the new track lacks an end of track, add one
				if (!newTrack.HasEndOfTrack) {
					newTrack.Add(MetaEvent.CreateMetaEvent("EndOfTrack", "", newTrack.Ticks(), 0));
				}
			}

			// Return the new sequence
			return newSequence;
		}
		
		/// <summary>Converts a MIDI sequence from its current format to the specified format.</summary>
		/// <param name="sequence">The sequence to be converted.</param>
		/// <param name="format">The format to which we want to convert the sequence.</param>
		/// <returns>The converted sequence.</returns>
		/// <remarks>
		/// This may or may not return the same sequence as passed in.
		/// </remarks>
		/// <remarks>This is based on the excellent MidiSharp package by Stephen Toub.</remarks>
		public static Sequence Convert(Sequence sequence, int format)
		{
			return Convert(sequence, format, FormatConversionOption.None);
		}
		
		/// <summary>Converts the MIDI sequence into a new one with the desired format.</summary>
		/// <param name="sequence">The sequence to be converted.</param>
		/// <param name="format">The format to which we want to convert the sequence.</param>
		/// <param name="options">Options used when doing the conversion.</param>
		/// <returns>The new, converted sequence.</returns>
		/// <remarks>This is based on the excellent MidiSharp package by Stephen Toub.</remarks>
		public static Sequence Convert(Sequence sequence, int format, FormatConversionOption options)
		{
			if (sequence.MidiFileType == format) {
				// If the desired format is the same as the original, just return a copy.
				// No transformation is necessary.
				sequence = new Sequence(sequence);
			}
			else if (format != 0 || sequence.Tracks.Count == 1) {
				// If the desired format is is not 0 or there's only one track, just copy the sequence with a different format number.
				// If it's not zero, then multiple tracks are acceptable, so no transformation is necessary.
				// Or if there's only one track, then there's no possible transformation to be done.
				var newSequence = new Sequence(sequence.DivisionType, sequence.Resolution, 0, format);
				foreach (Track t in sequence.Tracks) {
					newSequence.Tracks.Add(new Track(t));
				}
				sequence = newSequence;
			}
			else {
				// Now the harder cases, converting to format 0.  We need to combine all tracks into 1,
				// as format 0 requires that there only be a single track with all of the events for the song.
				sequence = new Sequence(sequence);
				sequence.MidiFileType = (int) MidiHelper.MidiFormat.SingleTrack;

				// Add all events to new track (except for end of track markers!)
				int trackNumber = 0;
				var newTrack = new Track();
				foreach (Track track in sequence.Tracks) {
					foreach (MidiEvent midiEvent in track.Events) {
						bool doAddEvent = true;
						
						MidiMessage msg = midiEvent.Message;
						if (msg is MetaMessage) {
							// add all meta messages except the end of track markers (we'll add our own)
							int type = ((MetaMessage)msg).GetMetaMessageType();
							if (type == (int) MidiHelper.MetaEventType.EndOfTrack) {
								doAddEvent = false;
							}
						} else if (msg is ShortMessage) {
							// If this event has a channel, and if we're storing tracks as channels, copy to it
							if ((options & FormatConversionOption.CopyTrackToChannel) > 0
							    && trackNumber >= MidiHelper.MIN_CHANNEL && trackNumber <= MidiHelper.MAX_CHANNEL) {

								// get status code
								int st = msg.GetStatus();
								
								// check if this is a channel message
								if ((st & 0xf0) <= 0xf0)
								{
									// get the data
									var commandByte = ((ShortMessage)msg).GetCommand();
									var data1 = ((ShortMessage)msg).GetData1();
									var data2 = ((ShortMessage)msg).GetData2();
									
									// store the track number as the channel
									((ShortMessage)msg).SetMessage(commandByte, trackNumber, data1, data2);
								}
							}
						} else if (msg is SysexMessage) {
							
						}
						
						// Add all events, except for end of track markers (we'll add our own)
						if (doAddEvent) {
							//newTrack.Events.Add(midiEvent);
							newTrack.Add(midiEvent);
						}
					}
					trackNumber++;
				}

				// Sort the events by total time
				// and top things off with an end-of-track marker.
				//newTrack.Events.Sort((x, y) => x.Tick.CompareTo(y.Tick));
				newTrack.Add(MetaEvent.CreateMetaEvent("EndOfTrack", "", newTrack.Ticks(), 0));

				// We now have all of the combined events in newTrack.  Clear out the sequence, replacing all the tracks
				// with this new one.
				sequence.Tracks.Clear();
				sequence.Tracks.Add(newTrack);
			}

			return sequence;
		}

		/// <summary>Options used when performing a format conversion.</summary>
		public enum FormatConversionOption
		{
			/// <summary>No special formatting.</summary>
			None,

			/// <summary>
			/// Uses the number of the track as the channel for all events on that track.
			/// Only valid if the number of the track is a valid track number.
			/// </summary>
			CopyTrackToChannel
		}
		#endregion
		
		#endregion
	}
}