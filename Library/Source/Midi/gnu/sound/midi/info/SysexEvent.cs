using System;

namespace gnu.sound.midi.info
{
	/// Handle Sysex events.
	/// by Per Ivar Nerseth (perivar@nerseth.com)
	public static class SysexEvent
	{
		/// <summary>
		/// Create a Sysex event
		/// </summary>
		/// <param name="data">a String that represents the data for the event.</param>
		/// <param name="tick">the position of the event in the sequence</param>
		/// <returns>the created Midi Sysex event</returns>
		public static MidiEvent CreateSysexEvent(string data, long tick)
		{
			var bytes = MidiHelper.StringToByteArray(data, ",");
			if (bytes.Length == 0) {
				throw new InvalidProgramException(string.Format("Could not parse the passed sysex event {0}", data));
			}
			
			var sysexMessage = new SysexMessage();
			sysexMessage.SetMessage(bytes, bytes.Length); // use base method to set the whole sysex message in one go
			var ev = new MidiEvent(sysexMessage, tick);
			return ev;
		}
	}
}
