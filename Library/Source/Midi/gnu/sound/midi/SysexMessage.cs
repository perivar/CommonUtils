// SysexMessage.java -- System Exclusive MIDI message.
// Copyright (C) 2005 Free Software Foundation, Inc.

using System;
using gnu.sound.midi.file; // BinaryReaderBigEndian

namespace gnu.sound.midi
{
	/// A system exclusive MIDI message.
	/// @author Anthony Green (green@redhat.com)
	/// @since 1.3
	public class SysexMessage : MidiMessage
	{
		/// <summary>
		/// Create a default valid system exclusive message.

		/// The official specs don't specify what message is to be
		/// created.  Our implementation creates an empty
		/// system exclusive message.
		/// </summary>
		public SysexMessage() : base(new byte[2])
		{
			data[0] = (byte) MidiHelper.MidiEventType.SystemExclusive;
			data[1] = (byte) MidiHelper.MidiEventType.EndOfExclusive;
		}

		/// <summary>
		/// Create a SysexMessage object.
		/// <param name="data">a complete system exclusive message</param>
		/// </summary>
		SysexMessage(byte[] data) : base(data)
		{
		}

		/// <summary>
		/// Set the sysex message.  The first data byte (status) must be 0xF0 or 0xF7.
		/// <param name="data">the message data</param>
		/// <param name="length">the length of the message data</param>
		/// <exception cref="InvalidMidiDataException">if the status byte is not 0xF0 or 0xF7</exception>
		/// </summary>
		public void SetMessage(byte[] data, int length)
		{
			if (data[0] != (byte) MidiHelper.MidiEventType.SystemExclusive && data[0] != (byte) MidiHelper.MidiEventType.EndOfExclusive)
				throw new InvalidMidiDataException("Sysex message starts with 0x" + data[0].ToString("X4") + " instead of 0xF0 or 0xF7");
			base.SetMessage(data, length);
		}

		/// <summary>
		/// Set the sysex message.  status must be either 0xF0 or 0xF7.
		/// <param name="status">the sysex statys byte (0xF0 or 0xF7)</param>
		/// <param name="data">the message data</param>
		/// <param name="length">the length of the message data</param>
		/// <exception cref="InvalidMidiDataException">if status is not 0xF0 or 0xF7</exception>
		/// </summary>
		public void SetMessage(int status, byte[] data, int length)
		{
			if (status != (byte) MidiHelper.MidiEventType.SystemExclusive && status != (byte) MidiHelper.MidiEventType.EndOfExclusive)
				throw new InvalidMidiDataException("Sysex message starts with 0x" + status.ToString("X4") + " instead of 0xF0 or 0xF7");
			this.data = new byte[length+1];
			this.data[0] = (byte) status;
			
			Array.Copy(data, 0, this.data, 1, length);
			this.length = length+1;
		}

		/// <summary>
		/// Get the data for this message, not including the status byte.
		/// <returns>the message data, not including the status byte</returns>
		/// </summary>
		public byte[] GetData()
		{
			var result = new byte[length - 1];
			Array.Copy(data, 1, result, 0, length - 1);
			return result;
		}

		#region ICloneable implementation
		/// <summary>
		/// Create a deep-copy clone of this object.
		/// <see cref="Clone()"/>
		/// </summary>
		public override object Clone()
		{
			var message = new byte[length];
			Array.Copy(data, 0, message, 0, length);
			return new SysexMessage(message);
		}
		#endregion
	}
}