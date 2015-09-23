using System;
using System.Linq;
using System.Globalization;

namespace CommonUtils
{
	/// <summary>
	/// Assorted number methods that might be helpful
	/// </summary>
	public static class NumberUtils
	{
		/// <summary>
		/// Parse a string into a decimal object treating both comma and dot as decimal point.
		/// This method uses the InvariantCulture locale.
		/// If it fails return 0.
		/// </summary>
		/// <param name="input">string</param>
		/// <returns>parsed decimal or zero</returns>
		public static Decimal DecimalTryParseDecimalPointOrZero(String input) {
			input = input.Replace(',', '.');
			Decimal d;
			Decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out d);
			return d;
		}
		
		/// <summary>
		/// Parse a string into decimal object. If it fails return 0.
		/// This method uses the default locale.
		/// </summary>
		/// <param name="input">string</param>
		/// <returns>parsed decimal or zero</returns>
		public static Decimal DecimalTryParseOrZero(String input) {
			Decimal d;
			Decimal.TryParse(input, out d);
			return d;
		}

		/// <summary>
		/// Parse a string into decimal object. If it fails return the passed default value.
		/// This method uses the default locale.
		/// </summary>
		/// <param name="input">string</param>
		/// <param name="defaultValue">default value to use if parsing fails</param>
		/// <returns>parsed decimal or passed default value</returns>
		public static Decimal DecimalTryParse(String input, decimal defaultValue) {
			Decimal result;
			if (!Decimal.TryParse(input, out result)) {
				result = defaultValue;
			}
			return result;
		}

		/// <summary>
		/// Parse a string into an int object. If it fails return the passed default value.
		/// This method uses the default locale.
		/// </summary>
		/// <param name="input">string</param>
		/// <param name="defaultValue">default value to use if parsing fails</param>
		/// <returns>parsed int or passed default value</returns>
		public static int IntTryParse(String input, int defaultValue) {
			int result;
			if (!int.TryParse(input, out result)) {
				result = defaultValue;
			}
			return result;
		}
		
		/// <summary>
		/// Parse a string into a boolean. If it fails return false.
		/// Supports 0, off, no and false for returning false.
		/// </summary>
		/// <param name="input">string</param>
		/// <returns>a boolean</returns>
		public static Boolean BooleanTryParseOrZero(String input)
		{
			return BooleanTryParse(input, false);
		}

		/// <summary>
		/// Parse a string into a boolean. If it fails return default value.
		/// Supports 0, off, no and false for returning false.
		/// </summary>
		/// <param name="input">string</param>
		/// <param name="defaultValue">default value to use if parsing fails</param>
		/// <returns>a boolean</returns>
		public static Boolean BooleanTryParse(String input, Boolean defaultValue)
		{
			String[] BooleanStringOff = { "0", "off", "no", "false" };

			if (input == null) {
				return defaultValue;
			} else if (input.Equals("")) {
				return defaultValue;
			} else if(BooleanStringOff.Contains(input,StringComparer.InvariantCultureIgnoreCase)) {
				return false;
			}

			Boolean result;
			if (!Boolean.TryParse(input, out result)) {
				result = true;
			}

			return result;
		}
		
		/// <summary>
		/// Parse a string into double object. If it fails return the passed default value.
		/// This method tries several locales before giving up.
		/// </summary>
		/// <param name="input">string</param>
		/// <param name="defaultValue">default value to use if parsing fails</param>
		/// <returns>a double</returns>
		public static double DoubleTryParse(String input, double defaultValue) {
			double result;

			//Try parsing in the current culture
			if (!double.TryParse(input, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out result) &&
			    //Then try in US english
			    !double.TryParse(input, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
			    //Then in neutral language
			    !double.TryParse(input, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result))
			{
				result = defaultValue;
			}
			
			return result;
		}
	}
}
