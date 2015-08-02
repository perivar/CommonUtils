// MidiMessage.java -- base class for MIDI messages.
//   Copyright (C) 2005 Free Software Foundation, Inc.

using System;
using gnu.sound.midi.file;

namespace gnu.sound.midi
{

	/// The base class for all MIDI messages.
	/// @author Anthony Green (green@redhat.com)
	/// @since 1.3
	public abstract class MidiMessage : ICloneable
	{
		/// <summary>
		/// MIDI message data.
		/// </summary>
		protected internal byte[] data;

		/// <summary>
		/// The total length of the MIDI message.
		/// </summary>
		protected internal int length;

		/// <summary>
		/// MidiMessage contructor.
		/// @param data a valid MIDI message
		/// </summary>
		protected internal MidiMessage(byte[] data)
		{
			this.data = data;
			this.length = data.Length;
		}

		/// <summary>
		/// Set the complete MIDI message.
		/// @param data The complete MIDI message.
		/// @param length The length of the MIDI message.
		/// @throws InvalidMidiDataException Thrown when the MIDI message is invalid.
		/// </summary>
		protected internal void SetMessage(byte[] data, int length)
		{
			this.data = new byte[length];
			Array.Copy(data, 0, this.data, 0, length);
			this.length = length;
		}

		/// <summary>
		/// Get the MIDI message data.
		/// @return an array containing the MIDI message data
		/// </summary>
		public byte[] GetMessage()
		{
			var copy = new byte[length];
			Array.Copy(data, 0, copy, 0, length);
			return copy;
		}

		/// <summary>
		/// Get the status byte of the MIDI message (as an int)
		/// @return the status byte of the MIDI message (as an int), or zero if the message length is zero.
		/// </summary>
		public int GetStatus()
		{
			if (length > 0)
				return (data[0] & 0xff);
			else
				return 0;
		}

		/// <summary>
		/// Get the length of the MIDI message.
		/// @return the length of the MIDI messsage
		/// </summary>
		public int Length
		{
			get
			{
				return length;
			}
		}

		#region ICloneable implementation
		/// <summary>
		/// Create a clone of this object.s
		/// </summary>
		/// <returns></returns>
		public abstract object Clone();
		#endregion
		
		/// <summary>
		/// Return the string representation of this object
		/// </summary>
		/// <returns>the string representation of this object</returns>
		public override string ToString()
		{
			string hex = BitConverter.ToString(data).Replace("-", ",");
			return string.Format("[{0}]", hex);
		}
	}

}