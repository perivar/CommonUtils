using System;

namespace CommonUtils.CommonMath.FFT
{
	public enum FFTWindowType
	{
		RECTANGULAR = 0,
		BARTLETT = 1,
		HAMMING = 2,
		HANNING = 3,
		BLACKMAN = 4,
		BLACKMAN_HARRIS = 5,
		WELCH = 6,
		GAUSSIAN = 7,
		GAUSSIAN_2_5 = 8,
		GAUSSIAN_3_5 = 9,
		GAUSSIAN_4_5 = 10,
		PARZEN = 11,
		NUTTALL = 12,
		COSINE = 13,
		TRIANGULAR = 14
	}
	
	/// <summary>
	/// FFT Window class that caches values for faster computations
	/// </summary>
	public class FFTWindow
	{
		const double PI = Math.PI;
		
		FFTWindowType m_type;
		int m_winsize;
		double[] m_win;
		double m_area;

		#region Getters
		public FFTWindowType WindowType {
			get {
				return m_type;
			}
		}

		public int WinSize {
			get {
				return m_winsize;
			}
		}

		public double[] Window {
			get {
				return m_win;
			}
		}

		public double Area {
			get {
				return m_area;
			}
		}
		#endregion

		/// <summary>
		/// Construct a window of the given type.
		/// </summary>
		/// <param name="type">window type</param>
		/// <param name="winSize">window size</param>
		public FFTWindow(FFTWindowType type, int winSize)
		{
			m_type = type;
			m_winsize = winSize;
			Initialize();
		}
		
		/// <summary>
		/// Construct a window using an existing window
		/// </summary>
		/// <param name="w">window</param>
		public FFTWindow(FFTWindow w)
		{
			m_type = w.m_type;
			m_winsize = w.m_winsize;
			Initialize();
		}

		/// <summary>
		/// Create a copy of the passed window
		/// </summary>
		/// <param name="w">window</param>
		/// <returns>a window</returns>
		public FFTWindow CopyFrom(FFTWindow w)
		{
			if (w == this) {
				return this;
			}
			
			m_type = w.m_type;
			m_winsize = w.m_winsize;
			Initialize();
			return this;
		}
		
		/// <summary>
		/// Initialize the cached window
		/// </summary>
		public void Initialize()
		{
			m_win = GetWindowFunction(m_type, m_winsize);
			
			int n = m_winsize;
			m_area = 0;
			for (int i = 0; i < n; i++)
			{
				m_area += m_win[i];
			}
			m_area /= n;
		}
		
		/// <summary>
		/// Apply a given window to the passed audio data and return in the data array
		/// </summary>
		/// <param name="data">the data to store the windowed audio data</param>
		/// <param name="audiodata">the data to be windowed</param>
		/// <param name="offset">offset within the audio data</param>
		public void Apply(ref float[] data, float[] audiodata, int offset)
		{
			for (int i = 0; i < m_winsize; i++) {
				data[i] = (float) m_win[i] * audiodata[i+offset];
			}
		}
		
		/// <summary>
		/// Perform the windowing inline
		/// </summary>
		/// <param name="src">data to perform windowing on</param>
		public void Apply(double[] src)
		{
			Apply(src, src);
		}

		/// <summary>
		/// Perform the windowing inline
		/// </summary>
		/// <param name="src">data to perform windowing on</param>
		public void Apply(float[] src)
		{
			Apply(src, src);
		}

		/// <summary>
		/// Perform windowing on the source data and store in the destination data
		/// </summary>
		/// <param name="src">source data</param>
		/// <param name="dst">destination data</param>
		public void Apply(double[] src, double[] dst)
		{
			for (int i = 0; i < m_winsize; ++i) {
				dst[i] = src[i] * m_win[i];
			}
		}

