// ShortMessage.java -- A MIDI message no longer than 3 bytes
//   Copyright (C) 2005 Free Software Foundation, Inc.

using System;

namespace gnu.sound.midi
{
	/// A short MIDI message that is no longer than 3 bytes long.
	/// @author Anthony Green (green@redhat.com)
	/// @since 1.3
	public class ShortMessage : MidiMessage
	{
		#region Status Byte Constants
		/// <summary>
		/// Status byte for Time Code message.
		/// </summary>
		public const int MIDI_TIME_CODE = 0xF1;

		/// <summary>
		/// Status byte for Song Position Pointer message.
		/// </summary>
		public const int SONG_POSITION_POINTER = 0xF2;

		/// <summary>
		/// Status byte for Song Select message.
		/// </summary>
		public const int SONG_SELECT = 0xF3;

		/// <summary>
		/// Status byte for Tune Request message.
		/// </summary>
		public const int TUNE_REQUEST = 0xF6;

		/// <summary>
		/// Status byte for End Of Exclusive message.
		/// </summary>
		public const int END_OF_EXCLUSIVE = 0xF7;

		/// <summary>
		/// Status byte for Timing Clock message.
		/// </summary>
		public const int TIMING_CLOCK = 0xF8;

		/// <summary>
		/// Status byte for Start message.
		/// </summary>
		public const int START = 0xFA;

		/// <summary>
		/// Status byte for Continue message.
		/// </summary>
		public const int CONTINUE = 0xFB;

		/// <summary>
		/// Status byte for Stop message.
		/// </summary>
		public const int STOP = 0xFC;

		/// <summary>
		/// Status byte for Active Sensing message.
		/// </summary>
		public const int ACTIVE_SENSING = 0xFE;

		/// <summary>
		/// Status byte for System Reset message.
		/// </summary>
		public const int SYSTEM_RESET = 0xFF;
		#endregion

		// Create and initialize a default, arbitrary message.
		private static byte[] defaultMessage;
		static ShortMessage()
		{
			defaultMessage = new byte[1];
			defaultMessage[0] = (byte) STOP;
		}

		/// <summary>
		/// Create a short MIDI message.
		/// The spec requires that this represent a valid MIDI message, but doesn't
		/// specify what it should be.  We've chosen the STOP message for our
		/// implementation.
		/// </summary>
		public ShortMessage() : this(defaultMessage)
		{
		}

		/// <summary>
		/// Create a short MIDI message.
		/// The data argument should be a valid MIDI message.  Unfortunately the spec
		/// does not allow us to throw an InvalidMidiDataException if data is invalid.
		/// @param data the message data
		/// </summary>
		protected internal ShortMessage(byte[] data) : base(data)
		{
		}

		/// <summary>
		/// Set the MIDI message.
		/// @param status the status byte for this message
		/// @param data1 the first data byte for this message
		/// @param data2 the second data byte for this message
		/// @throws InvalidMidiDataException if status is bad, or data is out of range
		/// </summary>
		public void SetMessage(int status, int data1, int data2)
		{
			length = GetDataLength(status);
			length++;
			if (data == null || data.Length < length)
				data = new byte[length];
			data[0] = (byte) status;
			if (length > 1)
			{
				if (data1 < 0 || data1 > 127)
					throw new InvalidMidiDataException("data1 (" + data1 + ") must be between 0 and 127.");
				data[1] = (byte) data1;
				if (length > 2)
				{
					if (data2 < 0 || data2 > 127)
						throw new InvalidMidiDataException("data2 (" + data2 + ") must be between 0 and 127.");
					data[2] = (byte) data2;
				}
			}
		}

		public void SetMessage(int command, int channel, int data1, int data2)
		{
			// TODO: This could probably stand some error checking.
			// It currently assumes command and channel are valid values.
			SetMessage(command + channel, data1, data2);
		}

		/// <summary>
		/// Set the MIDI message to one that requires no data bytes.
		/// @param status the status byte for this message
		/// @throws InvalidMidiDataException if status is bad, or requires data
		/// </summary>
		public void SetMessage(int status)
		{
			int dataLength = GetDataLength(status);
			if (dataLength != 0)
				throw new InvalidMidiDataException("Status byte 0x" + status.ToString("X4") + " requires " + dataLength + " bytes of data.");
			SetMessage(status, 0, 0);
		}

