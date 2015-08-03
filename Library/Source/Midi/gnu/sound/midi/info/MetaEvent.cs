using System;
using System.Collections.Generic;

namespace gnu.sound.midi.info
{
	/// Handle Midi Meta events.
	/// Taken from MidiQuickFix - A Simple Midi file editor and player
	/// Copyright (C) 2004-2009 John Lemcke
	/// jostle@users.sourceforge.net
	public static class MetaEvent
	{
		private static string[] typeNames = { "SEQUENCE_NUMBER", "TEXT", "COPYRIGHT", "TRACK_NAME", "INSTRUMENT", "LYRIC", "MARKER", "CUE_POINT", "PROGRAM_NAME", "DEVICE_NAME", "END_OF_TRACK", "TEMPO", "SMPTE_OFFSET", "TIME_SIGNATURE", "KEY_SIGNATURE", "PROPRIETARY_DATA" };

		// META event types
		public const int SEQUENCE_NUMBER = 0x00; //FF 00 02 ss ss or FF 00 00
		public const int TEXT = 0x01; //FF 01 len TEXT (arbitrary TEXT)
		public const int COPYRIGHT = 0x02; //FF 02 len TEXT
		public const int TRACK_NAME = 0x03; //FF 03 len TEXT
		public const int INSTRUMENT = 0x04; //FF 04 len TEXT
		public const int LYRIC = 0x05; //FF 05 len TEXT
		public const int MARKER = 0x06; //FF 06 len TEXT (e.g. Loop point)
		public const int CUE_POINT = 0x07; //FF 07 len TEXT (e.g..wav file name)
		public const int PROGRAM_NAME = 0x08; //FF 08 len TEXT (PIANO, FLUTE,...)
		public const int DEVICE_NAME = 0x09; //FF 09 len TEXT (MIDI Out 1, MIDI Out 2)
		public const int END_OF_TRACK = 0x2f; //FF 2F 00
		public const int TEMPO = 0x51; //FF 51 03 tt tt tt microseconds
		public const int SMPTE_OFFSET = 0x54; //FF 54 05 hr mn se fr ff

		// nn=numerator, dd=denominator (2^dd), cc=MIDI clocks/metronome click
		// bb=no. of notated 32nd notes per MIDI quarter note.
		// There are 24 MIDI clocks per quarter note.
		// No I don't understand that last one but it will almost certainly be 8.
		// 06 03 24 08 is 6/8 time, 36 clocks/metronome (2 per bar), 8 1/32nd notes / 1/4note
		public const int TIME_SIGNATURE = 0x58; //FF 58 04 nn dd cc bb
		public const int KEY_SIGNATURE = 0x59; //FF 59 02 sf mi
		
		// -sf=no. of flats +sf=no. of sharps mi=0=major mi=1=minor
		public const int PROPRIETARY_DATA = 0x7f; //FF 7F len data
		private static readonly Dictionary<string, int> mTypeNameToValue;

		static MetaEvent()
		{
			mTypeNameToValue = new Dictionary<string, int>(20);
			mTypeNameToValue["SEQUENCE_NUMBER"] = 0x00;
			mTypeNameToValue["TEXT"] = 0x01;
			mTypeNameToValue["COPYRIGHT"] = 0x02;
			mTypeNameToValue["TRACK_NAME"] = 0x03;
			mTypeNameToValue["INSTRUMENT"] = 0x04;
			mTypeNameToValue["LYRIC"] = 0x05;
			mTypeNameToValue["MARKER"] = 0x06;
			mTypeNameToValue["CUE_POINT"] = 0x07;
			mTypeNameToValue["PROGRAM_NAME"] = 0x08;
			mTypeNameToValue["DEVICE_NAME"] = 0x09;
			mTypeNameToValue["END_OF_TRACK"] = 0x2f;
			mTypeNameToValue["TEMPO"] = 0x51;
			mTypeNameToValue["SMPTE_OFFSET"] = 0x54;
			mTypeNameToValue["TIME_SIGNATURE"] = 0x58;
			mTypeNameToValue["KEY_SIGNATURE"] = 0x59;
			mTypeNameToValue["PROPRIETARY_DATA"] = 0x7f;
		}
		
		// Get the list of META event type names
		// @return the list of META event type names
		public static string[] GetTypeNames()
		{
			return typeNames;
		}
		
		// Get the values that represent the given META event.
		// The returned array consists of <ol>
		// <li>the event name as a String</li>
		// <li>the length of the event data as an Integer</li>
		// <li>the event data as a String</li>
		// </ol>
		// @param mess the META message to format
		// @return the representation of the message
		public static object[] GetMetaStrings(MetaMessage mess)
		{
			// Some data is String some is a series of bytes others are neither
			bool dumpText = false;
			bool dumpBytes = false;