		/// <summary>
		/// Perform windowing on the source data and store in the destination data
		/// </summary>
		/// <param name="src">source data</param>
		/// <param name="dst">destination data</param>
		public void Apply(float[] src, float[] dst)
		{
			for (int i = 0; i < m_winsize; ++i) {
				dst[i] = (float) (src[i] * m_win[i]);
			}
		}
		
		/// <summary>
		/// Return the window method as a float array with a given number of points
		/// </summary>
		/// <returns>a float data array</returns>
		public double[] DrawCurve(int numPoints = 128)
		{
			return GetWindowFunction(m_type, numPoints);
		}
		
		#region Static Helper Methods
		
		/// <summary>
		/// Gets the corresponding window function.
		/// 0: Rectangular (no window)
		/// 1: Bartlett    (triangular)
		/// 2: Hamming
		/// 3: Hanning
		/// 4: Blackman
		/// 5: Blackman-Harris
		/// 6: Welch
		/// 7: Gaussian
		/// 8: Gaussian(a=2.5)
		/// 9: Gaussian(a=3.5)
		/// 10: Gaussian(a=4.5)
		/// 11: Parzen
		/// 12: Nuttall
		/// 13: Cosine
		/// 14: Triangular
		/// </summary>
		/// <param name="windowType">What FFT function to use</param>
		/// <param name="fftWindowsSize">Length of the window</param>
		/// <returns>Window function</returns>
		public static double[] GetWindowFunction(FFTWindowType windowType, int fftWindowsSize)
		{
			var array = new double[fftWindowsSize];
			
			// initialize to 1's
			for (int i = 0; i < fftWindowsSize; i++) {
				array[i] = 1;
			}
			ApplyWindowFunction(windowType, fftWindowsSize, array);
			return array;
		}
		
