using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace gnu.sound.midi.info
{
	/// Methods associated with MIDI Controller events
	/// Taken from MidiQuickFix - A Simple Midi file editor and player
	/// Copyright (C) 2004-2009 John Lemcke
	/// jostle@users.sourceforge.net
	/// Modified by Per Ivar Nerseth (perivar@nerseth.com)
	public static class Controllers
	{
		// The names of the controllers are retrieved from a resource file.
		internal static string[] mNameArray;
		
		const string PATH_TO_FILE = @"resources\Controllers.properties";

		// Create an ControllerNames object.
		static Controllers()
		{
			// Read in properties file
			var properties = new Dictionary<string, string>();
			foreach (var row in File.ReadAllLines(PATH_TO_FILE)) {
				var columns = row.Split('=');
				if (columns.Count() > 1 && !columns[0].StartsWith("#")) {
					string key = columns[0];
					string value = columns[1];
					properties.Add(key, value);
				}
			}
			
			int count = Convert.ToInt32(properties["count"]);
			mNameArray = new string[count];
			for (int i = 0; i < count; ++i)
			{
				string value;
				if (properties.TryGetValue("" + i, out value)) {
					mNameArray[i] = value;
				}
			}
		}

		/// <summary>
		/// Get the name of a controller, given its numeric value.
		/// </summary>
		/// <param name="num">The MIDI controller number.</param>
		/// <returns>The name of the controller.</returns>
		public static string GetControlName(int num)
		{
			return mNameArray[num];
		}

		// Get the array of controller names.
		/// <returns>The names of the controllers.</returns>
		public static string[] GetNameArray()
		{
			return mNameArray;
		}

		// A String used to display a 'graphical' representation
		// of the controller's value.
		internal static string meter = "---------------|---------------";

		/// <summary>
		/// Get a String representation of the controller's value.
		/// </summary>
		/// <param name="val">The controller's value</param>
		/// <param name="graphic">If true the returned string is a graphical
		// representation of the value. e.g. value 64 gives "--------|--------"</param>
		/// <returns>The value of the controller.</returns>
		public static string GetControlValue(int val, bool graphic)
		{
			string result = "";
			if (graphic)
			{
				int Start = (127 - val)/8;
				result += meter.Substring(Start, Start + 16);
			}
			else
			{
				result += val;
			}
			return result;
		}

		/// <summary>
		/// Get the controller number associated with the name.
		/// </summary>
		/// <param name="name">The name of the controller.</param>
		/// <returns>The controller number for the named controller.</returns>
		public static int GetControllerNumber(string name)
		{
			int res = 0;
			for (int i = 0; i < mNameArray.Length; ++i)
			{
				if (name.Equals(mNameArray[i]))
				{
					res = i;
					break;
				}
			}
			return res;
		}
	}
}