			int type = mess.GetMetaMessageType();
			byte[] data = mess.GetMetaMessageData();

			// The returned Object array
			// { type name, length, value string }
			object[] result = { "M:", null, "" };
			result[1] = Convert.ToInt32(data.Length);

			switch (type)
			{
				case SEQUENCE_NUMBER:
					result[0] = "M:SequenceNumber";
					dumpBytes = true;
					break;
				case TEXT:
					result[0] = "M:Text";
					dumpText = true;
					break;
				case COPYRIGHT:
					result[0] = "M:Copyright";
					dumpText = true;
					break;
				case TRACK_NAME:
					result[0] = "M:TrackName";
					dumpText = true;
					break;
				case INSTRUMENT:
					result[0] = "M:Instrument";
					dumpText = true;
					break;
				case LYRIC:
					result[0] = "M:Lyric";
					dumpText = true;
					break;
				case MARKER:
					result[0] = "M:Marker";
					dumpText = true;
					break;
				case CUE_POINT:
					result[0] = "M:CuePoint";
					dumpText = true;
					break;
				case PROGRAM_NAME:
					result[0] = "M:ProgramName";
					dumpText = true;
					break;
				case DEVICE_NAME:
					result[0] = "M:DeviceName";
					dumpText = true;
					break;
				case SMPTE_OFFSET:
					result[0] = "M:SMPTEOffset";
					// Hour, Minute, Second, Frame, Field
					// hr mn se fr ff
					result[2] = string.Format("{0:00}:{1:00}:{2:00}:{3:00}:{4:00}",
					                          data[0] & 0x00ff,
					                          data[1] & 0x00ff,
					                          data[2] & 0x00ff,
					                          data[3] & 0x00ff,
					                          data[4] & 0x00ff);
					break;
				case TIME_SIGNATURE:
					result[0] = "M:TimeSignature";
					int numerator = (data[0] & 0x00ff);
					int denominator = (int)(Math.Pow(2, (data[1] & 0x00ff)));
					int clocksPerClick = (data[2] & 0x00ff);
					int my32ndPer4th = (data[3] & 0x00ff);
					result[2] = numerator + "/" + denominator + " " 
						+ clocksPerClick + " Metr. " 
						+ my32ndPer4th + " N/q";
					break;
				case KEY_SIGNATURE:
					result[0] = "M:KeySignature";
					result[2] = KeySignatures.GetKeyName(data);
					break;
				case TEMPO:
					result[0] = "M:Tempo";
					int bpm = MicroSecsToBpm(data);
					result[2] = string.Format("{0}", bpm);
					break;
				case END_OF_TRACK:
					result[0] = "M:EndOfTrack";
					break;
				case PROPRIETARY_DATA:
					result[0] = "M:ProprietaryData";
					dumpBytes = true;
					break;
				default:
					result[0] = "M:" + type;
					dumpBytes = true;
					break;
			}

			if (dumpText)
			{
				string text = MidiHelper.GetString(data);
				text = text.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
				result[2] = text;
			}

			if (dumpBytes)
			{
				result[2] = MidiHelper.ByteArrayToHexString(data, ",");
			}
			return result;
		}

		// Methods to handle TEMPO events.
		// Convert the given microsecond period to BeatsPerMinute
		// @param data 3 bytes of data that specify the microsecond period.
		// Calculated as <br>
		// <code>data[0] &lt;&lt; 16 + data[1] &lt;&lt; 8 + data[2]</code>
		// @return the BeatsPerMinute equivalent to the given microsecond period
		public static int MicroSecsToBpm(byte[] data)
		{
			// Coerce the bytes into ints
			var ints = new int[3];
			ints[0] = data[0] & 0x00ff;
			ints[1] = data[1] & 0x00ff;
			ints[2] = data[2] & 0x00ff;

			long t = ints[0] << 16;
			t += ints[1] << 8;
			t += ints[2];

			return (int)(60000000 / t);
		}

		// Convert the given BeatsPerMinute to a microsecond period
		// @param bpm the BeatsPerMinute to convert
		// @return 3 bytes of data that specify the microsecond period.
		// Calculated as <br>
		// <code>data[0] &lt;&lt; 16 + data[1] &lt;&lt; 8 + data [2]</code>
		public static byte[] BpmToMicroSecs(int bpm)
		{
			long t = 60000000 / bpm;
			var data = new byte[3];
			data[0] = (byte)((t & 0xff0000) >> 16);
			data[1] = (byte)((t & 0xff00) >> 8);
			data[2] = (byte)((t & 0xff));
			return data;
		}

