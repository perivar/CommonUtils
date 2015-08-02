// Track.java -- A track of MIDI events
//   Copyright (C) 2005 Free Software Foundation, Inc.

using System;
using System.Collections.Generic;

namespace gnu.sound.midi
{

	/// A Track contains a list of timecoded MIDI events for processing
	/// by a Sequencer.
	/// @author Anthony Green (green@redhat.com)
	/// @since 1.3
	public class Track
	{
		/// <summary>
		/// The list of MidiEvents for this track.
		/// </summary>
		private List<MidiEvent> events = new List<MidiEvent>();

		// This is only instantiable within this package.
		internal Track()
		{
		}

		/// <summary>
		/// Add a new event to this track.  Specific events may only be added once.
		/// The event will be inserted into the appropriate spot in the event list
		/// based on its timecode.
		/// @param event the event to add
		/// @return true if the event was added, false otherwise
		/// </summary>
		public bool Add(MidiEvent @event)
		{
			lock (events)
			{
				if (events.Contains(@event))
					return false;

				long targetTick = @event.GetTick();
				int i = events.Count - 1;
				while (i >= 0 && (((MidiEvent)events[i]).GetTick() > targetTick))
					i--;
				events.Insert(i+1, @event);
				return true;
			}
		}

		/// <summary>
		/// Remove an event from this track.
		/// @param event the event to remove
		/// @return true if the event was removed, false otherwise
		/// </summary>
		public bool Remove(MidiEvent @event)
		{
			lock (events)
			{
				return events.Remove(@event);
			}
		}

		/// <summary>
		/// Get an event idetified by its order index
		/// @param index the location of the event to get
		/// @return the event at index
		/// @throws ArrayIndexOutOfBoundsException if index is out of bounds
		/// </summary>
		public MidiEvent Get(int index)
		{
			lock (events)
			{
				return (MidiEvent) events[index];
			}
		}

		/// <summary>
		/// Get the event list
		/// </summary>
		/// <returns>the list of events</returns>
		public List<MidiEvent> GetEvents() {
			return this.events;
		}

		/// <summary>
		/// Get the number events in this track.
		/// @return the number of events in this track
		/// </summary>
		public int Size()
		{
			return events.Count;
		}

		/// <summary>
		/// Get the length of the track in MIDI ticks.
		/// @return the length of the track in MIDI ticks
		/// </summary>
		public long Ticks()
		{
			lock (events)
			{
				int size = events.Count;
				return ((MidiEvent) events[size - 1]).GetTick();
			}
		}
		
		public override string ToString()
		{
			return string.Format("[Track Events={0}]", Size());
		}

	}

}