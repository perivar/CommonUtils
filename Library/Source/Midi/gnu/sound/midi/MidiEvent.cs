// MidiEvent.java -- A MIDI Event
// Copyright (C) 2005 Free Software Foundation, Inc.

namespace gnu.sound.midi
{

	/// A MIDI event is the combination of a MIDI message and a timestamp specified
	/// in MIDI ticks.
	/// @author Anthony Green (green@redhat.com)
	/// @since 1.3
	public class MidiEvent
	{
		readonly MidiMessage message;
		long tick;

		/// <summary>
		/// Create a MIDI event object from the given MIDI message and timestamp.
		/// <param name="message">the MidiMessage for this event</param>
		/// <param name="tick">the timestamp for this event</param>
		/// </summary>
		public MidiEvent(MidiMessage message, long tick)
		{
			this.message = message;
			this.tick = tick;
		}

		/// <summary>
		/// Get the MIDI message for this event.
		/// <returns>the MidiMessage for this event</returns>
		/// </summary>
		public MidiMessage Message {
			get {
				return message;
			}
		}

		/// <summary>
		/// The timestemp for this event in MIDI ticks.
		/// </summary>
		public long Tick {
			get {
				return tick;
			}
			set {
				tick = value;
			}
		}

		/// <summary>
		/// Creates a deep copy of the MIDI event.
		/// </summary>
		/// <returns>A deep clone of the MIDI event.</returns>
		public MidiEvent DeepClone() {
			return new MidiEvent((MidiMessage) message.Clone(), tick);
		}
		
		public override string ToString()
		{
			return string.Format("[{0}] {1}", tick, message);
		}
	}
}