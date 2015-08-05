// MidiFileWriter.java -- MIDI file writing services
// Copyright (C) 2005 Free Software Foundation, Inc.

using System.IO;

namespace gnu.sound.midi.spi
{

	///
	/// MidiFileWriter provides MIDI file writing services.
	/// 
	/// There are three types of Standard MIDI File (SMF) formats,
	/// represented by integers 0, 1, and 2.
	/// 
	/// Type 0 files contain a single track and represents a single song
	/// performance.
	/// Type 1 may contain multiple tracks for a single song performance.
	/// Type 2 may contain multiple tracks, each representing a
	/// separate song performance.
	/// 
	/// See http://en.wikipedia.org/wiki/MIDI#MIDI_file_formats for more
	/// information.
	/// 
	/// @author Anthony Green (green@redhat.com)
	/// @since 1.3
	public abstract class MidiFileWriter
	{
		/// <summary>
		/// Return the MIDI file types supported by this writer.
		/// <returns>the MIDI file types, or an empty array</returns>
		/// </summary>
		public abstract int[] GetMidiFileTypes();

		/// <summary>
		/// Return the MIDI file types supported by this writer for the
		/// given sequence.
		/// <param name="sequence">the sequence we'd like to write</param>
		/// <returns>the MIDI file types, or an empty array</returns>
		/// </summary>
		public abstract int[] GetMidiFileTypes(Sequence sequence);

		/// <summary>
		/// Returns true if this writer supports the given file type.
		/// <param name="fileType">the file type we're asking about</param>
		/// <returns>true if this writer supports fileType, false otherwise</returns>
		/// </summary>
		public bool IsFileTypeSupported(int fileType)
		{
			int[] types = GetMidiFileTypes();
			for (int i = types.Length; i > 0;)
			{
				if (types[--i] == fileType)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Returns true if this writer supports the given file type for the
		/// given sequence.
		/// <param name="fileType">the file type we're asking about</param>
		/// <param name="sequence">the sequence we'd like to write</param>
		/// <returns>true if this writer supports fileType, false otherwise</returns>
		/// </summary>
		public bool IsFileTypeSupported(int fileType, Sequence sequence)
		{
			int[] types = GetMidiFileTypes(sequence);
			for (int i = types.Length; i > 0;)
			{
				if (types[--i] == fileType)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Write a sequence to a stream using the specified MIDI file type.
		/// <param name="stream">the sequence to write</param>
		/// <param name="fileType">the MIDI file type to use</param>
		/// <param name="out">the output stream to write to</param>
		/// <returns>the number of byte written</returns>
		/// <exception cref="IOException">if an I/O exception happens</exception>
		/// </summary>
		public abstract int Write(Sequence stream, int fileType, Stream @out);

		/// <summary>
		/// Write a sequence to a file using the specified MIDI file type.
		/// <param name="stream">the sequence to write</param>
		/// <param name="fileType">the MIDI file type to use</param>
		/// <param name="out">the file to write to</param>
		/// <returns>the number of byte written</returns>
		/// <exception cref="IOException">if an I/O exception happens</exception>
		/// </summary>
		public abstract int Write(Sequence stream, int fileType, FileInfo @out);

	}

}