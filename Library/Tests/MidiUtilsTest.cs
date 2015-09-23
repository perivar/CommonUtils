using System;
using NUnit.Framework;

namespace CommonUtils.Tests
{
	[TestFixture]
	public class MidiUtilsTest
	{
		[Test]
		public void TestMethod()
		{
			for (int i = 0; i < 128; i++ ) {
				float freq = MidiUtils.MidiNoteToPitch(i);
				int note = 0;
				int cents = 0;
				MidiUtils.PitchToMidiNote(freq, out note, out cents);
				string noteName = MidiUtils.GetNoteName(note, false, true);
				
				Console.Out.WriteLine("Midi Key: {0}, Frequency: {1:0.0000} (note: {2} {3} cents - {4})", i, freq, note, cents, noteName);
			}
		}
	}
}
