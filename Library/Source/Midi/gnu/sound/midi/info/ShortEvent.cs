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
						result[0] = "AfterTouchChannel";
						result[2] = Convert.ToInt32(d1);
						break;
					case (int) MidiHelper.MidiEventType.ControlChange:
						result[0] = Controllers.GetControlName(d1);
						result[2] = Controllers.GetControlValue(d2, false);
						break;
					case (int) MidiHelper.MidiEventType.NoteOff:
						result[0] = "NoteOff";
						result[1] = NoteNames.GetNoteName(d1, inFlats);
						result[2] = Convert.ToInt32(d2);
						break;
					case (int) MidiHelper.MidiEventType.NoteOn:
						result[0] = "NoteOn ";
						result[1] = NoteNames.GetNoteName(d1, inFlats);
						result[2] = Convert.ToInt32(d2);
						break;
					case (int) MidiHelper.MidiEventType.PitchBend:
						result[0] = "PitchBend";
						result[2] = Convert.ToInt32(d1 + (d2 << 7));
						break;
					case (int) MidiHelper.MidiEventType.AfterTouchPoly:
						result[0] = "AfterTouchPoly";
						result[1] = NoteNames.GetNoteName(d1, inFlats);
						result[2] = Convert.ToInt32(d2);
						break;
					case (int) MidiHelper.MidiEventType.ProgramChange:
						result[0] = "ProgramChange ";
						result[3] = InstrumentNames.GetName(d2, d1);
						break;
					default:
						result[0] = "Unknown";
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
						result[0] = "ActiveSensing";
						break;
					case (int) MidiHelper.MidiEventType.Continue:
						result[0] = "Continue";
						break;
					case (int) MidiHelper.MidiEventType.EndOfExclusive:
						result[0] = "EndOfExclusive";
						break;
					case (int) MidiHelper.MidiEventType.TimeCodeQuarterFrame:
						result[0] = "TimeCodeQuarterFrame";
						break;
					case (int) MidiHelper.MidiEventType.SongPosition:
						result[0] = "SongPosition";
						result[2] = Convert.ToInt32(d1 + (d2 << 7));
						break;
					case (int) MidiHelper.MidiEventType.SongSelect:
						result[0] = "SongSelect";
						result[2] = Convert.ToInt32(d1);
						break;
					case (int) MidiHelper.MidiEventType.Start:
						result[0] = "Start";
						break;
					case (int) MidiHelper.MidiEventType.Stop:
						result[0] = "Stop";
						break;
					case (int) MidiHelper.MidiEventType.SystemReset:
						result[0] = "SystemReset";
						break;
					case (int) MidiHelper.MidiEventType.Clock:
						result[0] = "Clock";
						break;
					case (int) MidiHelper.MidiEventType.TuneRequest:
						result[0] = "TuneRequest";
						break;
					default:
						result[0] = "Undefined";
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
		
		public static string CreateShortEventGeneratedCode(ShortMessage mess, bool inFlats, long tick) {
			
			int cmd = mess.GetCommand();
			string typeName = MidiHelper.GetEventTypeString(cmd);

			int st = mess.GetStatus();
			int d1 = mess.GetData1();
			int d2 = mess.GetData2();
			
			int chan = mess.GetChannel();
			string val1 = "0";
			string val2 = "0";
			string comment = "";

			if ((st & 0xf0) <= 0xf0) // This is a channel message
			{
				switch (cmd)
				{
					case (int) MidiHelper.MidiEventType.AfterTouchChannel:
						val1 = "" + Convert.ToInt32(d1);
						val2 = "0";
						break;
					case (int) MidiHelper.MidiEventType.ControlChange:
						string controlName = Controllers.GetControlName(d1);
						val1 = "" + Controllers.GetControllerNumber(controlName);
						val2 = "" + Convert.ToInt32(Controllers.GetControlValue(d2, false));
						comment = string.Format("// {0}", controlName);
						break;
					case (int) MidiHelper.MidiEventType.NoteOff:
						string noteOff = NoteNames.GetNoteName(d1, inFlats);
						val1 = string.Format("NoteNames.GetNoteNumber(\"{0}\")", noteOff);
						val2 = "" + Convert.ToInt32(d2);
						break;
					case (int) MidiHelper.MidiEventType.NoteOn:
						string noteOn = NoteNames.GetNoteName(d1, inFlats);
						val1 = string.Format("NoteNames.GetNoteNumber(\"{0}\")", noteOn);
						val2 = "" + Convert.ToInt32(d2);
						break;
					case (int) MidiHelper.MidiEventType.PitchBend:
						val1 = "" + Convert.ToInt32(d1 + (d2 << 7));
						val2 = "0";
						break;
					case (int) MidiHelper.MidiEventType.AfterTouchPoly:
						string noteAfterTouch = NoteNames.GetNoteName(d1, inFlats);
						val1 = string.Format("NoteNames.GetNoteNumber(\"{0}\")", noteAfterTouch);
						val2 = "" + Convert.ToInt32(d2);
						break;
					case (int) MidiHelper.MidiEventType.ProgramChange:
						string instrumentName = InstrumentNames.GetName(d2, d1);
						val1 = "" + InstrumentNames.GetInstrumentNumber(instrumentName);
						val2 = "0";
						comment = string.Format("// {0}", instrumentName);
						break;
					default:
						break;
				}
			}
			
			// This is a system message
			else
			{
				switch (st)
				{
					case (int) MidiHelper.MidiEventType.ActiveSensing:
					case (int) MidiHelper.MidiEventType.Continue:
					case (int) MidiHelper.MidiEventType.EndOfExclusive:
					case (int) MidiHelper.MidiEventType.TimeCodeQuarterFrame:
					case (int) MidiHelper.MidiEventType.Start:
					case (int) MidiHelper.MidiEventType.Stop:
					case (int) MidiHelper.MidiEventType.SystemReset:
					case (int) MidiHelper.MidiEventType.Clock:
					case (int) MidiHelper.MidiEventType.TuneRequest:
						val1 = "0";
						val2 = "0";
						break;
					case (int) MidiHelper.MidiEventType.SongPosition:
						val1 = "" + Convert.ToInt32(d1 + (d2 << 7));
						val2 = "0";
						break;
					case (int) MidiHelper.MidiEventType.SongSelect:
						val1 = "" + Convert.ToInt32(d1);
						val2 = "0";
						break;
					default:
						break;
				}
			}
			
			return string.Format("ShortEvent.CreateShortEvent((int) MidiHelper.MidiEventType.{0}, {1}, {2}, {3}, {4})); {5}",
			                     typeName, chan, val1, val2, tick, comment);
		}
	}
}