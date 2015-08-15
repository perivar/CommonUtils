/*
    QM Vamp Plugin Set

    Centre for Digital Music, Queen Mary, University of London.

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License as
    published by the Free Software Foundation; either version 2 of the
    License, or (at your option) any later version.  See the file
    COPYING included with this distribution for more information.
 */

using System;

namespace CommonUtils.CommonMath.FFT.VampPlugins {
	
	public enum WindowType
	{
		RectangularWindow,
		BartlettWindow,
		HammingWindow,
		HanningWindow,
		BlackmanWindow,
		GaussianWindow,
		ParzenWindow,
		NuttallWindow,
		BlackmanHarrisWindow
	}

	public class Window
	{
		const double M_PI = Math.PI;
		
		/**
		 * Construct a windower of the given type.
		 */
		public Window(WindowType type, int size)
		{
			m_type = type;
			m_size = size;
			EnCache();
		}
		
		public Window(Window w)
		{
			m_type = w.m_type;
			m_size = w.m_size;
			EnCache();
		}

		public Window CopyFrom(Window w)
		{
			if (w == this) {
				return this;
			}
			
			m_type = w.m_type;
			m_size = w.m_size;
			EnCache();
			return this;
		}
		
		public virtual void Dispose()
		{
			m_cache = null;
		}

		public void Cut(double[] src)
		{
			Cut(src, src);
		}

		public void Cut(double[] src, double[] dst)
		{
			for (uint i = 0; i < m_size; ++i) {
				dst[i] = src[i] * m_cache[i];
			}
		}

		public double GetArea()
		{
			return m_area;
		}
		
		public double GetValue(uint i)
		{
			return m_cache[i];
		}

		public WindowType GetType()
		{
			return m_type;
		}

		public int GetSize()
		{
			return m_size;
		}

		// The names used by these functions are un-translated, for use in
		// e.g. XML I/O.  Use Preferences::getPropertyValueLabel if you
		// want translated names for use in the user interface.
		public static string GetNameForType(WindowType type)
		{
			switch (type)
			{
				case WindowType.RectangularWindow:
					return "rectangular";
				case WindowType.BartlettWindow:
					return "bartlett";
				case WindowType.HammingWindow:
					return "hamming";
				case WindowType.HanningWindow:
					return "hanning";
				case WindowType.BlackmanWindow:
					return "blackman";
				case WindowType.GaussianWindow:
					return "gaussian";
				case WindowType.ParzenWindow:
					return "parzen";
				case WindowType.NuttallWindow:
					return "nuttall";
				case WindowType.BlackmanHarrisWindow:
					return "blackman-harris";
			}
			
			Console.Error.WriteLine("WARNING: Window::getNameForType: unknown type {0}", type);
			
			return "unknown";
		}
		
		public static WindowType GetTypeForName(string name)
		{
			if (name == "rectangular")
				return WindowType.RectangularWindow;
			if (name == "bartlett")
				return WindowType.BartlettWindow;
			if (name == "hamming")
				return WindowType.HammingWindow;
			if (name == "hanning")
				return WindowType.HanningWindow;
			if (name == "blackman")
				return WindowType.BlackmanWindow;
			if (name == "gaussian")
				return WindowType.GaussianWindow;
			if (name == "parzen")
				return WindowType.ParzenWindow;
			if (name == "nuttall")
				return WindowType.NuttallWindow;
			if (name == "blackman-harris")
				return WindowType.BlackmanHarrisWindow;
			
			Console.Error.WriteLine("WARNING: Window::getTypeForName: unknown name \"{0}\", defaulting to \"hanning\"", name);
			
			return WindowType.HanningWindow;
		}

		protected WindowType m_type;
		protected int m_size;
		protected double[] m_cache;
		protected double m_area;

		protected void EnCache()
		{
			int n = m_size;
			var mult = new double[n];
			int i;
			
			for (i = 0; i < n; ++i) {
				mult[i] = 1.0;
			}
			
			switch (m_type)
			{
				case WindowType.RectangularWindow:
					for (i = 0; i < n; ++i)
					{
						mult[i] *= 0.5;
					}
					break;
					
				case WindowType.BartlettWindow:
					for (i = 0; i < n/2; ++i)
					{
						mult[i] *= (i / (double)(n/2));
						mult[i + n/2] *= (1.0 - (i / (double)(n/2)));
					}
					break;
					
				case WindowType.HammingWindow:
					CosineWin(mult, 0.54, 0.46, 0.0, 0.0);
					break;
					
				case WindowType.HanningWindow:
					CosineWin(mult, 0.50, 0.50, 0.0, 0.0);
					break;
					
				case WindowType.BlackmanWindow:
					CosineWin(mult, 0.42, 0.50, 0.08, 0.0);
					break;
					
				case WindowType.GaussianWindow:
					for (i = 0; i < n; ++i)
					{
						mult[i] *= Math.Pow(2, - Math.Pow((i - (n-1)/2.0) / ((n-1)/2.0 / 3), 2));
					}
					break;
					
				case WindowType.ParzenWindow:
					{
						int N = n-1;
						for (i = 0; i < N/4; ++i)
						{
							double m = 2 * Math.Pow(1.0 - ((double)N/2 - i) / ((double)N/2), 3);
							mult[i] *= m;
							mult[N-i] *= m;
						}
						for (i = N/4; i <= N/2; ++i)
						{
							int wn = i - N/2;
							double m = 1.0 - 6 * Math.Pow(wn / ((double)N/2), 2) * (1.0 - Math.Abs(wn) / ((double)N/2));
							mult[i] *= m;
							mult[N-i] *= m;
						}
						break;
					}
					
				case WindowType.NuttallWindow:
					CosineWin(mult, 0.3635819, 0.4891775, 0.1365995, 0.0106411);
					break;
					
				case WindowType.BlackmanHarrisWindow:
					CosineWin(mult, 0.35875, 0.48829, 0.14128, 0.01168);
					break;
			}
			
			m_cache = mult;
			
			m_area = 0;
			for (i = 0; i < n; ++i)
			{
				m_area += m_cache[i];
			}
			m_area /= n;
		}
		
		protected void CosineWin(double[] mult, double a0, double a1, double a2, double a3)
		{
			int n = m_size;
			for (int i = 0; i < n; ++i)
			{
				mult[i] *= (a0 - a1 * Math.Cos((2 * M_PI * i) / n) + a2 * Math.Cos((4 * M_PI * i) / n) - a3 * Math.Cos((6 * M_PI * i) / n));
			}
		}
	}
}