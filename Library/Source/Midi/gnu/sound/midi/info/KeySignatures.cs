using System;
using System.Text;

namespace gnu.sound.midi.info
{
	/// Methods associated with key signatures.
	/// Taken from MidiQuickFix - A Simple Midi file editor and player
	/// Copyright (C) 2004-2009 John Lemcke
	/// jostle@users.sourceforge.net
	/// Modified by Per Ivar Nerseth (perivar@nerseth.com)
	public class KeySignatures
	{
		internal static string[] majorKeyNames = { "Cb", "Gb", "Db", "Ab", "Eb", "Bb", "F", "C", "G", "D", "A", "E", "B", "F#", "C#" };
		internal static string[] minorKeyNames = { "Ab", "Eb", "Bb", "F", "C", "G", "D", "A", "E", "B", "F#", "C#", "G#", "D#", "A#" };
		
		public static string GetKeyName(byte[] data)
		{
			var result = new StringBuilder();
			
			sbyte signed = unchecked((sbyte)data[0]);
			int key = signed + 7;
			int tonality = data[1];
			if (tonality == 1) // 0 = major, 1 = minor
			{
				result.Append(minorKeyNames[key]).Append("m");
			}
			else
			{
				result.Append(majorKeyNames[key]);
			}
			
			string keyName = result.ToString();
			return keyName;
		}
		
		public static sbyte[] GetKeyValues(string keyName)
		{
			sbyte[] result = {0, 0};
			
			// Check for a minor key
			int mPos = keyName.IndexOf("m");
			if (mPos != -1)
			{
				result[1] = 1;
				// and remove the trailing "m"
				keyName = keyName.Substring(0, keyName.Length - 1);
			}

			string[] keyNames = result[1] == 1 ? minorKeyNames : majorKeyNames;

			for (sbyte i = 0; i < keyNames.Length; ++i)
			{
				if (keyName.Equals(keyNames[i]))
				{
					result[0] = (sbyte)(i - 7);
					break;
				}
			}
			return result;
		}
		
		public static bool IsInFlats(string keyName)
		{
			if (keyName == null)
			{
				return false;
			}

			sbyte[] data = GetKeyValues(keyName);
			return IsInFlats(data[0]);
		}
		
		public static bool IsInFlats(sbyte keyNum)
		{
			return keyNum < 0;
		}
	}
}