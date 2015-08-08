using System;

namespace gnu.sound.midi.info
{
	/// Handle Midi Meta events.
	/// Taken from MidiQuickFix - A Simple Midi file editor and player
	/// Copyright (C) 2004-2009 John Lemcke
	/// jostle@users.sourceforge.net
	public static class MetaEvent
	{
		// A good MIDI Files Specification: http://www.somascape.org/midi/tech/mfile.html
		
		/// <summary>
		/// Get the list of META event type names
		/// </summary>
		/// <returns>the list of META event type names</returns>
		public static string[] GetTypeNames()
		{
			return  Enum.GetNames(typeof(MidiHelper.MetaEventType));
		}
		
		/// <summary>
		/// Get the values that represent the given META event.
		/// The returned array consists of <ol>
		/// <li>the event name as a String</li>
		/// <li>the length of the event data as an Integer</li>
		/// <li>the event data as a String</li>
		/// </ol>
		/// </summary>
		/// <param name="mess">the META message to format</param>
		/// <returns>the representation of the message</returns>
		public static object[] GetMetaStrings(this MetaMessage mess)
		{
			// Some data is String some is a series of bytes others are neither
			bool dumpText = false;
			bool dumpBytes = false;

			int type = mess.GetMetaMessageType();
			byte[] data = mess.GetMetaMessageData();

			// The returned Object array
			// { type name, length, value string }
			object[] result = { "M:", null, "" };
			result[1] = data.Length;

			switch (type)
			{
				case (int) MidiHelper.MetaEventType.SequenceNumber:
					result[0] = "M:SequenceNumber";
					dumpBytes = true;
					break;
				case (int) MidiHelper.MetaEventType.TextEvent:
					result[0] = "M:TextEvent";
					dumpText = true;
					break;
				case (int) MidiHelper.MetaEventType.CopyrightNotice:
					result[0] = "M:CopyrightNotice";
					dumpText = true;
					break;
				case (int) MidiHelper.MetaEventType.SequenceOrTrackName:
					result[0] = "M:SequenceOrTrackName";
					dumpText = true;
					break;
				case (int) MidiHelper.MetaEventType.InstrumentName:
					result[0] = "M:InstrumentName";
					dumpText = true;
					break;
				case (int) MidiHelper.MetaEventType.LyricText:
					result[0] = "M:LyricText";
					dumpText = true;
					break;
				case (int) MidiHelper.MetaEventType.MarkerText:
					result[0] = "M:MarkerText";
					dumpText = true;
					break;
				case (int) MidiHelper.MetaEventType.CuePoint:
					result[0] = "M:CuePoint";
					dumpText = true;
					break;
				case (int) MidiHelper.MetaEventType.ProgramName:
					result[0] = "M:ProgramName";
					dumpText = true;
					break;
				case (int) MidiHelper.MetaEventType.DeviceName:
					result[0] = "M:DeviceName";
					dumpText = true;
					break;
				case (int) MidiHelper.MetaEventType.SmpteOffset:
					result[0] = "M:SmpteOffset";
					// Hour, Minute, Second, Frame, Field
					// hr mn se fr ff
					result[2] = string.Format("{0:00}:{1:00}:{2:00}:{3:00}:{4:00}",
					                          data[0] & 0x00ff,
					                          data[1] & 0x00ff,
					                          data[2] & 0x00ff,
					                          data[3] & 0x00ff,
					                          data[4] & 0x00ff);
					break;
				case (int) MidiHelper.MetaEventType.TimeSignature:
					result[0] = "M:TimeSignature";
					int nn = (data[0] & 0x00ff); // numerator
					int dd = (int)(Math.Pow(2, (data[1] & 0x00ff))); // denominator
					int cc = (data[2] & 0x00ff); // midiClocksPerMetronomeClick
					int bb = (data[3] & 0x00ff); // notated 32nd-notes in a MIDI quarter-note
					result[2] = nn + "/" + dd + ", "
						+ cc + " clicks per metronome, "
						+ bb + " 32nd notes per quarter-note";
					break;
				case (int) MidiHelper.MetaEventType.KeySignature:
					result[0] = "M:KeySignature";
					result[2] = KeySignatures.GetKeyName(data);
					break;
				case (int) MidiHelper.MetaEventType.Tempo:
					result[0] = "M:Tempo";
					int bpm = MicroSecsToBpm(data);
					result[2] = string.Format("{0}", bpm);
					break;
				case (int) MidiHelper.MetaEventType.EndOfTrack:
					result[0] = "M:EndOfTrack";
					break;
				case (int) MidiHelper.MetaEventType.SequencerSpecificEvent:
					result[0] = "M:SequencerSpecificEvent";
					dumpBytes = true;
					break;
				default:
					result[0] = "M:" + type;
					dumpBytes = true;
					break;
			}

