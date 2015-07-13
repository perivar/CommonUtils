using System;
using System.Diagnostics;

namespace CommonUtils
{
	/// <summary>
	/// A simple timer class
	/// </summary>
	public class TimerUtil
	{
		private readonly Stopwatch _watch;

		/// <summary>
		/// Create a simple timer class
		/// <example>
		/// var timer = new TimerUtil();
		/// Thread.Sleep(10000);
		/// var elapsed = timer.Stop();
		/// </example>
		/// </summary>
		public TimerUtil() {
			_watch = Stopwatch.StartNew();
		}

		public TimeSpan Stop()
		{
			_watch.Stop();
			
			// Get the elapsed time as a TimeSpan value.
			return _watch.Elapsed;
		}
	}
}
