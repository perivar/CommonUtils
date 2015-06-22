using System;

namespace CommonUtils.CommonMath.FFT
{
	/// <summary>
	/// WindowFunction methods copied from the Mirage project:
	/// Mirage - High Performance Music Similarity and Automatic Playlist Generator
	/// http://hop.at/mirage
	///
	/// Copyright (C) 2007 Dominik Schnitzer, dominik@schnitzer.at
	/// Changed and enhanced by Per Ivar Nerseth, perivar@nerseth.com
	///
	/// This program is free software; you can redistribute it and/or
	/// modify it under the terms of the GNU General Public License
	/// as published by the Free Software Foundation; either version 2
	/// of the License, or (at your option) any later version.
	/// </summary>
	public interface IWindowFunction
	{
		void Initialize(int winsize);

		void Apply(ref float[] data, float[] audiodata, int offset);
		
		double[] GetWindow();
	}

	public class HammingWindow : IWindowFunction
	{
		int winsize;
		double[] win;
		
		public HammingWindow() {
		}

		// Initialize and setup the window
		public HammingWindow(int winsize) {
			Initialize(winsize);
		}
		
		public double[] GetWindow() {
			return win;
		}
		
		public void Initialize(int winsize)
		{
			this.winsize = winsize;
			win = new double[winsize];

			for (int i = 0; i < winsize; i++) {
				win[i] = (double)(0.54 - 0.46 * Math.Cos(2*Math.PI * ((double)i/(double)winsize)));
			}
		}
		
		public void Apply(ref float[] data, float[] audiodata, int offset)
		{
			for (int i = 0; i < winsize; i++) {
				data[i] = (float) win[i] * audiodata[i+offset];
			}
		}
	}

	public class HannWindow : IWindowFunction
	{
		int winsize;
		double[] win;
		
		public HannWindow() {
		}

		// Initialize and setup the window
		public HannWindow(int winsize) {
			Initialize(winsize);
		}

		public double[] GetWindow() {
			return win;
		}

		public void Initialize(int winsize)
		{
			this.winsize = winsize;
			win = new double[winsize];
			
			for (int i = 0; i < winsize; i++) {
				win[i] = (double)(0.5 * (1 - Math.Cos(2*Math.PI*(double)i/(winsize-1))));
			}
		}
		
		public void Apply(ref float[] data, float[] audiodata, int offset)
		{
			for (int i = 0; i < winsize; i++) {
				data[i] = (float) win[i] * audiodata[i+offset];
			}
		}
	}
}