		/// <summary>
		/// Applies a windowing function to the data in place.
		/// 0: Rectangular (no window)
		/// 1: Bartlett    (triangular)
		/// 2: Hamming
		/// 3: Hanning
		/// 4: Blackman
		/// 5: Blackman-Harris
		/// 6: Welch
		/// 7: Gaussian
		/// 8: Gaussian(a=2.5)
		/// 9: Gaussian(a=3.5)
		/// 10: Gaussian(a=4.5)
		/// 11: Parzen
		/// 12: Nuttall
		/// 13: Cosine
		/// 14: Triangular
		/// </summary>
		/// <param name="windowType">What FFT function to use</param>
		/// <param name="fftWindowsSize">Length of the window</param>
		/// <param name="data">Data array to modify</param>
		public static void ApplyWindowFunction(FFTWindowType windowType, int fftWindowsSize, double[] data)
		{
			int i;
			double A;
			int n = fftWindowsSize;

			switch(windowType)
			{
				case FFTWindowType.RECTANGULAR:
					// Rectangular:
					// i.e. do nothing
					break;
					
				case FFTWindowType.BARTLETT:
					// Bartlett (triangular) window
					for (i = 0; i < n / 2; i++)
					{
						data[i] *= (i / (n / 2));
						data[i + (n / 2)] *= (1.0 - (i / (n / 2)));
					}
					break;
					
				case FFTWindowType.HAMMING:
					// Hamming
					/*
					for (i = 0; i < n; i++)
					{
						data[i] *= (0.54 - 0.46 * Math.Cos(2 * PI * i / (n - 1)));
					}
					 */
					CosineWin(data, n, 0.54, 0.46, 0.0, 0.0);
					break;
					
				case FFTWindowType.HANNING:
					// Hanning
					/*
					for (i = 0; i < n; i++)
					{
						data[i] *= (0.50 - 0.50 * Math.Cos(2 * PI * i / (n - 1)));
						// this is the same as 0.5 * (1 - Math.Cos(2 * PI * i / (length - 1)));
					}
					 */
					CosineWin(data, n, 0.50, 0.50, 0.0, 0.0);
					break;
					
				case FFTWindowType.BLACKMAN:
					// Blackman
					/*
					for (i = 0; i < n; i++)
					{
						data[i] *= (0.42 - 0.5 * Math.Cos (2 * PI * i / (n - 1)) + 0.08 * Math.Cos (4 * PI * i / (n - 1)));
					}
					 */
					CosineWin(data, n, 0.42, 0.50, 0.08, 0.0);
					break;
					
				case FFTWindowType.BLACKMAN_HARRIS:
					// Blackman-Harris
					/*
					for (i = 0; i < n; i++)
					{
						data[i] *= (0.35875 - 0.48829 * Math.Cos(2 * PI * i /(n-1)) + 0.14128 * Math.Cos(4 * PI * i/(n-1)) - 0.01168 * Math.Cos(6 * PI * i/(n-1)));
					}
					 */
					CosineWin(data, n, 0.35875, 0.48829, 0.14128, 0.01168);
					break;
					
				case FFTWindowType.WELCH:
					// Welch
					for (i = 0; i < n; i++)
					{
						data[i] *= 4 *i/n*(1-(i/n));
					}
					break;
					
				case FFTWindowType.GAUSSIAN:
					for (i = 0; i < n; i++)
					{
						data[i] *= Math.Pow(2, - Math.Pow((i - (n-1)/2.0) / ((n-1)/2.0 / 3), 2));
					}
					break;

				case FFTWindowType.GAUSSIAN_2_5:
					// Gaussian (a=2.5)
					// Precalculate some values, and simplify the formula to try and reduce overhead
					A = -2 * 2.5 * 2.5;
					for (i = 0; i < n; i++)
					{
						// reduced
						data[i] *= Math.Exp(A*(0.25 + ((i/n)*(i/n)) - (i/n)));
					}
					break;
					
				case FFTWindowType.GAUSSIAN_3_5:
					// Gaussian (a=3.5)
					// Precalculate some values, and simplify the formula to try and reduce overhead
					A = -2 * 3.5 * 3.5;
					for (i = 0; i < n; i++)
					{
						// reduced
						data[i] *= Math.Exp(A*(0.25 + ((i/n)*(i/n)) - (i/n)));
					}
					break;
					
				case FFTWindowType.GAUSSIAN_4_5:
					// Gaussian (a=4.5)
					// Precalculate some values, and simplify the formula to try and reduce overhead
					A = -2 * 4.5 * 4.5;
					for (i = 0; i < n; i++)
					{
						// reduced
						data[i] *= Math.Exp(A*(0.25 + ((i/n)*(i/n)) - (i/n)));
					}
					
					break;
					
				case FFTWindowType.PARZEN:
					{
						int N = n-1;
						for (i = 0; i < N/4; ++i)
						{
							double m = 2 * Math.Pow(1.0 - ((double)N/2 - i) / ((double)N/2), 3);
							data[i] *= m;
							data[N-i] *= m;
						}
						for (i = N/4; i <= N/2; ++i)
						{
							int wn = i - N/2;
							double m = 1.0 - 6 * Math.Pow(wn / ((double)N/2), 2) * (1.0 - Math.Abs(wn) / ((double)N/2));
							data[i] *= m;
							data[N-i] *= m;
						}
						break;
					}
					
				case FFTWindowType.NUTTALL:
					CosineWin(data, n, 0.3635819, 0.4891775, 0.1365995, 0.0106411);
					break;
					
				case FFTWindowType.COSINE:
					for(i = 0; i < n; i++)
					{
						data[i] *= Math.Cos((PI * i) / (n - 1) - (PI / 2));
					}
					break;
					
				case FFTWindowType.TRIANGULAR:
					for(i = 0; i < n; i++)
					{
						data[i] *= ((2.0 / n) * ((n / 2.0) - Math.Abs(i - (n - 1) / 2.0)));
					}
					break;
					
				default:
					throw new ArgumentException("FFTWindows - Invalid window function", "windowType");
			}
		}
		