		// Parse a tempo string with an optional 'bpm' suffix e.g. 88bpm
		// @param tempoString the string to parse
		// @return the integer part of the string or 60 if the string does not
		// represent a valid integer (with optional 'bpm' suffix)
		public static int ParseTempo(string tempoString)
		{
			int bpmPos = tempoString.ToLower().IndexOf("bpm");

			// Default value is 60bpm
			int t = 60;
			if (bpmPos != -1)
			{
				tempoString = tempoString.Substring(0, bpmPos);
			}
			try
			{
				t = Convert.ToInt32(tempoString);
			}
			catch (Exception nfe)
			{
				// DO NOTHING - just use the default
			}
			return t;
		}

		// Parse a time signature string in the format nn[/dd]
		// nn=numerator, dd=denominator
		// If only nn is given then dd defaults to 4
		// @param timeSigString the string to parse
		// @param ticksPerBeat used to calculate the metronome click
		// @return the data for the event in a byte[]
		public static byte[] ParseTimeSignature(string timeSigString, int ticksPerBeat)
		{
			string[] parts = timeSigString.Split(new String [] { "/" } , StringSplitOptions.None) ;
			
			// default to 4/4
			byte[] result = {4, 2, (byte)(ticksPerBeat / 4), 8};
			switch (parts.Length)
			{
				case 0:
					break;
				case 1:
					// Assume beats
					result[0] = SafeParseByte(parts[0]);
					break;
				case 2:
					// Beats per bar / Beat note duration
					result[0] = GetBeatsPerBar(parts[0]);
					result[1] = GetBeatValue(parts[1]);
					result[2] = GetUsefulClocksPerMetronome(parts[0], parts[1]);
					break;
				case 3:
					break;
			}
			return result;
		}

		private static byte GetBeatsPerBar(string beats)
		{
			return SafeParseByte(beats);
		}

		private static byte GetBeatValue(string value)
		{
			byte beatValue = SafeParseByte(value);
			double log2 = Math.Log(beatValue) / Math.Log(2);
			return (byte)Math.Round(log2);
		}

		private static byte GetUsefulClocksPerMetronome(string beats, string value)
		{
			byte beatsPerBar = GetBeatsPerBar(beats);
			byte beatValue = GetBeatValue(value);
			
			// Try to generate a useful metronome click
			// How many MIDI clocks are there in each beat
			// (there are 24 MIDI clocks in a quarter note)
			int clocksPerBeat = (24 * 4) / beatValue;
			int clocksPerMetronome = clocksPerBeat;
			if (beatsPerBar > 0)
			{
				if (beatsPerBar % 4 == 0)
				{
					clocksPerMetronome = clocksPerBeat * beatsPerBar / 4;
				}
				else if (beatsPerBar % 3 == 0)
				{
					clocksPerMetronome = clocksPerBeat * beatsPerBar / 3;
				}
				else if (beatsPerBar % 2 == 0)
				{
					clocksPerMetronome = clocksPerBeat * beatsPerBar / 2;
				}
			}
			return (byte)clocksPerMetronome;
		}
		
		// Parse an SMPTE offset string in the form
		// "hours:minutes:seconds:frames:fields"
		// @param smpteString the string to parse
		// @return a byte array with elements representing
		// hours, minutes, seconds, frames, fields
		public static byte[] ParseSMPTEOffset(string smpteString)
		{
			string[] parts = smpteString.Split(new String [] { ":" } , StringSplitOptions.None) ;
			byte[] result = {0, 0, 0, 0, 0};
			switch (parts.Length)
			{
				case 0:
					break;
				case 1:
					// Assume secs
					result[2] = SafeParseByte(parts[0]);
					break;
				case 2:
					// Assume mins secs
					result[1] = SafeParseByte(parts[0]);
					result[2] = SafeParseByte(parts[1]);
					break;
				case 3:
					// Assume hrs mins secs
					result[0] = SafeParseByte(parts[0]);
					result[1] = SafeParseByte(parts[1]);
					result[2] = SafeParseByte(parts[2]);
					break;
				case 4:
					// Assume hrs mins secs frames
					result[0] = SafeParseByte(parts[0]);
					result[1] = SafeParseByte(parts[1]);
					result[2] = SafeParseByte(parts[2]);
					result[3] = SafeParseByte(parts[3]);
					break;
				case 5:
					// must be hrs mins secs frames fields
					result[0] = SafeParseByte(parts[0]);
					result[1] = SafeParseByte(parts[1]);
					result[2] = SafeParseByte(parts[2]);
					result[3] = SafeParseByte(parts[3]);
					result[4] = SafeParseByte(parts[4]);
					break;
			}
			return result;
		}

		// test if the message data should be treated as a string
		// @param mess the message to test
		// @return <code>true</code> if the message data should be
		// represented as a string
		public static bool IsText(MetaMessage mess)
		{
			int type = mess.GetMetaMessageType();
			return (type >= 1 && type <= 9);
		}

