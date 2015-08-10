// MidiDataOutputStream -- adds variable length MIDI ints
// Copyright (C) 2006 Free Software Foundation, Inc.

using System;
using System.IO;

namespace gnu.sound.midi.file
{

	/// MidiDataOutputStream is simply a BinaryWriter with the addition
	/// of special variable length int writing as defined by the MIDI spec.
	/// @author Anthony Green (green@redhat.com)
	public class MidiDataOutputStream : BinaryWriterBigEndian
	{
		object locker = new object();
		
		/// <summary>
		/// Create a MidiDataOutputStream.
		/// </summary>
		public MidiDataOutputStream(Stream os) : base(os)
		{
		}

		/// <summary>
		/// Return the length of a variable length encoded int
		/// without writing it out.
		/// <returns>the length of the encoding</returns>
		/// </summary>
		public static int VariableLengthIntLength(int value)
		{
			int length = 0;
			int buffer = value & 0x7F;
			
			while ((value >>= 7) != 0)
			{
				buffer <<= 8;
				buffer |= ((value & 0x7F) | 0x80);
			}

			while (true)
			{
				length++;
				if ((buffer & 0x80) != 0) {
					buffer = (int)((uint)buffer >> 8);
				} else {
					break;
				}
			}

			return length;
		}
		
		/// <summary>
		/// Write an int encoded in the MIDI-style variable length
		/// encoding format.
		/// </summary>
		public void WriteVariableLengthInt(int value)
		{
			lock (locker) {
				
				int length = 0;
				int buffer = value & 0x7F;
				
				while ((value >>= 7) != 0) {
					buffer <<= 8;
					buffer |= ((value & 0x7F) | 0x80);
				}
				
				while (true) {
					length++;
					
					Write((byte) (buffer & 0xFF));
					
					if ((buffer & 0x80) != 0) {
						buffer = (int)((uint)buffer >> 8);
					} else {
						break;
					}
				}
			}
		}
	}
}