		/// <summary>
		/// Returns the name of the windowing function (for UI display)
		/// </summary>
		/// <param name="windowType"></param>
		/// <returns></returns>
		public static String GetNameForType(FFTWindowType windowType)
		{
			switch (windowType)
			{
				default:
				case FFTWindowType.RECTANGULAR:
					return "Rectangular";
				case FFTWindowType.BARTLETT:
					return "Bartlett";
				case FFTWindowType.HAMMING:
					return "Hamming";
				case FFTWindowType.HANNING:
					return "Hanning";
				case FFTWindowType.BLACKMAN:
					return "Blackman";
				case FFTWindowType.BLACKMAN_HARRIS:
					return "Blackman-Harris";
				case FFTWindowType.WELCH:
					return "Welch";
				case FFTWindowType.GAUSSIAN:
					return "Gaussian";
				case FFTWindowType.GAUSSIAN_2_5:
					return "Gaussian(a=2.5)";
				case FFTWindowType.GAUSSIAN_3_5:
					return "Gaussian(a=3.5)";
				case FFTWindowType.GAUSSIAN_4_5:
					return "Gaussian(a=4.5)";
				case FFTWindowType.PARZEN:
					return "Parzen";
				case FFTWindowType.NUTTALL:
					return "Nuttall";
				case FFTWindowType.COSINE:
					return "Cosine";
				case FFTWindowType.TRIANGULAR:
					return "Triangular";
			}
		}

		/// <summary>
		/// Return the windowing function using a lowercase string value
		/// </summary>
		/// <param name="name">lowercase window name</param>
		/// <returns>the window type</returns>
		public static FFTWindowType GetTypeForName(string name)
		{
			if (name == "rectangular")
				return FFTWindowType.RECTANGULAR;
			if (name == "bartlett")
				return FFTWindowType.BARTLETT;
			if (name == "hamming")
				return FFTWindowType.HAMMING;
			if (name == "hanning")
				return FFTWindowType.HANNING;
			if (name == "blackman")
				return FFTWindowType.BLACKMAN;
			if (name == "blackman-harris")
				return FFTWindowType.BLACKMAN_HARRIS;
			if (name == "welch")
				return FFTWindowType.WELCH;
			if (name == "gaussian")
				return FFTWindowType.GAUSSIAN;
			if (name == "gaussian25")
				return FFTWindowType.GAUSSIAN_2_5;
			if (name == "gaussian35")
				return FFTWindowType.GAUSSIAN_3_5;
			if (name == "gaussian45")
				return FFTWindowType.GAUSSIAN_4_5;
			if (name == "parzen")
				return FFTWindowType.PARZEN;
			if (name == "nuttall")
				return FFTWindowType.NUTTALL;
			if (name == "cosine")
				return FFTWindowType.COSINE;
			if (name == "triangular")
				return FFTWindowType.TRIANGULAR;
			
			Console.Error.WriteLine("WARNING: Window.GetTypeForName: unknown name \"{0}\", defaulting to \"hanning\"", name);
			return FFTWindowType.HANNING;
		}
		
		/// <summary>
		/// Returns the number of windowing functions supported
		/// </summary>
		/// <returns></returns>
		public static int NumberOfWindowFunctions()
		{
			return Enum.GetNames(typeof(FFTWindowType)).Length;
		}
		
		/// <summary>
		/// Cosine Window Method
		/// </summary>
		/// <param name="mult">array to store the result in</param>
		/// <param name="n">length</param>
		/// <param name="a0">a0</param>
		/// <param name="a1">a1</param>
		/// <param name="a2">a2</param>
		/// <param name="a3">a3</param>
		private static void CosineWin(double[] mult, int n, double a0, double a1, double a2, double a3)
		{
			for (int i = 0; i < n; ++i)
			{
				mult[i] *= (a0
				            - a1 * Math.Cos((2 * PI * i) / n)
				            + a2 * Math.Cos((4 * PI * i) / n)
				            - a3 * Math.Cos((6 * PI * i) / n));
			}
		}
		
		#endregion
	}
}


