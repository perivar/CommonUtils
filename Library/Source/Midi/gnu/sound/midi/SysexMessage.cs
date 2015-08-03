// SysexMessage.java -- System Exclusive MIDI message.
//   Copyright (C) 2005 Free Software Foundation, Inc.

using System;
using gnu.sound.midi.file; // BinaryReaderBigEndian

namespace gnu.sound.midi
{
	/// A system exclusive MIDI message.
	/// @author Anthony Green (green@redhat.com)
	/// @since 1.3
	public class SysexMessage : MidiMessage
	{
		public const int SYSTEM_EXCLUSIVE = 0xF0;

		public const int SPECIAL_SYSTEM_EXCLUSIVE = 0xF7;

		/// <summary>
		/// Create a default valid system exclusive message.

		/// The official specs don't specify what message is to be
		/// created.  Our implementation creates an empty
		/// system exclusive message.
		/// </summary>
		public SysexMessage() : base(new byte[2])
		{
			data[0] = (byte) SYSTEM_EXCLUSIVE;
			data[1] = (byte) ShortMessage.END_OF_EXCLUSIVE;
		}

		/// <summary>
		/// Create a SysexMessage object.
		/// @param data a complete system exclusive message
		/// </summary>
		protected internal SysexMessage(byte[] data) : base(data)
		{
		}

		/// <summary>
		/// Set the sysex message.  The first data byte (status) must be
		/// 0xF0 or 0xF7.
		/// @param data the message data
		/// @param length the length of the message data
		/// @throws InvalidMidiDataException if the status byte is not 0xF0 or 0xF7
		/// </summary>
		public void SetMessage(byte[] data, int length)
		{
			if (data[0] != SYSTEM_EXCLUSIVE && data[0] != SPECIAL_SYSTEM_EXCLUSIVE)
				throw new InvalidMidiDataException("Sysex message starts with 0x" + data[0].ToString("X4") + " instead of 0xF0 or 0xF7");
			base.SetMessage(data, length);
		}

		/// <summary>
		/// Set the sysex message.  status must be either 0xF0 or 0xF7.
		/// @param status the sysex statys byte (0xF0 or 0xF7)
		/// @param data the message data
		/// @param length the length of the message data
		/// @throws InvalidMidiDataException if status is not 0xF0 or 0xF7
		/// </summary>
		public void SetMessage(int status, sbyte[] data, int length)
		{
			if (status != SYSTEM_EXCLUSIVE && status != SPECIAL_SYSTEM_EXCLUSIVE)
				throw new InvalidMidiDataException("Sysex message starts with 0x" + status.ToString("X4") + " instead of 0xF0 or 0xF7");
			this.data = new byte[length+1];
			this.data[0] = (byte) status;
			
			//Array.Copy(data, 0, this.data, 1, length);
			var unsigned_data = MidiHelper.ConvertSBytes(data);
			Array.Copy(unsigned_data, 0, this.data, 1, length);
			
			this.length = length+1;
		}

		/// <summary>
		/// Get the data for this message, not including the status byte.
		/// @return the message data, not including the status byte
		/// </summary>
		public byte[] GetData()
		{
			var result = new byte[length - 1];
			Array.Copy(data, 1, result, 0, length - 1);
			return result;
		}

		/// <summary>
		/// Create a deep-copy clone of this object.
		/// @see java.lang.Object#clone()
		/// </summary>
		public override object Clone()
		{
			var message = new byte[length];
			Array.Copy(data, 0, message, 0, length);
			return new SysexMessage(message);
		}
	}

}