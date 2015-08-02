using System;

namespace gnu.sound.midi
{
	/// <summary>
	/// MidiHelper class originally taken from
	/// https://searchcode.com/codesearch/view/28045005/
	/// </summary>
	public static class MidiHelper
	{
		//--Constants
		public const uint MICRO_SECONDS_PER_MINUTE = 60000000; //microseconds in a minute
		public const int MAX_CHANNEL = 15;
		public const int MIN_CHANNEL = 0;
		public const int DRUM_CHANNEL = 9;
		
		//--Enum
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
		// Midi event enum
		public enum MidiEventType
		{
			None = int.MinValue,
			Note_Off = 0x80, 		// Status nibble for Note Off message.
			Note_On = 0x90,			// Status nibble for Note On message.
			Note_Aftertouch = 0xA0,	// Status nibble for Poly Pressure message.
			Controller = 0xB0,		// Status nibble for Control Change message.
			Program_Change = 0xC0,	// Status nibble for Program Change message.
			Channel_Aftertouch = 0xD0, // Statue nibble for Channel Pressure message.
			Pitch_Bend = 0xE0, 		// Status nibble for Pitch Bend message.
			System_Exclusive = 0xF0, 
			MetaEvent = 0xFF
		}
		public enum ControllerType
		{
			None = int.MinValue,
			BankSelectMSB = 0x00,
			ModulationMSB = 0x01,
			BreathControllerMSB = 0x02,
			FootControllerMSB = 0x04,
			PortamentoTimeMSB = 0x05,
			DataEntryMSB = 0x06,
			VolumeMSB = 0x07,
			BalanceMSB = 0x08,
			PanMSB = 0x0A,
			ExpressionControllerMSB = 0x0B,
			EffectControl1MSB = 0x0C,
			EffectControl2MSB = 0x0D,
			GeneralPurposeController1 = 0x10,
			GeneralPurposeController2 = 0x11,
			GeneralPurposeController3 = 0x12,
			GeneralPurposeController4 = 0x13,
			BankSelectLSB = 0x20,
			ModulationLSB = 0x21,
			BreathControllerLSB = 0x22,
			FootControllerLSB = 0x24,
			PortamentoTimeLSB = 0x25,
			DataEntryLSB = 0x26,
			VolumeLSB = 0x27,
			BalanceLSB = 0x28,
			PanLSB = 0x2A,
			ExpressionControllerLSB = 0x2B,
			EffectControl1LSB = 0x2C,
			EffectControl2LSB = 0x2D,
			DamperPedal = 0x40,
			Portamento = 0x41,
			Sostenuto = 0x42,
			SoftPedal = 0x43,
			LegatoFootswitch = 0x44,
			Hold2 = 0x45,
			SoundController1 = 0x46,
			SoundController2 = 0x47,
			SoundController3 = 0x48,
			SoundController4 = 0x49,
			SoundController5 = 0x4A,
			SoundController6 = 0x4B,
			SoundController7 = 0x4C,
			SoundController8 = 0x4D,
			SoundController9 = 0x4E,
			SoundController10 = 0x4F,
			GeneralPurposeController5 = 0x50,
			GeneralPurposeController6 = 0x51,
			GeneralPurposeController7 = 0x52,
			GeneralPurposeController8 = 0x53,
			PortamentoControl = 0x54,
			Effects1Depth = 0x5B,
			Effects2Depth = 0x5C,
			Effects3Depth = 0x5D,
			Effects4Depth = 0x5E,
			Effects5Depth = 0x5F,
			DataIncrement = 0x60,
			DataDecrement = 0x61,
			NonRegisteredParameterLSB = 0x62,
			NonRegisteredParameterMSB = 0x63,
			RegisteredParameterLSB = 0x64,
			RegisteredParameterMSB = 0x65,
			AllSoundOff = 0x78,
			ResetControllers = 0x79,
			AllNotesOff = 0x7B,
			OmniModeOff = 0x7C,
			OmniModeOn = 0x7D
		}
		public enum MetaEventType
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
		// Helper methods
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
	}
}
