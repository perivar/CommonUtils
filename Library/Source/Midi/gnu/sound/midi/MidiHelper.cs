using System;
using System.Linq;
using System.Text;

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
		//--Constants
		public const uint MICRO_SECONDS_PER_MINUTE = 60000000; //microseconds in a minute
		public const int MAX_CHANNEL = 15;
		public const int MIN_CHANNEL = 0;
		public const int DRUM_CHANNEL = 9;
		
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
			ControlChange         = 0xB0,	// Control Change / Channel Mode
			ProgramChange         = 0xC0,	// Program Change
			AfterTouchChannel     = 0xD0,	// Channel (monophonic) AfterTouch
			PitchBend             = 0xE0,	// Pitch Bend
			SystemExclusive       = 0xF0,	// System Exclusive
			TimeCodeQuarterFrame  = 0xF1,	// System Common - MIDI Time Code Quarter Frame
			SongPosition          = 0xF2,	// System Common - Song Position Pointer
			SongSelect            = 0xF3,	// System Common - Song Select
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
		
		public enum MetaEventType
		{
			None = int.MinValue,
			SequenceNumber					= 0x00,
			TextEvent						= 0x01,
			CopyrightNotice					= 0x02,
			SequenceOrTrackName				= 0x03,
			InstrumentName					= 0x04,
			LyricText						= 0x05,
			MarkerText						= 0x06,
			CuePoint						= 0x07,
			MidiChannelPrefixAssignment		= 0x20,
			MidiPort						= 0x21,
			EndOfTrack						= 0x2F,
			Tempo							= 0x51,
			SmpteOffset						= 0x54,
			TimeSignature					= 0x58,
			KeySignature					= 0x59,
			SequencerSpecificEvent			= 0x7F
		}
		
		#region Enum Helper Methods
		public static string GetEventTypeString(MidiEventType value)
		{
			return Enum.GetName(typeof(MidiEventType), value);
		}
		
		public static string GetControllerString(ControllerType value)
		{
			return Enum.GetName(typeof(ControllerType), value);
		}
		
		public static string GetMetaString(MetaEventType value)
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

	}
}
