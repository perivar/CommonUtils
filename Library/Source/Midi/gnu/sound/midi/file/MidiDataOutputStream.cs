// MidiDataOutputStream.java -- adds variable length MIDI ints
//   Copyright (C) 2006 Free Software Foundation, Inc.
/// </summary>
//This file is part of GNU Classpath.
/// </summary>
//GNU Classpath is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 2, or (at your option)
//any later version.
/// </summary>
//GNU Classpath is distributed in the hope that it will be useful, but
//WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//General Public License for more details.
/// </summary>
//You should have received a copy of the GNU General Public License
//along with GNU Classpath; see the file COPYING.  If not, write to the
//Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
//02110-1301 USA.
/// </summary>
//Linking this library statically or dynamically with other modules is
//making a combined work based on this library.  Thus, the terms and
//conditions of the GNU General Public License cover the whole
//combination.
/// </summary>
//As a special exception, the copyright holders of this library give you
//permission to link this library with independent modules to produce an
//executable, regardless of the license terms of these independent
//modules, and to copy and distribute the resulting executable under
//terms of your choice, provided that you also meet, for each linked
//independent module, the terms and conditions of the license of that
//module.  An independent module is a module which is not derived from
//or based on this library.  If you modify this library, you may extend
//this exception to your version of the library, but you are not
//obligated to do so.  If you do not wish to do so, delete this
//exception statement from your version.

using System.IO;
using System.Runtime.CompilerServices;

namespace gnu.sound.midi.file
{

	/// MidiDataOutputStream is simply a BinaryWriter with the addition
	/// of special variable length int writing as defined by the MIDI spec.
	/// @author Anthony Green (green@redhat.com)
	public class MidiDataOutputStream : BinaryWriterBigEndian
	{
		/// <summary>
		/// Create a MidiDataOutputStream.
		/// </summary>
		public MidiDataOutputStream(Stream os) : base(os)
		{
		}

		/// <summary>
		/// Return the length of a variable length encoded int without
		/// writing it out.
		/// @return the length of the encoding
		/// </summary>
		public int VariableLengthIntLength(int value)
		{
			int length = 0;
			int buffer = value & 0x7F;
			//int buffer = value;

			while ((value >>= 7) != 0)
			{
				buffer <<= 8;
				buffer |= ((value & 0x7F) | 0x80);
				//buffer |= ((value) | 0x80);
			}

			while (true)
			{
				length++;
				if ((buffer & 0x80) != 0)
					buffer = (int) ((uint)buffer>> 8);
				else
					break;
			}

			return length;
		}

		/// <summary>
		/// Write an int encoded in the MIDI-style variable length
		/// encoding format.
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void WriteVariableLengthInt(int value)
		{
			int buffer = value & 0x7F;
			//int buffer = value;

			while ((value >>= 7) != 0)
			{
				buffer <<= 8;
				buffer |= ((value & 0x7F) | 0x80);
				//buffer |= ((value) | 0x80);
			}

			while (true)
			{
				//Write(buffer & 0xff);
				Write(buffer);
				if ((buffer & 0x80) != 0)
					
					buffer = (int) ((uint)buffer>> 8);
				else
					break;
			}
		}
	}

}