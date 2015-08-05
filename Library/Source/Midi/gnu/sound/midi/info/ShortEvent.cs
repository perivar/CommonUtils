using System;

namespace gnu.sound.midi.info
{
	/// Handle Midi Short events.
	/// Taken from MidiQuickFix - A Simple Midi file editor and player
	/// Copyright (C) 2004-2009 John Lemcke
	/// jostle@users.sourceforge.net
	public static class ShortEvent
	{
		internal static object[] GetShortStrings(ShortMessage mess, bool inFlats)
		{
			// Event, Note, Value, Patch, Text, Channel
			object[] result = { "", "", null, "", "", null };
			int st = mess.GetStatus();
			int d1 = mess.GetData1();
			int d2 = mess.GetData2();

			if ((st & 0xf0) <= 0xf0) // This is a channel message
			{
				int cmd = mess.GetCommand();
				switch (cmd)
				{
					case (int) MidiHelper.MidiEventType.AfterTouchChannel:
						result[0] = "CHANNEL_PRESSURE";
						result[2] = Convert.ToInt32(d1); //"" + d1;
						break;
					case (int) MidiHelper.MidiEventType.ControlChange:
						result[0] = Controllers.GetControlName(d1);
						result[2] = Controllers.GetControlValue(d2, false);
						break;
					case (int) MidiHelper.MidiEventType.NoteOff:
						result[0] = "NOTE_OFF";
						result[1] = NoteNames.GetNoteName(d1, inFlats);
						result[2] = Convert.ToInt32(d2);
						break;
					case (int) MidiHelper.MidiEventType.NoteOn:
						result[0] = "NOTE_ON ";
						result[1] = NoteNames.GetNoteName(d1, inFlats);
						result[2] = Convert.ToInt32(d2);
						break;
					case (int) MidiHelper.MidiEventType.PitchBend:
						result[0] = "PITCH_BEND";
						result[2] = Convert.ToInt32(d1 + (d2 << 7));
						break;
					case (int) MidiHelper.MidiEventType.AfterTouchPoly:
						result[0] = "POLY_PRESSURE";
						result[1] = NoteNames.GetNoteName(d1, inFlats);
						result[2] = Convert.ToInt32(d2);
						break;
					case (int) MidiHelper.MidiEventType.ProgramChange:
						result[0] = "PATCH ";
						result[3] = InstrumentNames.GetName(d2, d1);
						break;
					default:
						result[0] = "UNKNOWN";
						break;
				}
				int chan = mess.GetChannel();
				result[5] = Convert.ToInt32(chan);
			}
			
			// This is a system message
			else
			{
				switch (st)
				{
					case (int) MidiHelper.MidiEventType.ActiveSensing:
						result[0] = "ACTIVE_SENSING";
						break;
					case (int) MidiHelper.MidiEventType.Continue:
						result[0] = "CONTINUE";
						break;
					case (int) MidiHelper.MidiEventType.EndOfExclusive:
						result[0] = "END_OF_EXCLUSIVE";
						break;
					case (int) MidiHelper.MidiEventType.TimeCodeQuarterFrame:
						result[0] = "MIDI_TIME_CODE";
						break;
					case (int) MidiHelper.MidiEventType.SongPosition:
						result[0] = "SONG_POSITION_POINTER";
						result[2] = Convert.ToInt32(d1 + (d2 << 7));
						break;
					case (int) MidiHelper.MidiEventType.SongSelect:
						result[0] = "SONG_SELECT";
						result[2] = Convert.ToInt32(d1); //"" + d1;
						break;
					case (int) MidiHelper.MidiEventType.Start:
						result[0] = "START";
						break;
					case (int) MidiHelper.MidiEventType.Stop:
						result[0] = "STOP";
						break;
					case (int) MidiHelper.MidiEventType.SystemReset:
						result[0] = "RESET";
						break;
					case (int) MidiHelper.MidiEventType.Clock:
						result[0] = "TIMING_CLOCK";
						break;
					case (int) MidiHelper.MidiEventType.TuneRequest:
						result[0] = "TUNE_REQUEST";
						break;
					default:
						result[0] = "UNDEFINED";
						break;
				}
			}
			return result;
		}

		public static MidiEvent CreateShortEvent(int status, long tick)
		{
			var sm = new ShortMessage();
			sm.SetMessage(status);
			var ev = new MidiEvent(sm, tick);
			return ev;
		}

		public static MidiEvent CreateShortEvent(int status, int d1, int d2, long tick)
		{
			var sm = new ShortMessage();
			sm.SetMessage(status, d1, d2);
			var ev = new MidiEvent(sm, tick);
			return ev;
		}

		public static MidiEvent CreateShortEvent(int status, int channel, int d1, int d2, long tick)
		{
			var sm = new ShortMessage();
			sm.SetMessage(status, channel, d1, d2);
			var ev = new MidiEvent(sm, tick);
			return ev;
		}
	}
}