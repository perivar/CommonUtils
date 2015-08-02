// InvalidMidiDataException.java -- Thrown for invalid MIDI data.
//   Copyright (C) 2005 Free Software Foundation, Inc.

using System;

namespace gnu.sound.midi
{

	/// This exception is thrown when we encounter bad MIDI data.
	/// @author Anthony Green (green@redhat.com)
	/// @since 1.3
	public class InvalidMidiDataException : Exception
	{
		private const long serialVersionUID = 2780771756789932067L;

		/// <summary>
		/// Create an InvalidMidiDataException object.
		/// </summary>
		public InvalidMidiDataException() : base()
		{
		}

		/// <summary>
		/// Create an InvalidMidiDataException object.
		/// @param s the exception message string
		/// </summary>
		public InvalidMidiDataException(string s) : base(s)
		{
		}
	}

}