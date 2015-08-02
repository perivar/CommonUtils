// MidiEvent.java -- A MIDI Event
//   Copyright (C) 2005 Free Software Foundation, Inc.

namespace gnu.sound.midi
{

	/// A MIDI event is the combination of a MIDI message and a timestamp specified
	/// in MIDI ticks.
	/// @author Anthony Green (green@redhat.com)
	/// @since 1.3
	public class MidiEvent
	{
		private readonly MidiMessage message;
		private long tick;

		/// <summary>
		/// Create a MIDI event object from the given MIDI message and timestamp.
		/// @param message the MidiMessage for this event
		/// @param tick the timestamp for this event
		/// </summary>
		public MidiEvent(MidiMessage message, long tick)
		{
			this.message = message;
			this.tick = tick;
		}

		/// <summary>
		/// Get the MIDI message for this event.
		/// @return the MidiMessage for this event
		/// </summary>
		public MidiMessage GetMessage()
		{
			return message;
		}

		/// <summary>
		/// Set the timestemp for this event in MIDI ticks.
		/// @param tick the timestamp
		/// </summary>
		public void SetTick(long tick)
		{
			this.tick = tick;
		}

		/// <summary>
		/// Get the timestamp for this event in MIDI ticks.
		/// @return the timestamp for this even in MIDI ticks
		/// </summary>
		public long GetTick()
		{
			return tick;
		}
		
		public override string ToString()
		{
			return string.Format("[{0}] {1}", tick, message);
		}
	}
}