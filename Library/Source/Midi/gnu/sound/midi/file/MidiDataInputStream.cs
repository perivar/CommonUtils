// MidiDataInputStream.java -- adds variable length MIDI ints
//   Copyright (C) 2006 Free Software Foundation, Inc.

using System;
using System.IO;

namespace gnu.sound.midi.file
{

	/// MidiDataInputStream is simply a BinaryReader with the addition
	/// of special variable length int reading as defined by the MIDI spec.
	/// @author Anthony Green (green@redhat.com)
	public class MidiDataInputStream : BinaryReaderBigEndian
	{
		/// <summary>
		/// Create a MidiDataInputStream.
		/// </summary>
		public MidiDataInputStream(Stream stream) : base(stream)
		{
		}

		/// <summary>
		/// Read an int encoded in the MIDI-style variable length
		/// encoding format.
		/// @return an int
		/// </summary>
		public int ReadVariableLengthInt()
		{
			int c = 0, value = ReadByte();

			if ((value & 0x80) != 0)
			{
				value &= 0x7F;
				do
				{
					value = (value << 7) + ((c = ReadByte()) & 0x7F);
				} while ((c & 0x80) != 0);
			}

			return value;
		}
	}

}