using System;

namespace CommonUtils.CommonMath.Filters
{
	/// <summary>
	/// Description of MelUtils.
	/// </summary>
	public static class MelUtils
	{
		/// <summary>
		/// Compute mel frequency from linear frequency.
		/// </summary>
		/// <param name="inputFreq">the input frequency in linear scale</param>
		/// <returns>the frequency in a mel scale</returns>
		public static double LinToMelFreq(double inputFreq) {
			return (2595.0 * Math.Log10(1.0 + inputFreq / 700.0));
		}
		
		/// <summary>
		/// Compute linear frequency from mel frequency.
		/// </summary>
		/// <param name="inputFreq">the input frequency in mel scale</param>
		/// <returns>the frequency in a linear scale</returns>
		public static double MelToLinFreq(double inputFreq) {
			return (700.0 * (Math.Pow(10.0, (inputFreq / 2595.0)) - 1.0));
		}
	}
}
