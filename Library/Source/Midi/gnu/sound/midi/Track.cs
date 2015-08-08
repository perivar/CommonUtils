// Track.java -- A track of MIDI events
// Copyright (C) 2005 Free Software Foundation, Inc.

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
		/// Initializes the track with a copy of the data in another track.
		/// </summary>
		/// <returns>The track to copy.</returns>
		public Track(Track source)
		{
			foreach (var e in source.Events) {
				this.Add(e.DeepClone());
			}
		}
		
		/// <summary>
		/// Add a new event to this track. Specific events may only be added once.
		/// The event will be inserted into the appropriate spot in the event list
		/// based on its timecode.
		/// <param name="event">the event to add</param>
		/// <returns>true if the event was added, false otherwise</returns>
		/// </summary>
		public bool Add(MidiEvent @event)
		{
			lock (events)
			{
				if (events.Contains(@event))
					return false;

				long targetTick = @event.Tick;
				int i = events.Count - 1;
				while (i >= 0 && (((MidiEvent)events[i]).Tick > targetTick)) {
					i--;
				}
				events.Insert(i+1, @event);
				return true;
			}
		}

		/// <summary>
		/// Remove an event from this track.
		/// <param name="event">the event to remove</param>
		/// <returns>true if the event was removed, false otherwise</returns>
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
		/// <param name="index">the location of the event to get</param>
		/// <returns>the event at index</returns>
		/// <exception cref="ArrayIndexOutOfBoundsException">if index is out of bounds</exception>
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
		public List<MidiEvent> Events {
			get {
				return events;
			}
		}
		
		/// <summary>
		/// Get the number events in this track.
		/// <returns>the number of events in this track</returns>
		/// </summary>
		public int EventCount()
		{
			return events.Count;
		}

		/// <summary>
		/// Get the length of the track in MIDI ticks.
		/// <returns>the length of the track in MIDI ticks</returns>
		/// </summary>
		public long Ticks()
		{
			lock (events)
			{
				int size = events.Count;
				return ((MidiEvent) events[size - 1]).Tick;
			}
		}
		
		/// <summary>Gets whether an end of track event has been added.</summary>
		public bool HasEndOfTrack
		{
			get
			{
				if (events.Count == 0) return false;
				
				MidiMessage msg = events[events.Count-1].Message;
				if (msg is MetaMessage) {
					int type = ((MetaMessage)msg).GetMetaMessageType();
					if (type == (int) MidiHelper.MetaEventType.EndOfTrack) {
						return true;
					}
				}
				return false;
			}
		}
		
		public override string ToString()
		{
			return string.Format("[Events={0}, Ticks:{1}]", EventCount(), Ticks());
		}
	}
}