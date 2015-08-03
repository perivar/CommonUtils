using System;

namespace gnu.sound.midi.info
{
	/// Convert between Midi note numbers and textual note names.
	/// @todo This probably should be i18n'd.
	/// Taken from MidiQuickFix - A Simple Midi file editor and player
	/// Copyright (C) 2004-2009 John Lemcke
	/// jostle@users.sourceforge.net
	public static class NoteNames
	{
		internal static readonly string[] sharpNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
		internal static readonly string[] flatNames = { "C", "Db", "D", "Eb", "E", "F", "Gb", "G", "Ab", "A", "Bb", "B" };
		internal static readonly string[] bothNames = { "C", "Db/C#", "D", "Eb/D#", "E", "F", "Gb/F#", "G", "Ab/G#", "A", "Bb/A#", "B" };
		
		internal static string GetNoteName(int note, bool flats)
		{
			int noteNum = (note % 12);
			int octave = (note / 12);
			
			string noteName = string.Empty;
			
			if (flats)
			{
				noteName = string.Format("{0}{1}", flatNames[noteNum], octave);
			}
			else
			{
				noteName = string.Format("{0}{1}", sharpNames[noteNum], octave);
			}
			
			return noteName;
		}

		internal static string GetBothNoteNames(int note)
		{
			int noteNum = (note % 12);
			int octave = (note / 12);
			
			return string.Format("{0}{1}", bothNames[noteNum], octave);
		}

		public static int GetNoteNumber(string name)
		{
			int octave = Convert.ToInt32(name.Substring(name.Length - 1));
			int noteNum = octave * 12;
			string note = name.Substring(0, name.Length - 1);
			bool found = false;
			for (int i = 0; i < flatNames.Length; ++i)
			{
				if (note.Equals(flatNames[i], StringComparison.InvariantCultureIgnoreCase)) {
					noteNum += i;
					found = true;
					break;
				}
			}
			if (!found)
			{
				for (int i = 0; i < sharpNames.Length; ++i)
				{
					if (note.Equals(sharpNames[i], StringComparison.InvariantCultureIgnoreCase))
					{
						noteNum += i;
						break;
					}
				}
			}
			return noteNum;
		}

		public static string[] GetNoteNameArray()
		{
			return bothNames;
		}
	}
}