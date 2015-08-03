// MidiFileFormat.java -- Information about a MIDI file
//   Copyright (C) 2005 Free Software Foundation, Inc.

namespace gnu.sound.midi
{
	/// Describe a MIDI file, including specifics about its type, length and timing.
	/// @author Anthony Green (green@redhat.com)
	/// @since 1.3
	public class MidiFileFormat
	{
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

		/// <summary>
		/// The division type of the MIDI file.
		/// </summary>
		protected internal float divisionType;

		/// <summary>
		/// The timing resolution of the MIDI file.
		/// </summary>
		protected internal int resolution;

		/// <summary>
		/// The size of the MIDI file in bytes.
		/// </summary>
		protected internal int byteLength = UNKNOWN_LENGTH;

		/// <summary>
		/// The length of the MIDI file in microseconds.
		/// </summary>
		protected internal long microsecondLength = UNKNOWN_LENGTH;

		/// <summary>
		/// A special value indicating an unknown quantity.
		/// </summary>
		public const int UNKNOWN_LENGTH = -1; // FIXME is this really -1?

		/// <summary>
		/// Create a MidiFileFormat object from the given parameters.
		/// @param type the MIDI file type (0, 1, or 2)
		/// @param divisionType the MIDI file division type
		/// @param resolution the MIDI file timing resolution
		/// @param bytes the MIDI file size in bytes
		/// @param microseconds the MIDI file length in microseconds
		/// </summary>
		public MidiFileFormat(int type, float divisionType, int resolution, int bytes, long microseconds)
		{
			this.type = type;
			this.divisionType = divisionType;
			this.resolution = resolution;
			this.byteLength = bytes;
			this.microsecondLength = microseconds;
		}

		/// <summary>
		/// Get the MIDI file type (0, 1, or 2).
		/// @return the MIDI file type (0, 1, or 2)
		/// </summary>
		public int GetType()
		{
			return type;
		}

		/// <summary>
		/// Get the file division type.
		/// @return the file divison type
		/// </summary>
		public float GetDivisionType()
		{
			return divisionType;
		}

		/// <summary>
		/// Get the file timing resolution.  If the division type is PPQ, then this
		/// is value represents ticks per beat, otherwise it's ticks per frame (SMPTE).
		/// @return the timing resolution in ticks per beat or ticks per frame
		/// </summary>
		public int GetResolution()
		{
			return resolution;
		}

		/// <summary>
		/// Get the file length in bytes.
		/// @return the file length in bytes or UNKNOWN_LENGTH
		/// </summary>
		public int GetByteLength()
		{
			return byteLength;
		}

		/// <summary>
		/// Get the file length in microseconds.
		/// @return the file length in microseconds or UNKNOWN_LENGTH
		/// </summary>
		public long GetMicrosecondLength()
		{
			return microsecondLength;
		}
		
		public override string ToString()
		{
			return string.Format("Type={0}, DivisionType={1}, Resolution={2}, ByteLength={3}, MicrosecondLength={4}", type, divisionType, resolution, byteLength, microsecondLength);
		}

	}
}