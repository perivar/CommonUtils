// MidiFileFormat -- Information about a MIDI file
// Copyright (C) 2005 Free Software Foundation, Inc.

namespace gnu.sound.midi
{
	/// Describe a MIDI file, including specifics about its type, length and timing.
	/// @author Anthony Green (green@redhat.com)
	/// Modified by Per Ivar Nerseth (perivar@nerseth.com)
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
		int midiFileType;

		/// <summary>
		/// The division type of the MIDI file.
		/// </summary>
		float divisionType;

		/// <summary>
		/// The timing resolution of the MIDI file.
		/// </summary>
		int resolution;

		/// <summary>
		/// The size of the MIDI file in bytes.
		/// </summary>
		int byteLength = UNKNOWN_LENGTH;

		/// <summary>
		/// The length of the MIDI file in microseconds.
		/// </summary>
		long microsecondLength = UNKNOWN_LENGTH;

		/// <summary>
		/// A special value indicating an unknown quantity.
		/// </summary>
		public const int UNKNOWN_LENGTH = -1; // FIXME is this really -1?

		/// <summary>
		/// Create a MidiFileFormat object from the given parameters.
		/// <param name="type">the MIDI file type (0, 1, or 2)</param>
		/// <param name="divisionType">the MIDI file division type</param>
		/// <param name="resolution">the MIDI file timing resolution</param>
		/// <param name="bytes">the MIDI file size in bytes</param>
		/// <param name="microseconds">the MIDI file length in microseconds</param>
		/// </summary>
		public MidiFileFormat(int type, float divisionType, int resolution, int bytes, long microseconds)
		{
			this.midiFileType = type;
			this.divisionType = divisionType;
			this.resolution = resolution;
			this.byteLength = bytes;
			this.microsecondLength = microseconds;
		}

		/// <summary>
		/// Get the MIDI file type (0, 1, or 2).
		/// <returns>the MIDI file type (0, 1, or 2)</returns>
		/// </summary>
		public int MidiFileType {
			get {
				return midiFileType;
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
		/// Get the file length in bytes.
		/// <returns>the file length in bytes or UNKNOWN_LENGTH</returns>
		/// </summary>
		public int ByteLength {
			get {
				return byteLength;
			}
		}

		/// <summary>
		/// Get the file length in microseconds.
		/// <returns>the file length in microseconds or UNKNOWN_LENGTH</returns>
		/// </summary>
		public long MicrosecondLength {
			get {
				return microsecondLength;
			}
		}
		
		public override string ToString()
		{
			return string.Format("Type={0}, DivisionType={1}, Resolution={2}, ByteLength={3}, MicrosecondLength={4}", midiFileType, divisionType, resolution, byteLength, microsecondLength);
		}

	}
}