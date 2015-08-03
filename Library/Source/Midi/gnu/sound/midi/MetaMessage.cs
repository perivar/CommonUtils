// MetaMessage.java -- A meta message for MIDI files.
//   Copyright (C) 2005 Free Software Foundation, Inc.

using System;
using gnu.sound.midi.info;

namespace gnu.sound.midi
{

	/// A system exclusive MIDI message.
	/// @author Anthony Green (green@redhat.com)
	/// @since 1.3
	public class MetaMessage : MidiMessage
	{
		// enums
		// https://searchcode.com/codesearch/view/28045005/
		public enum MetaEventType : int
		{
			None = int.MinValue,
			Sequence_Number = 0x00,
			Text_Event = 0x01,
			Copyright_Notice = 0x02,
			Sequence_Or_Track_Name = 0x03,
			Instrument_Name = 0x04,
			Lyric_Text = 0x05,
			Marker_Text = 0x06,
			Cue_Point = 0x07,
			Midi_Channel_Prefix_Assignment = 0x20,
			Midi_Port = 0x21,
			End_of_Track = 0x2F,
			Tempo = 0x51,
			Smpte_Offset = 0x54,
			Time_Signature = 0x58,
			Key_Signature = 0x59,
			Sequencer_Specific_Event = 0x7F
		}
		
		/// <summary>
		/// The META status code.  Only valid for MIDI files, not the wire protocol.
		/// </summary>
		public const int META = 0xFF;

		// The length of the variable length data length encoding.
		private int lengthLength = 0;

		/// <summary>
		/// Create a default valid meta message.
		/// The official specs don't specify what message is to be
		/// created.  For now, we create a zero length meta message
		/// with a type code of 0.
		/// </summary>
		public MetaMessage() : base(new byte[4])
		{
			data[0] = (byte) META;
			data[1] = (byte) 0; // Type
			data[2] = (byte) 1; // Length length
			data[3] = (byte) 0; // Length
			lengthLength = 1;
		}

		/// <summary>
		/// Create a MetaMessage object.
		/// @param data a complete system exclusive message
		/// </summary>
		protected internal MetaMessage(byte[] data) : base(data)
		{
			int index = 2;
			lengthLength = 1;
			while ((data[index++] & 0x80) > 0)
				lengthLength++;
		}

		/// <summary>
		/// Set the meta message.
		/// @param type the meta type byte (&gt; 128)
		/// @param data the message data
		/// @param length the length of the message data
		/// @throws InvalidMidiDataException if this message is invalid
		/// </summary>
		public void SetMessage(int type, sbyte[] data, int length)
		{
			if (type > 127)
				throw new InvalidMidiDataException("Meta type 0x" + type.ToString("X4") + " must be less than 128");

			// For a nice description of how variable length values are handled,
			// http://web.archive.org/web/20051129113105/http://www.borg.com/~jglatt/tech/midifile/vari.htm

			// First compute the length of the length value
			lengthLength = 0;
			int lengthValue = length;
			do
			{
				lengthValue = lengthValue >> 7;
				lengthLength++;
			} while (lengthValue > 0);

			// Now allocate our data array
			this.length = 2 + lengthLength + length;
			this.data = new byte[this.length];
			this.data[0] = (byte) META;
			this.data[1] = (byte) type;

			// Now compute the length representation
			int buffer = length & 0x7F; // was long
			while ((length >>= 7) > 0)
			{
				buffer <<= 8;
				buffer |= ((length & 0x7F) | 0x80);
			}

			// Now store the variable length length value
			int index = 2;
			do
			{
				this.data[index++] = (byte)(buffer & 0xFF);
				if ((buffer & 0x80) == 0)
					break;
				buffer >>= 8;
			} while (true);

			// Now copy the real data.
			//Array.Copy(data, 0, this.data, index, length);
			var unsigned_data = MidiHelper.ConvertSBytes(data);
			Array.Copy(unsigned_data, 0, this.data, index, unsigned_data.Length);
		}

		/// <summary>
		/// Get the meta message type.
		/// @return the meta message type
		/// </summary>
		public int GetMetaMessageType()
		{
			return data[1];
		}

		/// <summary>
		/// Get the data for this message, not including the status,
		/// type, or length information.
		/// @return the message data, not including status, type or lenght info
		/// </summary>
		public byte[] GetMetaMessageData()
		{
			int dataLength = length - 2 - lengthLength;
			var result = new byte[dataLength];
			Array.Copy(data, 2 + lengthLength, result, 0, dataLength);
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
			return new MetaMessage(message);
		}
		
		/// <summary>
		/// Return the string representation of this object
		/// </summary>
		/// <returns>the string representation of this object</returns>
		public override string ToString()
		{
			// { type name, length, value string }
			object[] meta = MetaEvent.GetMetaStrings(this);
			string metaStrings = string.Format("{0} '{2}' ({1} bytes)", meta[0], meta[1], meta[2]);
			
			int messageType = GetMetaMessageType();
			string typeName = Enum.GetName(typeof(MetaEventType), messageType);
			
			byte[] messageData = GetMetaMessageData();
			string hex = MidiHelper.ByteArrayToString(messageData, ",");

			return string.Format("{0} [{1}:{2}: {3}]", metaStrings, messageType, typeName, hex);
		}
	}
}