			if (dumpText)
			{
				result[2] = MidiHelper.GetStringWithoutNewlines(data);
			}

			if (dumpBytes)
			{
				result[2] = MidiHelper.ByteArrayToHexString(data, ",");
			}
			return result;
		}

		/// <summary>
		/// Methods to handle TEMPO events.
		/// Convert the given microsecond period to BeatsPerMinute
		/// </summary>
		/// <param name="data">3 bytes of data that specify the microsecond period.
		/// Calculated as <br>
		/// <code>data[0] &lt;&lt; 16 + data[1] &lt;&lt; 8 + data[2]</code></param>
		/// <returns>the BeatsPerMinute equivalent to the given microsecond period</returns>
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

			return (int)(MidiHelper.MICRO_SECONDS_PER_MINUTE / t);
		}

		/// <summary>
		/// Convert the given BeatsPerMinute to a microsecond period
		/// </summary>
		/// <param name="bpm">the BeatsPerMinute to convert</param>
		/// <returns>3 bytes of data that specify the microsecond period.
		/// Calculated as <br>
		/// <code>data[0] &lt;&lt; 16 + data[1] &lt;&lt; 8 + data [2]</code></returns>
		public static byte[] BpmToMicroSecs(int bpm)
		{
			long t = MidiHelper.MICRO_SECONDS_PER_MINUTE / bpm;
			var data = new byte[3];
			data[0] = (byte)((t & 0xff0000) >> 16);
			data[1] = (byte)((t & 0xff00) >> 8);
			data[2] = (byte)((t & 0xff));
			return data;
		}

