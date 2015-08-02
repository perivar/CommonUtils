// ExtendedMidiFileFormat.java -- extended with track count info.
//   Copyright (C) 2006 Free Software Foundation, Inc.

namespace gnu.sound.midi.file
{
	/// ExtendedMidiFileFormat is a package private class that simply
	/// adds the number of MIDI tracks for the MidiFileFormat class.
	/// @author Anthony Green (green@redhat.com)
	internal class ExtendedMidiFileFormat : gnu.sound.midi.MidiFileFormat
	{
		private int ntracks;

		/// <summary>
		/// Get the number of tracks for this MIDI file.
		/// @return the number of tracks for this MIDI file
		/// </summary>
		public int GetNumberTracks()
		{
			return ntracks;
		}

		/// <summary>
		/// Create an ExtendedMidiFileFormat object from the given parameters.
		/// @param type the MIDI file type (0, 1, or 2)
		/// @param divisionType the MIDI file division type
		/// @param resolution the MIDI file timing resolution
		/// @param bytes the MIDI file size in bytes
		/// @param microseconds the MIDI file length in microseconds
		/// @param ntracks the number of tracks
		/// </summary>
		public ExtendedMidiFileFormat(int type, float divisionType, int resolution, int bytes, long microseconds, int ntracks) : base(type, divisionType, resolution, bytes, microseconds)
		{
			this.ntracks = ntracks;
		}
	}

}