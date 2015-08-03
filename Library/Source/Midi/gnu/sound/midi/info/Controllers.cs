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

		// Get the name of a controller, given its numeric value.
		// @param num The MIDI controller number.
		// @return The name of the controller.
		public static string GetControlName(int num)
		{
			return mNameArray[num];
		}

		// Get the array of controller names.
		// @return The names of the controllers.
		public static string[] GetNameArray()
		{
			return mNameArray;
		}

		// A String used to display a 'graphical' representation
		// of the controller's value.
		internal static string meter = "---------------|---------------";

		// Get a String representation of the controller's value.
		// @param val The controller's value
		// @param graphic If true the returned string is a graphical
		// representation of the value. e.g. value 64 gives "--------|--------"
		// @return The value of the controller.
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

		// Get the controller number associated with the name.
		// @param name The name of the controller.
		// @return The controller number for the named controller.
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