		// test if the message data can be edited in the track editor
		// @param mess the message to test
		// @return <code>true</code> if the message data can be edited
		public static bool IsEditable(MetaMessage mess)
		{
			int type = mess.GetMetaMessageType();
			return ((type >= 1 && type <= 9) || type == TEMPO || type == KEY_SIGNATURE);
		}

		// Update the data content of the message
		// @param mess the message to update
		// @param value a String that represents the data for the event.<br>
		// This is parsed into a <code>byte[]</code> that becomes the data of
		// the MetaMessage.<br>
		// Most events treat the string as a text value and just convert
		// each character to a <code>byte</code> in the array but the
		// following message types are handled specially
		// <dl>
		// <dt>TEMPO</dt>
		// <dd>an integer value with an optional "bpm" suffix. e.g. 120bpm</dd>
		// <dt>SMPTE_OFFSET</dt>
		// <dd>a string in the form <code>h:m:s:f:d</code> where<br>
		// h=hours m=minutes s=seconds f=frames d=fields<br>
		// If fewer than 5 values are given then the parser treats them as<br>
		// 1. <b>s</b><br>
		// 2. <b>m:s</b><br>
		// 3. <b>h:m:s</b><br>
		// 4. <b>h:m:s:f</b><br>
		// 5. <b>h:m:s:f:d</b><br>
		// and the unspecified values are set to zero.
		// </dd>
		// <dt>TIME_SIGNATURE</dt>
		// <dd>a time signature string in the format <code>n[/d]</code> where<br>
		// n=numerator d=denominator<br>
		// If only <code>n</code> is given then <code>d</code> defaults to 4</dd>
		// <dt>KEY_SIGNATURE</dt>
		// <dd>one of the following key signature strings<br>
		// <b>Cb Gb Db Ab Eb Bb F C G D A E B F# C#</b/></dd>
		// <dt>SEQUENCE_NUMBER and PROPRIETARY_DATA</dt>
		// <dd>a space-separated list of values that are parsed into
		// a <code>byte[]</code> using <code>Byte.decode()</code><br>
		// If any value cannot be parsed into a <code>byte</code>
		// then it is treated as zero</dd>
		// </dl>
		// @param ticksPerBeat the tick resolution of the sequence
		public static void SetMetaData(MetaMessage mess, string value, int ticksPerBeat)
		{
			byte[] data;
			int type = mess.GetMetaMessageType();
			if (IsText(mess))
			{
				data = MidiHelper.GetBytes(value);
			}
			else if (type == TEMPO)
			{
				int bpm = ParseTempo(value);
				data = BpmToMicroSecs(bpm);
			}
			else if (type == TIME_SIGNATURE)
			{
				data = ParseTimeSignature(value, ticksPerBeat);
			}
			else if (type == KEY_SIGNATURE)
			{
				data = MidiHelper.ConvertSBytes(KeySignatures.GetKeyValues(value));
			}
			else if (type == SMPTE_OFFSET)
			{
				data = ParseSMPTEOffset(value);
			}
			else
			{
				// treat the string as a space separated list of
				// string representations of byte values
				// Should handle decimal, hexadecimal and octal representations
				// using the java.lang.Byte.decode() method
				string[] strings = value.Split(new String[] { " " }, StringSplitOptions.None );
				
				// TODO: fix this
				int len = strings.Length;
				data = new byte[len];
				for (int i = 0; i < len; ++i)
				{
					data = MidiHelper.GetBytes(strings[i]);
				}
				/*
				int len = strings.Length;
				data = new byte[len];
				for (int i = 0; i < len; ++i)
				{
					//data[i] = byte.decode(strings[i]);
				}
				 */
			}

			if (data != null)
			{
				var sbytes = MidiHelper.ConvertBytes(data);
				mess.SetMessage(type, sbytes, sbytes.Length);
			}
		}

		// Create a Midi Meta event
		// @param type the type of the event as defined by the array returned from getTypeNames()
		// @param data a String that represents the data for the event.<br>
		// see {@see #setMetaData} for details.
		// @param tick the position of the event in the sequence
		// @param ticksPerBeat the tick resolution of the sequence
		// @return the created Midi Meta event
		// @throws javax.sound.midi.InvalidMidiDataException if the MetaMessage.setMessage() parameters are not valid
		public static MidiEvent CreateMetaEvent(string type, string data, long tick, int ticksPerBeat)
		{
			var mm = new MetaMessage();
			mm.SetMessage(mTypeNameToValue[type], null, 0);
			SetMetaData(mm, data, ticksPerBeat);
			var ev = new MidiEvent(mm, tick);
			return ev;
		}

		private static byte SafeParseByte(string s)
		{
			return SafeParseByte(s, (byte)0);
		}

		private static byte SafeParseByte(string s, byte defVal)
		{
			byte t = defVal;
			try {
				t = Convert.ToByte(s);
			} catch (Exception e) {
				// DO NOTHING - just use the default
			}
			return t;
		}
	}
}
