// Sequence.java -- A sequence of MIDI events
//   Copyright (C) 2005 Free Software Foundation, Inc.

using System;
using System.Collections.Generic;

namespace gnu.sound.midi
{
	/// Objects of this type represent sequences of MIDI messages that can be
	/// played back by a Sequencer.
	/// @author Anthony Green (green@redhat.com)
	/// @since 1.3
	public class Sequence
	{
		/// <summary>
		/// The timing division type for this sequence (PPQ or SMPTE*)
		/// </summary>
		protected internal float divisionType;

		/// <summary>
		/// The timing resolution in ticks/beat or ticks/frame, depending on the
		/// division type.
		/// </summary>
		protected internal int resolution;

		/// <summary>
		/// The MIDI tracks used by this sequence.
		/// </summary>
		protected internal List<Track> tracks;

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

		/// <summary>
		/// The MIDI file type.  This is either 0, 1 or 2.
		/// Type 0 files contain a single track and represents a single song
		/// performance.
		/// Type 1 may contain multiple tracks for a single song performance.
		/// Type 2 may contain multiple tracks, each representing a
		/// separate song performance.
		/// See http://en.wikipedia.org/wiki/MIDI#MIDI_file_formats for more
		/// information.
		/// </summary>
		protected internal int type;
		
		// Private helper class
		private void Init(float divisionType, int resolution, int numTracks, int type)
		{
			if (divisionType != PPQ && divisionType != SMPTE_24 && divisionType != SMPTE_25 && divisionType != SMPTE_30 && divisionType != SMPTE_30DROP)
				throw new InvalidMidiDataException("Invalid division type (" + divisionType + ")");

			this.divisionType = divisionType;
			this.resolution = resolution;
			this.type = type;

			tracks = new List<Track>(numTracks);
			while (numTracks > 0) {
				tracks[--numTracks] = new Track();
			}
		}

		/// <summary>
		/// Create a MIDI sequence object with no initial tracks.
		/// @param divisionType the division type (must be one of PPQ or SMPTE_*)
		/// @param resolution the timing resolution
		/// @throws InvalidMidiDataException if the division type is invalid
		/// </summary>
		public Sequence(float divisionType, int resolution)
		{
			Init(divisionType, resolution, 0, 0);
		}

		/// <summary>
		/// Create a MIDI seqence object.
		/// @param divisionType the division type (must be one of PPQ or SMPTE_*)
		/// @param resolution the timing resolution
		/// @param numTracks the number of initial tracks
		/// @throws InvalidMidiDataException if the division type is invalid
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
		/// The division type of this sequence.
		/// @return division type of this sequence
		/// </summary>
		public float GetDivisionType()
		{
			return divisionType;
		}

		/// <summary>
		/// The timing resolution for this sequence, relative to the division type.
		/// @return the timing resolution for this sequence
		/// </summary>
		public int GetResolution()
		{
			return resolution;
		}

		/// <summary>
		/// Create a new empty MIDI track and add it to this sequence.
		/// @return the newly create MIDI track
		/// </summary>
		public Track CreateTrack()
		{
			var track = new Track();
			tracks.Add(track);
			return track;
		}

		/// <summary>
		/// Remove the specified MIDI track from this sequence.
		/// @param track the track to remove
		/// @return true if track was removed and false othewise
		/// </summary>
		public bool DeleteTrack(Track track)
		{
			return tracks.Remove(track);
		}

		/// <summary>
		/// Get an array of MIDI tracks used in this sequence.
		/// @return a possibly empty array of tracks
		/// </summary>
		public Track[] GetTracks()
		{
			return tracks.ToArray();
		}

		/// <summary>
		/// The length of this sequence in microseconds.
		/// @return the length of this sequence in microseconds
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
		/// @return the length of this sequence in MIDI ticks
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
		/// Get an array of patches used in this sequence.
		/// @return an array of patches used in this sequence
		/// </summary>
		public Patch[] GetPatchList()
		{
			// FIXE: not quite sure how to do this yet.
			throw new NotImplementedException("Can't get patch list yet");
		}
		
		public override string ToString()
		{
			return string.Format("[Type={0}, DivisionType={1}, Resolution={2}, Tracks={3}]", type, divisionType, resolution, tracks.Count);
		}
	}
}