		/// <summary>
		/// Return the number of data bytes needed for a given MIDI status byte.
		/// @param status the status byte for a short MIDI message
		/// @return the number of data bytes needed for this status byte
		/// @throws InvalidMidiDataException if status is an invalid status byte
		/// </summary>
		protected static int GetDataLength(int status)
		{
			int originalStatus = status;

			if ((status & 0xF0) != 0xF0)
				status &= 0xF0;

			switch (status)
			{
				case (int) MidiHelper.MidiEventType.Note_Off:
				case (int) MidiHelper.MidiEventType.Note_On:
				case (int) MidiHelper.MidiEventType.Note_Aftertouch:
				case (int) MidiHelper.MidiEventType.Controller:
				case (int) MidiHelper.MidiEventType.Pitch_Bend:
				case SONG_POSITION_POINTER:
					return 2;

				case (int) MidiHelper.MidiEventType.Program_Change:
				case (int) MidiHelper.MidiEventType.Channel_Aftertouch:
				case SONG_SELECT:
				case 0xF5: // FIXME: unofficial bus select. Not in spec??
					return 1;

				case TUNE_REQUEST:
				case END_OF_EXCLUSIVE:
				case TIMING_CLOCK:
				case START:
				case CONTINUE:
				case STOP:
				case ACTIVE_SENSING:
				case SYSTEM_RESET:
					return 0;

				default:
					throw new InvalidMidiDataException("Invalid status: 0x" + originalStatus.ToString("X4"));
			}
		}

		/// <summary>
		/// Get the channel information from this MIDI message, assuming it is a
		/// MIDI channel message.
		/// @return the MIDI channel for this message
		/// </summary>
		public int GetChannel()
		{
			return data[0] & 0x0F;
		}

		/// <summary>
		/// Get the command nibble from this MIDI message, assuming it is a MIDI
		/// channel message.
		/// @return the MIDI command for this message
		/// </summary>
		public int GetCommand()
		{
			return data[0] & 0xF0;
		}

		/// <summary>
		/// Get the first data byte from this message, assuming it exists, and
		/// zero otherwise.
		/// @return the first data byte or zero if none exists.
		/// </summary>
		public int GetData1()
		{
			return length > 1 ? data[1] : 0;
		}

		/// <summary>
		/// Get the second data byte from this message, assuming it exists, and
		/// zero otherwise.
		/// @return the second date byte or zero if none exists.
		/// </summary>
		public int GetData2()
		{
			return length > 2 ? data[2] : 0;
		}

		/// <summary>
		/// Create a deep-copy clone of this object.
		/// @see java.lang.Object#clone()
		/// </summary>
		public override object Clone()
		{
			var message = new byte[length];
			Array.Copy(data, 0, message, 0, length);
			return new ShortMessage(message);
		}
		
		/// <summary>
		/// Return the string representation of this object
		/// </summary>
		/// <returns>the string representation of this object</returns>
		public override string ToString()
		{
			int command = GetCommand();
			string commandName = MidiHelper.GetEventTypeString((MidiHelper.MidiEventType)command);
			
			int channel = GetChannel();
			
			byte[] messageData = GetMessage();
			string hex = BitConverter.ToString(messageData).Replace("-", ",");

			/*
			switch(command) {
				case (int) MidiHelper.MidiEventType.None:
				case (int) MidiHelper.MidiEventType.Note_Off:
				case (int) MidiHelper.MidiEventType.Note_On:
				case (int) MidiHelper.MidiEventType.Note_Aftertouch:
				case (int) MidiHelper.MidiEventType.Controller:
				case (int) MidiHelper.MidiEventType.Program_Change:
				case (int) MidiHelper.MidiEventType.Channel_Aftertouch:
				case (int) MidiHelper.MidiEventType.Pitch_Bend:
				case (int) MidiHelper.MidiEventType.System_Exclusive:
				case (int) MidiHelper.MidiEventType.MetaEvent:
					break;
			}
			 */
			return string.Format("[MSG] {0} ({1}) \tCh: {2} = [{3}]", commandName, command, channel, hex);
		}
	}
}