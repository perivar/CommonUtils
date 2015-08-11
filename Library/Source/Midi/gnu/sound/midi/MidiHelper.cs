using System;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace gnu.sound.midi
{
	/// <summary>
	/// MidiHelper class originally taken from
	/// https://searchcode.com/codesearch/view/28045005/
	/// but extended by
	/// perivar@nerseth.com
	/// </summary>
	public static class MidiHelper
	{
		// Time Constants
		public const uint MICRO_SECONDS_PER_MINUTE = 60000000; // microseconds in a minute

		// Channels Constants
		public const int MIN_CHANNEL = 0;
		public const int MAX_CHANNEL = 0xF;
		public const int DRUM_CHANNEL = 9; // Channel 10 (1-based) is reserved for the percussion map
		
		/// <summary>
		/// The META status code.  Only valid for MIDI files, not the wire protocol.
		/// </summary>
		public const int META = 0xFF;
		
		// Midi format enum
		public enum MidiTimeFormat
		{
			TicksPerBeat,
			FamesPerSecond
		}
		
		public enum MidiFormat
		{
			SingleTrack = 0,
			MultiTrack = 1,
			MultiSong = 2
		}
		
		public enum MidiEventType {
			None = int.MinValue,
			NoteOff	              = 0x80,	// Note Off
			NoteOn                = 0x90,	// Note On
			AfterTouchPoly        = 0xA0,	// Polyphonic AfterTouch
			ControlChange         = 0xB0,	// Control Change / Channel Mode Control Change. Controller numbers 120-127 are reserved as "Channel Mode Messages".
			ProgramChange         = 0xC0,	// Program Change
			AfterTouchChannel     = 0xD0,	// Channel (monophonic) AfterTouch

			PitchBend             = 0xE0,	// Pitch Bend
			// The two bytes of the pitch bend message form a 14 bit number, 0 to 16383.
			// The value 8192 (sent, LSB first, as 0x00 0x40), is centered, or "no pitch bend."
			// The value 0 (0x00 0x00) means, "bend as low as possible,"
			// and, similarly, 16383 (0x7F 0x7F) is to "bend as high as possible."
			
			SystemExclusive       = 0xF0,	// System Exclusive
			TimeCodeQuarterFrame  = 0xF1,	// System Common - MIDI Time Code Quarter Frame
			
			SongPosition          = 0xF2,	// System Common, Song Pointer, with data1 and data2 being LSB and MSB respectively.
			// This is an internal 14 bit register that holds the number of MIDI beats (1 beat = six MIDI clock messages) since the start of the song.
			// This 14-bit value is the MIDI Beat upon which to start the song.
			// Songs are always assumed to start on a MIDI Beat of 0. Each MIDI Beat spans 6 MIDI Clocks.
			// In other words, each MIDI Beat is a 16th note (since there are 24 MIDI Clocks in a quarter note).
			
			SongSelect            = 0xF3,	// System Common, Song Select, with data1 being the Song Number, data2 unused
			BusSelect             = 0xF5, 	// FIXME: unofficial bus select. Not in spec??
			TuneRequest           = 0xF6,	// System Common - Tune Request
			EndOfExclusive        = 0xF7,	// End Of Exclusive message.
			Clock                 = 0xF8,	// System Real Time - Timing Clock
			Start                 = 0xFA,	// System Real Time - Start
			Continue              = 0xFB,	// System Real Time - Continue
			Stop                  = 0xFC,	// System Real Time - Stop
			ActiveSensing         = 0xFE,	// System Real Time - Active Sensing
			SystemReset           = 0xFF,	// System Real Time - System Reset (MetaEvent)
			InvalidType           = 0x00    // For notifying errors
		};
		
		public enum ControllerType
		{
			None = int.MinValue,
			BankSelectMSB				= 0x00,
			ModulationMSB				= 0x01,
			BreathControllerMSB			= 0x02,
			FootControllerMSB			= 0x04,
			PortamentoTimeMSB			= 0x05,
			DataEntryMSB				= 0x06,
			VolumeMSB					= 0x07,
			BalanceMSB					= 0x08,
			PanMSB						= 0x0A,
			ExpressionControllerMSB		= 0x0B,
			EffectControl1MSB			= 0x0C,
			EffectControl2MSB			= 0x0D,
			GeneralPurposeController1	= 0x10,
			GeneralPurposeController2	= 0x11,
			GeneralPurposeController3	= 0x12,
			GeneralPurposeController4	= 0x13,
			BankSelectLSB				= 0x20,
			ModulationLSB				= 0x21,
			BreathControllerLSB			= 0x22,
			FootControllerLSB			= 0x24,
			PortamentoTimeLSB			= 0x25,
			DataEntryLSB				= 0x26,
			VolumeLSB					= 0x27,
			BalanceLSB					= 0x28,
			PanLSB						= 0x2A,
			ExpressionControllerLSB		= 0x2B,
			EffectControl1LSB			= 0x2C,
			EffectControl2LSB			= 0x2D,
			DamperPedal					= 0x40,
			Portamento					= 0x41,
			Sostenuto					= 0x42,
			SoftPedal					= 0x43,
			LegatoFootswitch			= 0x44,
			Hold2						= 0x45,
			SoundController1			= 0x46,
			SoundController2			= 0x47,
			SoundController3			= 0x48,
			SoundController4			= 0x49,
			SoundController5			= 0x4A,
			SoundController6			= 0x4B,
			SoundController7			= 0x4C,
			SoundController8			= 0x4D,
			SoundController9			= 0x4E,
			SoundController10			= 0x4F,
			GeneralPurposeController5	= 0x50,
			GeneralPurposeController6	= 0x51,
			GeneralPurposeController7	= 0x52,
			GeneralPurposeController8	= 0x53,
			PortamentoControl			= 0x54,
			Effects1Depth				= 0x5B,
			Effects2Depth				= 0x5C,
			Effects3Depth				= 0x5D,
			Effects4Depth				= 0x5E,
			Effects5Depth				= 0x5F,
			DataIncrement				= 0x60,
			DataDecrement				= 0x61,
			NonRegisteredParameterLSB	= 0x62,
			NonRegisteredParameterMSB	= 0x63,
			RegisteredParameterLSB		= 0x64,
			RegisteredParameterMSB		= 0x65,
			AllSoundOff					= 0x78,
			ResetControllers			= 0x79,
			AllNotesOff					= 0x7B,
			OmniModeOff					= 0x7C,
			OmniModeOn					= 0x7D
		}
		
		// TimeSignature = 0x58, //FF 58 04 nn dd cc bb
		// nn is a byte specifying the numerator of the time signature (as notated).
		// dd is a byte specifying the denominator of the time signature as a negative power of 2 (ie 2 represents a quarter-note, 3 represents an eighth-note, etc). (2^dd)
		// cc is a byte specifying the number of MIDI clocks between metronome clicks.
		// bb is a byte specifying the number of notated 32nd-notes in a MIDI quarter-note (24 MIDI Clocks). The usual value for this parameter is 8, though some sequencers allow the user to specify that what MIDI thinks of as a quarter note, should be notated as something else.
		// 
		// Examples
		// A time signature of 4/4, with a metronome click every 1/4 note, would be encoded :
		// FF 58 04 04 02 18 08
		// There are 24 MIDI Clocks per quarter-note, hence cc=24 (0x18).
		// 
		// A time signature of 6/8, with a metronome click every 3rd 1/8 note, would be encoded :
		// FF 58 04 06 03 24 08
		// Remember, a 1/4 note is 24 MIDI Clocks, therefore a bar of 6/8 is 72 MIDI Clocks.
		
		// KeySignature	= 0x59, //FF 59 02 sf mi
		// sf is a byte specifying the number of flats (-ve) or sharps (+ve) that identifies the key signature (-7 = 7 flats, -1 = 1 flat, 0 = key of C, 1 = 1 sharp, etc).
		// mi is a byte specifying a major (0) or minor (1) key.
		
		public enum MetaEventType
		{
			None = int.MinValue,
			SequenceNumber					= 0x00,	//FF 00 02 ss ss or FF 00 00
			TextEvent						= 0x01, //FF 01 len TEXT (arbitrary TEXT)
			CopyrightNotice					= 0x02, //FF 02 len TEXT
			SequenceOrTrackName				= 0x03, //FF 03 len TEXT
			InstrumentName					= 0x04, //FF 04 len TEXT
			LyricText						= 0x05, //FF 05 len TEXT
			MarkerText						= 0x06, //FF 06 len TEXT (e.g. Loop point)
			CuePoint						= 0x07, //FF 07 len TEXT (e.g. .wav file name)
			ProgramName						= 0x08, //FF 08 len TEXT (PIANO, FLUTE,...)
			DeviceName						= 0x09, //FF 09 len TEXT (MIDI Out 1, MIDI Out 2)
			MidiChannelPrefixAssignment		= 0x20,
			MidiPort						= 0x21,
			EndOfTrack						= 0x2F, //FF 2F 00
			Tempo							= 0x51, //FF 51 03 tt tt tt microseconds
			SmpteOffset						= 0x54, //FF 54 05 hr mn se fr ff
			TimeSignature					= 0x58, //FF 58 04 nn dd cc bb
			KeySignature					= 0x59, //FF 59 02 sf mi
			SequencerSpecificEvent			= 0x7F  //FF 7F len data
		}
		
		#region Enum Helper Methods
		public static bool TryParse<TEnum>(string value, bool ignoreCase, ref TEnum result) where TEnum : struct
		{
			bool parsed = false;
			try
			{
				result = (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase);
				parsed = true;
			}
			catch { }
			return parsed;
		}
		
		public static string GetMidiTimeFormatString(MidiTimeFormat value)
		{
			return Enum.GetName(typeof(MidiTimeFormat), value);
		}

		public static string GetMidiTimeFormatString(int value)
		{
			return Enum.GetName(typeof(MidiTimeFormat), value);
		}

		public static string GetMidiFormatString(MidiFormat value)
		{
			return Enum.GetName(typeof(MidiFormat), value);
		}

		public static string GetMidiFormatString(int value)
		{
			return Enum.GetName(typeof(MidiFormat), value);
		}
		
		public static string GetEventTypeString(MidiEventType value)
		{
			return Enum.GetName(typeof(MidiEventType), value);
		}

		public static string GetEventTypeString(int value)
		{
			return Enum.GetName(typeof(MidiEventType), value);
		}
		
		public static string GetControllerString(ControllerType value)
		{
			return Enum.GetName(typeof(ControllerType), value);
		}
		
		public static string GetControllerString(int value)
		{
			return Enum.GetName(typeof(ControllerType), value);
		}

		public static string GetMetaString(MetaEventType value)
		{
			return Enum.GetName(typeof(MetaEventType), value);
		}
		public static string GetMetaString(int value)
		{
			return Enum.GetName(typeof(MetaEventType), value);
		}
		#endregion

		#region Useful util methods
		public static sbyte[] ConvertBytes(byte[] byteArray)
		{
			//sbyte[] sbytes = Array.ConvertAll(bytes, b => unchecked((sbyte)b));
			var sbyteArray = new sbyte[byteArray.Length];
			for (int i = 0; i < sbyteArray.Length; i++)
			{
				sbyteArray[i] = unchecked((sbyte) byteArray[i]);
			}

			return sbyteArray;
		}

		public static byte[] ConvertSBytes(sbyte[] sbyteArray)
		{
			var byteArray = new byte[sbyteArray.Length];
			for (int i = 0; i < byteArray.Length; i++)
			{
				byteArray[i] = unchecked((byte) sbyteArray[i]);
			}
			return byteArray;
		}
		
		public static byte[] GetBytes(string str)
		{
			return Encoding.Default.GetBytes(str);
		}
		
		public static string GetString(byte[] bytes)
		{
			return Encoding.Default.GetString(bytes);
		}
		
		public static string GetStringWithoutNewlines(byte[] bytes) {
			string text = GetString(bytes);
			text = text.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
			return text;
		}

		/// <summary>
		/// Convert a byte array into a hex string representation
		/// </summary>
		/// <param name="data">byte array</param>
		/// <param name="delimiter">delimiter</param>
		/// <returns>a hex string in the format 0x0A,0xA5,0x4A (',' is the default delimiter)</returns>
		public static string ByteArrayToHexString(byte[] data, string delimiter=",") {
			var sb = new StringBuilder(data.Length * 6);
			for (int k = 0; k < data.Length; ++k)
			{
				int i = data[k];
				if (k > 0)
				{
					sb.Append(delimiter);
				}
				sb.AppendFormat("0x{0:X2}", i);
			}
			return sb.ToString();
		}
		
		/// <summary>
		/// Convert a byte array into a hex string representation
		/// </summary>
		/// <param name="bytes">byte array</param>
		/// <param name="delimiter">delimiter</param>
		/// <returns>a hex string</returns>
		/// <example>
		/// byte[] {0x00,0xA0,0xBf} will return the string "00A0BF"
		/// byte[] {0x00,0xA0,0xBf} delimiter ',' will return the string "00,A0,BF"
		/// </example>
		public static string ByteArrayToString(byte[] bytes, string delimiter="")
		{
			string hex = BitConverter.ToString(bytes);
			return hex.Replace("-", delimiter);
		}

		/// <summary>
		/// Convert a hex string into a byte array
		/// </summary>
		/// <param name="hex">hex string</param>
		/// <param name="delimiter">delimiter</param>
		/// <returns>a byte array</returns>
		/// <example>
		/// the string "00A0BF" will return byte[] {0x00,0xA0,0xBf}
		/// the string "00,A0,BF" delimiter ',' will return byte[] {0x00,0xA0,0xBf}
		/// </example>
		public static byte[] StringToByteArray(String hex, string delimiter="")
		{
			if (delimiter != "") {
				var elements = hex.Split(new String[]{delimiter}, StringSplitOptions.RemoveEmptyEntries);
				int NumberBytes = elements.Length;
				var bytes = new byte[NumberBytes];
				for (int i = 0; i < NumberBytes; i++) {
					bytes[i] = Convert.ToByte(elements[i], 16);
				}
				return bytes;
				
			} else {
				int NumberChars = hex.Length;
				var bytes = new byte[NumberChars / 2];
				for (int i = 0; i < NumberChars; i += 2) {
					bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
				}
				return bytes;
			}
		}
		#endregion
		
		#region C# Generator Utils taken from the MidiSharp project by Stephen Toub
		
		[ThreadStatic]
		private static StringBuilder t_cachedBuilder;
		
		/// <summary>
		/// Create a C# string from text, escaping some characters as unicode to make sure it renders reasonably in the C# code while
		/// still resulting in the same output string.
		/// </summary>
		/// <param name="text">The text to output.</param>
		/// <returns>The text put into a string.</returns>
		/// <remarks>This is based on the excellent MidiSharp package by Stephen Toub.</remarks>
		public static string TextString(string text)
		{
			bool acceptable = true;
			foreach (char c in text) {
				if (!IsValidInTextString(c)) {
					acceptable = false;
					break;
				}
			}
			if (acceptable) {
				return text;
			}

			StringBuilder sb = t_cachedBuilder ?? (t_cachedBuilder = new StringBuilder(text.Length * 2 + 2));
			foreach (char c in text) {
				if (IsValidInTextString(c)) {
					sb.Append(c);
				}
				else {
					sb.AppendFormat(CultureInfo.InvariantCulture, "\\u{0:X4}", (int)c);
				}
			}
			string result = sb.ToString();
			sb.Clear();
			return result;
		}

		/// <summary>
		/// Gets whether a character is ok to render in a C# quoted string.
		/// Letters and digits are ok.  A space is fine, but whitespace like new lines could
		/// cause problems for a string, since it's not rendered as a verbatim string.
		/// Punctuation is generally ok, but certainly punctuation has special meaning inside
		/// a C# string and is not ok.
		/// </summary>
		/// <param name="c">The character to examine.</param>
		/// <returns>true if the character is valid; otherwise, false.</returns>
		private static bool IsValidInTextString(char c)
		{
			return
				Char.IsLetterOrDigit(c) || c == ' ' ||
				(Char.IsPunctuation(c) && c != '\\' && c != '\"' && c != '{');
		}
		#endregion

		/// <summary>
		/// Return an URL as a Stream
		/// </summary>
		/// <param name="url">url</param>
		/// <returns>Stream</returns>
		public static Stream GetStreamFromUrl(string url)
		{
			byte[] data = null;

			using (var wc = new System.Net.WebClient())
				data = wc.DownloadData(url);

			return new MemoryStream(data);
		}

		/// <summary>
		/// Convert two 7-bit databytes (LSB and MSB) into an int value
		/// E.g. convert the least significant and most significant byte
		/// for PitchBend or SongPosition into an int value between 0 and 16383.
		/// msg.GetData1() is the LSB, and msg.GetData2 in the MSB
		/// </summary>
		/// <param name="lsb">least significant byte (message.GetData1())</param>
		/// <param name="msb">most significant byte (message.GetData2())</param>
		/// <returns>a 14 bit number, 0 to 16383.</returns>
		public static int TwoBytesToInt(int lsb, int msb) {
			return  (msb * 128) + lsb; // this is the same as Convert.ToInt32(d1 + (d2 << 7));
		}
		
		/// <summary>
		/// Convert an int value into two 7-bit data bytes (LSB and MSB).
		/// E.g. convert a PitchBend or SongPosition int value into two bits,
		/// the least significant and most significant byte.
		/// </summary>
		/// <param name="value">a 14 bit number, 0 to 16383.</param>
		/// <returns>least significant and most significant bytes</returns>
		public static KeyValuePair<int,int> IntToTwoBytes(int value) {
			int msb = value >> 7; 	// MSB is: e.g. pitchbend bit shift right 7
			int lsb = value & 0x7F; // LSB is: e.g. pitchbend bitwise AND with a mask of 127
			return new KeyValuePair<int, int>(lsb, msb);
		}
	}
}
