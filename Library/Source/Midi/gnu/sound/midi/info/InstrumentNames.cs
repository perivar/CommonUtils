using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace gnu.sound.midi.info
{
	/// Methods associated with MIDI instrument (patch) names.
	/// Taken from MidiQuickFix - A Simple Midi file editor and player
	/// Copyright (C) 2004-2009 John Lemcke
	/// jostle@users.sourceforge.net
	public static class InstrumentNames
	{
		// The names of the controllers are retrieved from a resource file.
		internal static Dictionary<string, string> properties = new Dictionary<string, string>();
		
		const string PATH_TO_FILE = @"resources\GM1Instruments.properties";

		/// Create an InstrumentNames object.
		static InstrumentNames()
		{
			// Read in properties file
			properties = new Dictionary<string, string>();
			foreach (var row in File.ReadAllLines(PATH_TO_FILE)) {
				var columns = row.Split('=');
				if (columns.Count() > 1 && !columns[0].StartsWith("#")) {
					string key = columns[0];
					string value = columns[1];
					properties.Add(key, value);
				}
			}
		}

		/// <summary>
		/// Get the instrument (patch) name.
		/// </summary>
		/// <param name="bank">The bank number.</param>
		/// <param name="program">The program number.</param>
		/// <returns>The instrument (patch) name.</returns>
		public static string GetName(int bank, int program)
		{
			// TODO don't ignore bank
			
			// See whether Dictionary contains this string.
			string key = "" + program;
			string value = string.Empty;
			if (properties.ContainsKey(key))
			{
				value = properties[key];
			}
			return value;
		}

		/// <summary>
		/// Get the instrument (patch) name assuming it to be in the first bank.
		/// </summary>
		/// <param name="program">The program number.</param>
		/// <returns>The instrument (patch) name.</returns>
		public static string GetName(int program)
		{
			return GetName(0, program);
		}

		/// <summary>
		/// Get all the instrument names.
		/// </summary>
		/// <returns>The array of instrument names.</returns>
		public static object[] GetNameArray()
		{
			return properties.Values.ToArray();
		}

		/// <summary>
		/// Get the instrument names for a given bank.
		/// </summary>
		/// <param name="bank">the bank for which to get the names</param>
		/// <returns>The array of instrument names.</returns>
		public static object[] GetNameArray(int bank)
		{
			// TODO don't ignore bank
			return properties.Values.ToArray();
		}

		/// <summary>
		/// Get the program number associated with the named instrument.
		/// </summary>
		/// <param name="name">The name of the instrument.</param>
		/// <returns>The patch number for the named instrument.</returns>
		public static int GetInstrumentNumber(string name)
		{
			// The program number is in the low order byte.
			var value = properties.FirstOrDefault(x => x.Value == name).Key;
			return int.Parse(value);
		}

		/// <summary>
		/// Get the program number associated with the named instrument.
		/// </summary>
		/// <param name="name">The name of the instrument.</param>
		/// <returns>The patch number for the named instrument.</returns>
		public static int GetInstrumentBank(string name)
		{
			throw new NotImplementedException();
		}
	}
}