// MidiMessage.java -- base class for MIDI messages.
// Copyright (C) 2005 Free Software Foundation, Inc.

using System;

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
		protected byte[] data;

		/// <summary>
		/// The total length of the MIDI message.
		/// </summary>
		protected int length;

		/// <summary>
		/// MidiMessage contructor.
		/// <param name="data">a valid MIDI message</param>
		/// </summary>
		protected MidiMessage(byte[] data)
		{
			this.data = data;
			this.length = data.Length;
		}

		/// <summary>
		/// Set the complete MIDI message.
		/// <param name="data">The complete MIDI message.</param>
		/// <param name="length">The length of the MIDI message.</param>
		/// <exception cref="InvalidMidiDataException">Thrown when the MIDI message is invalid.</exception>
		/// </summary>
		protected void SetMessage(byte[] data, int length)
		{
			this.data = new byte[length];
			Array.Copy(data, 0, this.data, 0, length);
			this.length = length;
		}

		/// <summary>
		/// Get the MIDI message data.
		/// <returns>an array containing the MIDI message data</returns>
		/// </summary>
		public byte[] GetMessage()
		{
			var copy = new byte[length];
			Array.Copy(data, 0, copy, 0, length);
			return copy;
		}

		/// <summary>
		/// Get the status byte of the MIDI message (as an int)
		/// <returns>the status byte of the MIDI message (as an int), or zero if the message length is zero.</returns>
		/// </summary>
		public int GetStatus()
		{
			if (length > 0) {
				return (data[0] & 0xFF);
			} else {
				return 0;
			}
		}

		/// <summary>
		/// Get the length of the MIDI message.
		/// <returns>the length of the MIDI messsage</returns>
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
			string hex = MidiHelper.ByteArrayToString(data, ",");
			return string.Format("[{0}]", hex);
		}
	}
}