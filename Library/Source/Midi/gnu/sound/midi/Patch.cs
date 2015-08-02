// Patch.java -- A MIDI patch.
//   Copyright (C) 2005 Free Software Foundation, Inc.

namespace gnu.sound.midi
{

	/// A Patch describes where an Instrument is loaded on a Synthesizer.
	/// @author Anthony Green (green@redhat.com)
	/// @since 1.3
	public class Patch
	{
		// Private data describing the patch
		private int bank = 0;
		private int program = 0;

		/// <summary>
		/// Create a Patch object, specifying the bank and program in which this Patch
		/// is located.
		/// @param bank the bank in which this Patch is located
		/// @param program the program in which this Patch is located
		/// </summary>
		public Patch(int bank, int program)
		{
			this.bank = bank;
			this.program = program;
		}

		/// <summary>
		/// Get the bank in which this Patch is located.
		/// @return the bank in which this Patch is located
		/// </summary>
		public int getBank()
		{
			return bank;
		}

		/// <summary>
		/// Get the program in which this Patch is located.
		/// @return the program in which this Patch is located
		/// </summary>
		public int getProgram()
		{
			return program;
		}
	}
}