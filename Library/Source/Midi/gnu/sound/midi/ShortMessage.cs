// ShortMessage.java -- A MIDI message no longer than 3 bytes
// Copyright (C) 2005 Free Software Foundation, Inc.

using System;
using gnu.sound.midi.info;

namespace gnu.sound.midi
{
	/// A short MIDI message that is no longer than 3 bytes long.
	/// @author Anthony Green (green@redhat.com)
	/// @since 1.3
	public class ShortMessage : MidiMessage
	{
		// Create and initialize a default, arbitrary message.
		private static byte[] defaultMessage;
		static ShortMessage()
		{
			defaultMessage = new byte[1];
			defaultMessage[0] = (byte) MidiHelper.MidiEventType.Stop;
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
		/// <param name="data">the message data</param>
		/// </summary>
		ShortMessage(byte[] data) : base(data)
		{
		}

		/// <summary>
		/// Set the MIDI message.
		/// <param name="status">the status byte for this message</param>
		/// <param name="data1">the first data byte for this message</param>
		/// <param name="data2">the second data byte for this message</param>
		/// <exception cref="InvalidMidiDataException">if status is bad, or data is out of range</exception>
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
		/// <param name="status">the status byte for this message</param>
		/// <exception cref="InvalidMidiDataException">if status is bad, or requires data</exception>
		/// </summary>
		public void SetMessage(int status)
		{
			int dataLength = GetDataLength(status);
			if (dataLength != 0) {
				throw new InvalidMidiDataException("Status byte 0x" + status.ToString("X4") + " requires " + dataLength + " bytes of data.");
			}
			SetMessage(status, 0, 0);
		}

		/// <summary>
		/// Return the number of data bytes needed for a given MIDI status byte.
		/// <param name="status">the status byte for a short MIDI message</param>
		/// <returns>the number of data bytes needed for this status byte</returns>
		/// <exception cref="InvalidMidiDataException">if status is an invalid status byte</exception>
		/// </summary>
		protected static int GetDataLength(int status)
		{
			int originalStatus = status;

			if ((status & 0xF0) != 0xF0) {
				status &= 0xF0;
			}

			switch (status)
			{
				case (int) MidiHelper.MidiEventType.NoteOff:
				case (int) MidiHelper.MidiEventType.NoteOn:
				case (int) MidiHelper.MidiEventType.AfterTouchPoly:
				case (int) MidiHelper.MidiEventType.ControlChange:
				case (int) MidiHelper.MidiEventType.PitchBend:
				case (int) MidiHelper.MidiEventType.SongPosition:
					return 2;

				case (int) MidiHelper.MidiEventType.ProgramChange:
				case (int) MidiHelper.MidiEventType.AfterTouchChannel:
				case (int) MidiHelper.MidiEventType.SongSelect:
				case 0xF5: // FIXME: unofficial bus select. Not in spec??
					return 1;

				case (int) MidiHelper.MidiEventType.TuneRequest:
				case (int) MidiHelper.MidiEventType.EndOfExclusive:
				case (int) MidiHelper.MidiEventType.Clock:
				case (int) MidiHelper.MidiEventType.Start:
				case (int) MidiHelper.MidiEventType.Continue:
				case (int) MidiHelper.MidiEventType.Stop:
				case (int) MidiHelper.MidiEventType.ActiveSensing:
				case (int) MidiHelper.MidiEventType.SystemReset:
					return 0;

				default:
					throw new InvalidMidiDataException("Invalid status: 0x" + originalStatus.ToString("X4"));
			}
		}

		/// <summary>
		/// Get the channel information from this MIDI message, assuming it is a
		/// MIDI channel message.
		/// <returns>the MIDI channel for this message</returns>
		/// </summary>
		public int GetChannel()
		{
			return data[0] & 0x0F;
		}

		/// <summary>
		/// Get the command nibble from this MIDI message, assuming it is a MIDI
		/// channel message.
		/// <returns>the MIDI command for this message</returns>
		/// </summary>
		public int GetCommand()
		{
			return data[0] & 0xF0;
		}

		/// <summary>
		/// Get the first data byte from this message, assuming it exists, and
		/// zero otherwise.
		/// <returns>the first data byte or zero if none exists.</returns>
		/// </summary>
		public int GetData1()
		{
			return length > 1 ? data[1] : 0;
		}

		/// <summary>
		/// Get the second data byte from this message, assuming it exists, and
		/// zero otherwise.
		/// <returns>the second date byte or zero if none exists.</returns>
		/// </summary>
		public int GetData2()
		{
			return length > 2 ? data[2] : 0;
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
			return new ShortMessage(message);
		}
		#endregion
		
		/// <summary>
		/// Return the string representation of this object
		/// </summary>
		/// <returns>the string representation of this object</returns>
		public override string ToString()
		{
			// Event, Note, Value, Patch, Text, Channel
			object[] meta = ShortEvent.GetShortStrings(this, false);
			string metaStrings = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", meta[0], meta[1], meta[2], meta[3], meta[4], meta[5]);
			
			int command = GetCommand();
			string commandName = MidiHelper.GetEventTypeString((MidiHelper.MidiEventType)command);
			
			int channel = GetChannel();
			
			byte[] messageData = GetMessage();
			string hex = MidiHelper.ByteArrayToString(messageData, ",");

			return string.Format("{0} [{1}:{2}: {3}]", metaStrings, command, commandName, hex);
		}
	}
}