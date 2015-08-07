// MetaMessage.java -- A meta message for MIDI files.
// Copyright (C) 2005 Free Software Foundation, Inc.

using System;
using gnu.sound.midi.info;

namespace gnu.sound.midi
{
	/// A system exclusive MIDI message.
	/// @author Anthony Green (green@redhat.com)
	/// @since 1.3
	public class MetaMessage : MidiMessage
	{	
		/// <summary>
		/// The META status code.  Only valid for MIDI files, not the wire protocol.
		/// </summary>
		public const int META = 0xFF;

		// The length of the variable length data length encoding.
		private int lengthByteLength = 0;

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
			lengthByteLength = 1;
		}

		/// <summary>
		/// Create a MetaMessage object.
		/// <param name="data">a complete system exclusive message</param>
		/// </summary>
		MetaMessage(byte[] data) : base(data)
		{
			int index = 2;
			lengthByteLength = 1;
			while ((data[index++] & 0x80) > 0) {
				lengthByteLength++;
			}
		}

		/// <summary>
		/// Set the meta message.
		/// <param name="type">the meta type byte (&gt; 128)</param>
		/// <param name="data">the message data</param>
		/// <param name="length">the length of the message data</param>
		/// <exception cref="InvalidMidiDataException">if this message is invalid</exception>
		/// </summary>
		public void SetMessage(int type, byte[] data, int length)
		{
			if (type > 127) {
				throw new InvalidMidiDataException("Meta type 0x" + type.ToString("X4") + " must be less than 128");
			}

			// For a nice description of how variable length values are handled,
			// http://web.archive.org/web/20051129113105/http://www.borg.com/~jglatt/tech/midifile/vari.htm
			
			// First compute the length of the length value, i.e. how many bytes do we need to store this length value
			lengthByteLength = 0;
			int lengthValue = length;
			do
			{
				lengthValue = lengthValue >> 7;
				lengthByteLength++;
			} while (lengthValue > 0);
			
			// Now allocate our data array
			this.length = 2 + lengthByteLength + length;
			this.data = new byte[this.length];
			this.data[0] = (byte) META;
			this.data[1] = (byte) type;
			
			// Now compute the length representation
			int buffer = length & 0x7F;
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
				if ((buffer & 0x80) == 0) {
					break;
				}
				buffer >>= 8;
			} while (true);
			
			// Now copy the real data.
			if (data != null) {
				Array.Copy(data, 0, this.data, index, data.Length); // the original Java method used 'length' and not 'data.Length', but this doesn't work
			}
		}

		/// <summary>
		/// Get the meta message type (as an int) 
		/// <returns>the meta message type (as an int)</returns>
		/// </summary>
		public int GetMetaMessageType()
		{
			return data[1];
		}

		/// <summary>
		/// Get the data for this message, not including the status,
		/// type, or length information.
		/// <returns>the message data, not including status, type or lenght info</returns>
		/// </summary>
		public byte[] GetMetaMessageData()
		{
			int dataLength = length - 2 - lengthByteLength;
			var result = new byte[dataLength];
			Array.Copy(data, 2 + lengthByteLength, result, 0, dataLength);
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
			return new MetaMessage(message);
		}
		#endregion
		
		/// <summary>
		/// Return the string representation of this object
		/// </summary>
		/// <returns>the string representation of this object</returns>
		public override string ToString()
		{
			// GetMetaStrings: { type name, length, value string }
			object[] meta = MetaEvent.GetMetaStrings(this);
			string metaStrings = string.Format("{0} '{2}' ({1} bytes)", meta[0], meta[1], meta[2]);
			
			int messageType = GetMetaMessageType();
			string typeName = Enum.GetName(typeof(MidiHelper.MetaEventType), messageType);
			
			byte[] messageData = GetMetaMessageData();
			string hex = MidiHelper.ByteArrayToString(messageData, ",");

			//return string.Format("{0} [{1}:{2}: {3}]", metaStrings, messageType, typeName, hex);
			return string.Format("{0}", metaStrings);
		}
	}
}