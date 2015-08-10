using System;
using gnu.sound.midi.info;

namespace gnu.sound.midi
{
	/// <summary>Common manipulations of Midi Sequences.</summary>
	public static class SequenceExtensions
	{
		#region Useful methods based the excellent MidiSharp package by Stephen Toub
		
		/// <summary>Transposes a MIDI sequence up/down the specified number of half-steps.</summary>
		/// <param name="sequence">The sequence to be transposed.</param>
		/// <param name="steps">The number of steps up(+) or down(-) to transpose the sequence.</param>
		public static void Transpose(this Sequence sequence, int steps)
		{
			// Transpose the sequence; do not transpose drum tracks
			Transpose(sequence, steps, false);
		}

		/// <summary>Transposes a MIDI sequence up/down the specified number of half-steps.</summary>
		/// <param name="sequence">The sequence to be transposed.</param>
		/// <param name="steps">The number of steps up(+) or down(-) to transpose the sequence.</param>
		/// <param name="includeDrums">Whether drum tracks should also be transposed.</param>
		/// <remarks>If the step value is too large or too small, notes may wrap.</remarks>
		public static void Transpose(this Sequence sequence, int steps, bool includeDrums)
		{
			// Modify each track
			foreach (Track track in sequence.Tracks) {
				// Modify each event
				foreach (MidiEvent ev in track.Events) {
					
					// If the event is not a voice MIDI event but the channel is the
					// drum channel and the user has chosen not to include drums in the
					// transposition (which makes sense), skip this event.
					MidiMessage msg = ev.Message;
					
					// check if this is a short message
					var sm = msg as ShortMessage;
					if (sm != null) {
						
						// get status code
						int st = msg.GetStatus();
						
						// check if this is a channel message
						if (sm.IsChannelMessage())
						{
							var channel = sm.GetChannel();
							
							if (!includeDrums && channel == (byte)MidiHelper.DRUM_CHANNEL)
								continue;

							int cmd = sm.GetCommand();
							
							// If the event is a NoteOn, NoteOff or Aftertouch
							if (cmd == (int) MidiHelper.MidiEventType.NoteOff
							    || cmd == (int) MidiHelper.MidiEventType.NoteOn
							    || cmd == (int) MidiHelper.MidiEventType.AfterTouchPoly) {
								
								// shift the note according to the supplied number of steps.
								var data1 = sm.GetData1();
								var data2 = sm.GetData2();

								// note number is stored in data[1]
								byte noteTransposed = (byte)((data1 + steps) % 128);
								
								// store the track number as the channel
								sm.SetMessage(cmd, channel, noteTransposed, data2);
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
		public static Sequence Trim(this Sequence sequence, long totalTime)
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
		/// This is based on the excellent MidiSharp package by Stephen Toub.
		/// </remarks>
		public static Sequence Convert(this Sequence sequence, int format)
		{
			return Convert(sequence, format, FormatConversionOption.None, 0, null);
		}

		/// <summary>Converts a MIDI sequence from its current format to the specified format.</summary>
		/// <param name="sequence">The sequence to be converted.</param>
		/// <param name="format">The format to which we want to convert the sequence.</param>
		/// <param name="newResolution">the new resolution (disable using 0 or -1)</param>
		/// <returns>The converted sequence.</returns>
		/// <remarks>
		/// This may or may not return the same sequence as passed in.
		/// This is based on the excellent MidiSharp package by Stephen Toub.
		/// </remarks>
		public static Sequence Convert(this Sequence sequence, int format, int newResolution)
		{
			return Convert(sequence, format, FormatConversionOption.None, newResolution, null);
		}
		
		/// <summary>Converts the MIDI sequence into a new one with the desired format.</summary>
		/// <param name="sequence">The sequence to be converted.</param>
		/// <param name="format">The format to which we want to convert the sequence.</param>
		/// <param name="options">Options used when doing the conversion.</param>
		/// <param name="newResolution">the new resolution (disable using 0 or -1)</param>
		/// <returns>The new, converted sequence.</returns>
		/// <remarks>This is based on the excellent MidiSharp package by Stephen Toub.</remarks>
		public static Sequence Convert(this Sequence sequence, int format, FormatConversionOption options, int newResolution, string trackName)
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
				int originalResolution = sequence.Resolution;
				if (newResolution <= 0) newResolution = originalResolution;

				sequence = new Sequence(sequence);
				sequence.MidiFileType = (int) MidiHelper.MidiFormat.SingleTrack;

				// Add all events to new track (except for end of track markers and SequenceOrTrackName events)
				int trackNumber = 0;
				var newTrack = new Track();
				
				foreach (Track track in sequence.Tracks) {
					foreach (MidiEvent midiEvent in track.Events) {
						bool doAddEvent = true;
						
						var msg = midiEvent.Message;
						
						// check if this is a meta message
						var mm = msg as MetaMessage;
						if (mm != null) {

							// we have a meta message
							// add all meta messages except the end of track markers (we'll add our own)
							int type = mm.GetMetaMessageType();
							
							if (type == (int) MidiHelper.MetaEventType.EndOfTrack) {
								doAddEvent = false;
							} else if (type == (int) MidiHelper.MetaEventType.SequenceOrTrackName) {
								doAddEvent = false;

								// store track name, will be used later
								if (string.IsNullOrEmpty(trackName)) {
									byte[] data = mm.GetMetaMessageData();
									string text = MidiHelper.GetString(data);
									trackName = MidiHelper.TextString(text);
								}
							}
						}
						
						// check if this is a short message
						var sm = msg as ShortMessage;
						if (sm != null) {
							// get the data
							var commandByte = sm.GetCommand();
							var data1 = sm.GetData1();
							var data2 = sm.GetData2();
							
							// If this event has a channel, and if we're storing tracks as channels, copy to it
							if ((options & FormatConversionOption.CopyTrackToChannel) > 0
							    && trackNumber >= MidiHelper.MIN_CHANNEL && trackNumber <= MidiHelper.MAX_CHANNEL) {
								
								if (sm.IsChannelMessage()) {
									// store the track number as the channel
									sm.SetMessage(commandByte, trackNumber, data1, data2);
								}
							}
							
							if ((options & FormatConversionOption.NoteOffZero2NoteOnZero) > 0) {
								
								// If the event is a NoteOff with Volume 0
								if (commandByte == (int) MidiHelper.MidiEventType.NoteOff && data2 == 0) {
									// convert to a NoteOn instead
									sm.SetMessage((int) MidiHelper.MidiEventType.NoteOn, data1, data2);
								}
							}
						}
						
						// convert ticks if resolution has changed
						if (originalResolution != newResolution) {
							if (midiEvent.Tick != 0) {
								double fraction = (double) midiEvent.Tick / (double) originalResolution;
								int tick = (int) (fraction * newResolution);
								midiEvent.Tick = tick;
							}
						}
						
						// Add all events, except for end of track markers (we'll add our own)
						if (doAddEvent) {
							//newTrack.Events.Add(midiEvent); // add to end of list
							newTrack.Add(midiEvent); // add in the right position based on the tick
						}
					}
					trackNumber++;
				}

				if (originalResolution != newResolution) {
					sequence.Resolution = newResolution;
				}
				
				// Sort the events by total time
				// newTrack.Events.Sort((x, y) => x.Tick.CompareTo(y.Tick));
				// Note! using newTrack.Add instead of newTrack.Events.Add, already ensures a correct sort order
				
				// Top things off with an end-of-track marker.
				newTrack.Add(MetaEvent.CreateMetaEvent("EndOfTrack", "", newTrack.Ticks(), 0));

				// add a new track name as the very first event
				newTrack.Events.Insert(0, MetaEvent.CreateMetaEvent((int) MidiHelper.MetaEventType.SequenceOrTrackName, trackName, 0, 0));
				
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
			CopyTrackToChannel,
			
			/// <summary>
			/// Convert NoteOff events with Volume 0 to NoteOn events with Volume 0
			/// </summary>
			NoteOffZero2NoteOnZero
		}
		#endregion
		
	}
}