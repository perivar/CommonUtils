// ExtendedMidiFileFormat -- extended with track count info.
// Copyright (C) 2006 Free Software Foundation, Inc.

namespace gnu.sound.midi.file
{
	/// ExtendedMidiFileFormat is a package private class that simply
	/// adds the number of MIDI tracks for the MidiFileFormat class.
	/// @author Anthony Green (green@redhat.com)
	/// Modified by Per Ivar Nerseth (perivar@nerseth.com)
	internal class ExtendedMidiFileFormat : gnu.sound.midi.MidiFileFormat
	{
		private int ntracks;

		/// <summary>
		/// Get the number of tracks for this MIDI file.
		/// <returns>the number of tracks for this MIDI file</returns>
		/// </summary>
		public int NumberOfTracks {
			get {
				return ntracks;
			}
		}

		/// <summary>
		/// Create an ExtendedMidiFileFormat object from the given parameters.
		/// <param name="type">the MIDI file type (0, 1, or 2)</param>
		/// <param name="divisionType">the MIDI file division type</param>
		/// <param name="resolution">the MIDI file timing resolution</param>
		/// <param name="bytes">the MIDI file size in bytes</param>
		/// <param name="microseconds">the MIDI file length in microseconds</param>
		/// <param name="ntracks">the number of tracks</param>
		/// </summary>
		public ExtendedMidiFileFormat(int type, float divisionType, int resolution, int bytes, long microseconds, int ntracks) : base(type, divisionType, resolution, bytes, microseconds)
		{
			this.ntracks = ntracks;
		}
	}

}