		/// <summary>
		/// Parse a tempo string with an optional 'bpm' suffix e.g. 88bpm
		/// </summary>
		/// <param name="tempoString">the string to parse</param>
		/// <returns>the integer part of the string or 60 if the string does not
		/// represent a valid integer (with optional 'bpm' suffix)</returns>
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
			catch (Exception)
			{
				// DO NOTHING - just use the default
			}
			return t;
		}

		/// <summary>
		/// Parse a time signature string in the format nn[/dd]
		/// nn=numerator, dd=denominator
		/// If only nn is given then dd defaults to 4
		/// </summary>
		/// <param name="timeSigString">the string to parse</param>
		/// <param name="ticksPerBeat">used to calculate the metronome click</param>
		/// <returns>the data for the event in a byte[]</returns>
		public static byte[] ParseTimeSignature(string timeSigString, int ticksPerBeat)
		{
			string[] parts = timeSigString.Split(new String [] { "/", "," } , StringSplitOptions.None) ;
			
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
					result[2] = GetUsefulClocksPerMetronome(parts[0], parts[1], ticksPerBeat);
					break;
				default:
					result[0] = GetBeatsPerBar(parts[0]);
					result[1] = GetBeatValue(parts[1]);
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

		private static byte GetUsefulClocksPerMetronome(string beats, string value, int resolution)
		{
			// Examples
			// A time signature of 4/4, with a metronome click every 1/4 note, would be encoded :
			// FF 58 04 04 02 18 08
			// There are 24 MIDI Clocks per quarter-note, hence cc=24 (0x18).
			// 
			// A time signature of 6/8, with a metronome click every 3rd 1/8 note, would be encoded :
			// FF 58 04 06 03 24 08
			// Remember, a 1/4 note is 24 MIDI Clocks, therefore a bar of 6/8 is 72 MIDI Clocks.
			
			// 96 / 4 x 1 = 24	(0x18 = 24)
			// 96 / 8 x 3 = 36 	(0x24 = 36)
			
			// Time Signature: 2 / 4
			// The lower numeral indicates the note value that represents one beat (the beat unit).
			// The upper numeral indicates how many such beats there are grouped together in a bar.
			
			byte beatsPerBar = GetBeatsPerBar(beats); 	// e.g. 6	nn is a byte specifying the numerator of the time signature (as notated).
			byte beatValue = GetBeatValue(value);		// 			dd is a byte specifying the denominator of the time signature as a negative power of 2.
			byte oneBar = SafeParseByte(value);			// e.g. 8
			
			resolution = 96;
			
			// Try to generate a useful metronome click
			// How many MIDI clocks are there in each beat
			int midiClocksPerBeat = resolution / oneBar;
			int clocksPerMetronome = midiClocksPerBeat;
			if (beatsPerBar > 0) {
				if (beatsPerBar % 4 == 0) {
					clocksPerMetronome = midiClocksPerBeat * 1;
				} else if (beatsPerBar % 3 == 0) {
					clocksPerMetronome = midiClocksPerBeat * 3;
				}
			}
			
			// cc is a byte specifying the number of MIDI clocks between metronome clicks.
			return (byte)clocksPerMetronome;
		}
		
		/// <summary>
		/// Parse an SMPTE offset string in the form
		/// "hours:minutes:seconds:frames:fields"
		/// </summary>
		/// <param name="smpteString">the string to parse</param>
		/// <returns>a byte array with elements representing
		///  hours, minutes, seconds, frames, fields</returns>
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

		/// <summary>
		/// Test if the message data should be treated as a string
		/// </summary>
		/// <param name="mess">the message to test</param>
		/// <returns><code>true</code> if the message data should be represented as a string</returns>
		public static bool IsText(MetaMessage mess)
		{
			int type = mess.GetMetaMessageType();
			return (type >= 1 && type <= 9);
		}

		/// <summary>
		/// Test if the message data can be edited in the track editor
		/// </summary>
		/// <param name="mess">the message to test</param>
		/// <returns><code>true</code> if the message data can be edited</returns>
		public static bool IsEditable(MetaMessage mess)
		{
			int type = mess.GetMetaMessageType();
			return ((type >= 1 && type <= 9) || type == (int) MidiHelper.MetaEventType.Tempo || type == (int) MidiHelper.MetaEventType.KeySignature);
		}
		
		/// <summary>
		/// Update the data content of the message
		/// This is parsed into a <code>byte[]</code> that becomes the data of
		/// the MetaMessage.<br>
		/// Most events treat the string as a text value and just convert
		/// each character to a <code>byte</code> in the array but the
		/// following message types are handled specially
		/// <dl>
		/// <dt>Tempo</dt>
		/// <dd>an integer value with an optional "bpm" suffix. e.g. 120bpm</dd>
		/// <dt>SmpteOffset</dt>
		/// <dd>a string in the form <code>h:m:s:f:d</code> where<br>
		/// h=hours m=minutes s=seconds f=frames d=fields<br>
		/// If fewer than 5 values are given then the parser treats them as<br>
		/// 1. <b>s</b><br>
		/// 2. <b>m:s</b><br>
		/// 3. <b>h:m:s</b><br>
		/// 4. <b>h:m:s:f</b><br>
		/// 5. <b>h:m:s:f:d</b><br>
		/// and the unspecified values are set to zero.
		/// </dd>
		/// <dt>TimeSignature</dt>
		/// <dd>a time signature string in the format <code>n[/d]</code> where<br>
		/// n=numerator d=denominator<br>
		/// If only <code>n</code> is given then <code>d</code> defaults to 4</dd>
		/// <dt>KeySignature</dt>
		/// <dd>one of the following key signature strings<br>
		/// <b>Cb Gb Db Ab Eb Bb F C G D A E B F# C#</b/></dd>
		/// <dt>SequenceNumber and SequencerSpecificEvent</dt>
		/// <dd>a space-separated list of values that are parsed into
		/// a <code>byte[]</code> using <code>Convert.ToByte()</code><br>
		/// If any value cannot be parsed into a <code>byte</code>
		/// then it is treated as zero</dd>
		/// </dl
		/// </summary>
		/// <param name="mess">the message to update</param>
		/// <param name="value">a String that represents the data for the event.<br></param>
		/// <param name="ticksPerBeat">the tick resolution of the sequence</param>
		public static void SetMetaData(MetaMessage mess, string value, int ticksPerBeat)
		{
			byte[] data;
			int type = mess.GetMetaMessageType();
			if (IsText(mess))
			{
				data = MidiHelper.GetBytes(value);
			}
			else if (type == (int) MidiHelper.MetaEventType.Tempo)
			{
				int bpm = ParseTempo(value);
				data = BpmToMicroSecs(bpm);
			}
			else if (type == (int) MidiHelper.MetaEventType.TimeSignature)
			{
				data = ParseTimeSignature(value, ticksPerBeat);
			}
			else if (type == (int) MidiHelper.MetaEventType.KeySignature)
			{
				data = MidiHelper.ConvertSBytes(KeySignatures.GetKeyValues(value));
			}
			else if (type == (int) MidiHelper.MetaEventType.SmpteOffset)
			{
				data = ParseSMPTEOffset(value);
			}
			else
			{
				// treat the string as a space separated list of
				// string representations of byte values
				// TODO: Should handle decimal, hexadecimal and octal representations
				// originally using the java.lang.Byte.decode() method
				data = MidiHelper.StringToByteArray(value, " ");
			}

			if (data != null)
			{
				mess.SetMessage(type, data, data.Length);
			}
		}
		
		/// <summary>
		/// Create a Midi Meta event
		/// </summary>
		/// <param name="type">the type of the event as defined by the array returned from GetTypeNames()</param>
		/// <param name="data">a String that represents the data for the event.</param>
		/// <see cref="SetMetaData"/> for details.
		/// <param name="tick">the position of the event in the sequence</param>
		/// <param name="ticksPerBeat">the tick resolution of the sequence</param>
		/// <returns>the created Midi Meta event</returns>
		public static MidiEvent CreateMetaEvent(string type, string data, long tick, int ticksPerBeat)
		{
			var mm = new MetaMessage();
			
			MidiHelper.MetaEventType e = MidiHelper.MetaEventType.None;
			if ( MidiHelper.TryParse(type, true, ref e) )
			{
				mm.SetMessage((int)e, null, 0);
				SetMetaData(mm, data, ticksPerBeat);
			} else {
				throw new InvalidProgramException(string.Format("Could not parse the passed meta event {0}", type));
			}
			
			var ev = new MidiEvent(mm, tick);
			return ev;
		}

		/// <summary>
		/// Create a Midi Meta event
		/// </summary>
		/// <param name="type">the type of the event as an int</param>
		/// <param name="data">a String that represents the data for the event.</param>
		/// <see cref="SetMetaData"/> for details.
		/// <param name="tick">the position of the event in the sequence</param>
		/// <param name="ticksPerBeat">the tick resolution of the sequence</param>
		/// <returns>the created Midi Meta event</returns>
		public static MidiEvent CreateMetaEvent(int type, string data, long tick, int ticksPerBeat)
		{
			var mm = new MetaMessage();
			mm.SetMessage(type, null, 0);
			SetMetaData(mm, data, ticksPerBeat);
			var ev = new MidiEvent(mm, tick);
			return ev;
		}
		
		/// <summary>
		/// Create a C# code line for this meta event
		/// </summary>
		/// <param name="mess">the META message to process</param>
		/// <param name="tick">the position of the event in the sequence</param>
		/// <param name="ticksPerBeat">the tick resolution of the sequence</param>
		/// <returns>a C# code line</returns>
		public static string CreateMetaEventGeneratedCode(this MetaMessage mess, long tick, int ticksPerBeat) {
			
			int type = mess.GetMetaMessageType();
			string typeName = MidiHelper.GetMetaString(type);
			byte[] data = mess.GetMetaMessageData();
			int dataLength = data.Length;

			string value = "";
			
			switch (type)
			{
					// First handle the text based events (0x01 - 0x09)
				case (int) MidiHelper.MetaEventType.TextEvent:
				case (int) MidiHelper.MetaEventType.CopyrightNotice:
				case (int) MidiHelper.MetaEventType.SequenceOrTrackName:
				case (int) MidiHelper.MetaEventType.InstrumentName:
				case (int) MidiHelper.MetaEventType.LyricText:
				case (int) MidiHelper.MetaEventType.MarkerText:
				case (int) MidiHelper.MetaEventType.CuePoint:
				case (int) MidiHelper.MetaEventType.ProgramName:
				case (int) MidiHelper.MetaEventType.DeviceName:
					string text = MidiHelper.GetString(data);
					value = MidiHelper.TextString(text);
					break;
					
					// And then the special events
				case (int) MidiHelper.MetaEventType.SmpteOffset:
					// Hour, Minute, Second, Frame, Field
					// hr mn se fr ff
					value = string.Format("{0:00}:{1:00}:{2:00}:{3:00}:{4:00}",
					                      data[0] & 0x00ff,
					                      data[1] & 0x00ff,
					                      data[2] & 0x00ff,
					                      data[3] & 0x00ff,
					                      data[4] & 0x00ff);
					break;
				case (int) MidiHelper.MetaEventType.TimeSignature:
					int nn = (data[0] & 0x00ff); // numerator
					int dd = (int)(Math.Pow(2, (data[1] & 0x00ff))); // denominator
					int cc = (data[2] & 0x00ff); // midiClocksPerMetronomeClick
					int bb = (data[3] & 0x00ff); // notated 32nd-notes in a MIDI quarter-note
					value = nn + "/" + dd;
					break;
				case (int) MidiHelper.MetaEventType.KeySignature:
					value = KeySignatures.GetKeyName(data);
					break;
				case (int) MidiHelper.MetaEventType.Tempo:
					int bpm = MicroSecsToBpm(data);
					value = string.Format("{0}", bpm);
					break;
				case (int) MidiHelper.MetaEventType.EndOfTrack:
					break;
				default:
					value = MidiHelper.ByteArrayToString(data, " ");
					break;
			}
			
			return string.Format("MetaEvent.CreateMetaEvent((int) MidiHelper.MetaEventType.{0}, \"{1}\", {2}, {3})", typeName, value, tick, ticksPerBeat);
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
			} catch {
				// DO NOTHING - just use the default
			}
			return t;
		}
	}
}
