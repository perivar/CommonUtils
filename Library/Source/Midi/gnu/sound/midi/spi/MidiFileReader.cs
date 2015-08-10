// MidiFilerReader -- MIDI file reading services
// Copyright (C) 2005 Free Software Foundation, Inc.

using System.IO;

namespace gnu.sound.midi.spi
{
	///
	/// The MidiFileReader abstract class defines the methods to be provided
	/// by a MIDI file reader.
	/// @author Anthony Green (green@redhat.com)
	/// Modified by Per Ivar Nerseth (perivar@nerseth.com)
	public abstract class MidiFileReader
	{
		/// <summary>
		/// Read a MidiFileFormat from the given stream.
		/// <param name="stream">the stream from which to read the MIDI data</param>
		/// <returns>the MidiFileFormat object</returns>
		/// <exception cref="InvalidMidiDataException">if the stream refers to invalid data</exception>
		/// <exception cref="IOException">if an I/O exception occurs while reading</exception>
		/// </summary>
		public abstract MidiFileFormat GetMidiFileFormat(Stream stream);
		
		/// <summary>
		/// Read a MidiFileFormat from the given stream.
		/// <param name="url">the url from which to read the MIDI data</param>
		/// <returns>the MidiFileFormat object</returns>
		/// <exception cref="InvalidMidiDataException">if the url refers to invalid data</exception>
		/// <exception cref="IOException">if an I/O exception occurs while reading</exception>
		/// </summary>
		public abstract MidiFileFormat GetMidiFileFormat(string url);

		/// <summary>
		/// Read a MidiFileFormat from the given stream.
		/// <param name="file">the file from which to read the MIDI data</param>
		/// <returns>the MidiFileFormat object</returns>
		/// <exception cref="InvalidMidiDataException">if the file refers to invalid data</exception>
		/// <exception cref="IOException">if an I/O exception occurs while reading</exception>
		/// </summary>
		public abstract MidiFileFormat GetMidiFileFormat(FileInfo file);
		
		/// <summary>
		/// Read a Sequence from the given stream.
		/// <param name="stream">the stream from which to read the MIDI data</param>
		/// <returns>the Sequence object</returns>
		/// <exception cref="InvalidMidiDataException">if the stream refers to invalid data</exception>
		/// <exception cref="IOException">if an I/O exception occurs while reading</exception>
		/// </summary>
		public abstract Sequence GetSequence(Stream stream);
		
		/// <summary>
		/// Read a Sequence from the given stream.
		/// <param name="url">the url from which to read the MIDI data</param>
		/// <returns>the Sequence object</returns>
		/// <exception cref="InvalidMidiDataException">if the url refers to invalid data</exception>
		/// <exception cref="IOException">if an I/O exception occurs while reading</exception>
		/// </summary>
		public abstract Sequence GetSequence(string url);
		
		/// <summary>
		/// Read a Sequence from the given stream.
		/// <param name="file">the file from which to read the MIDI data</param>
		/// <returns>the Sequence object</returns>
		/// <exception cref="InvalidMidiDataException">if the file refers to invalid data</exception>
		/// <exception cref="IOException">if an I/O exception occurs while reading</exception>
		/// </summary>
		public abstract Sequence GetSequence(FileInfo file);
	}

}