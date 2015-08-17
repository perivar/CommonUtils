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
using System.Threading.Tasks;
using System.Collections.Generic;
using Lomont;
using CommonUtils.CommonMath.FFT;

namespace CommonUtils.CommonMath.FFT.VampPlugins {
	
	/// <summary>
	/// A ported versjon of the Adaptive Spectrogram Vamp Plugin
	/// </summary>
	public class AdaptiveSpectrogram
	{
		/// <summary>
		/// Sample rate
		/// </summary>
		protected float sampleRate;
		
		/// <summary>
		/// Smallest resolution
		/// Smallest of the consecutive powers of two to use as spectrogram resolutions
		/// </summary>
		protected int m_w;
		
		/// <summary>
		/// Number of resolutions
		/// Number of consecutive powers of two in the range to be used as spectrogram resolutions,
		/// starting with the minimum resolution specified
		/// </summary>
		protected int m_n;
		
		/// <summary>
		/// Omit alternate resolutions
		/// Generate a coarser spectrogram faster by excluding every alternate resolution (first and last resolution are always retained)
		/// </summary>
		protected bool m_coarse;
		
		/// <summary>
		/// Multi-threaded processing
		/// Perform calculations using several threads in parallel
		/// </summary>
		protected bool m_threaded;
		
		// Thread variables for multi-threaded processing
		protected List<CutThread> m_cutThreads = new List<CutThread>();
		protected bool m_threadsInUse;
		protected Dictionary<int, FFTThread> m_fftThreads = new Dictionary<int, FFTThread>();

		public AdaptiveSpectrogram(float inputSampleRate)
		{
			sampleRate = inputSampleRate;
			
			// original defaults
			// m_w: 8, m_n: 2 => min window: 512, max window: 2048, 3 iterations
			m_w = 8; 	// One less than the GUI, e.g. m_w: 8 is really 9 => 512
			m_n = 2;	// One less than the GUI, e.g. m_n: 2 is really 3
			
			m_coarse = false;
			m_threaded = false;
			m_threadsInUse = false;
		}
		
		public virtual void Dispose()
		{
			m_cutThreads.Clear();
			m_fftThreads.Clear();
		}

		public static string GetName()
		{
			return "Adaptive Spectrogram";
		}

		public static string GetDescription()
		{
			return "Produce an adaptive spectrogram by adaptive selection from spectrograms at multiple resolutions";
		}

		public static string GetMaker()
		{
			return "Queen Mary, University of London";
		}

		public static string GetCopyright()
		{
			return "Plugin by Wen Xue and Chris Cannam.  Copyright (c) 2009 Wen Xue and QMUL - All Rights Reserved";
		}

		/// <summary>
		/// Return window increment (step size)
		/// </summary>
		/// <returns></returns>
		public int GetPreferredStepSize()
		{
			return ((2 << m_w) << m_n) / 2;
		}

		/// <summary>
		/// Return the number of Audio frames per block
		/// </summary>
		/// <returns></returns>
		public int GetPreferredBlockSize()
		{
			return (2 << m_w) << m_n;
		}
		
		/// <summary>
		/// Using the GUI window value (which is one less than the version used in the code)
		/// return the actual window length used
		/// </summary>
		/// <param name="w">window integer</param>
		/// <returns>actual window length used</returns>
		public static string GetWindowDescription(int w) {
			switch (w) {
				case 1:
					return "2";
				case 2:
					return "4";
				case 3:
					return "8";
				case 4:
					return "16";
				case 5:
					return "32";
				case 6:
					return "64";
				case 7:
					return "128";
				case 8:
					return "256";
				case 9:
					return "512";
				case 10:
					return "1024";
				case 11:
					return "2048";
				case 12:
					return "4096";
				case 13:
					return "8192";
				case 14:
					return "16384";
				default:
					return "no window";
			}
		}

		/*
	public AdaptiveSpectrogram.ParameterList getParameterDescriptors()
	{
		ParameterList list = new ParameterList();
		
		ParameterDescriptor desc = new ParameterDescriptor();
		desc.identifier = "n";
		desc.name = "Number of resolutions";
		desc.description = "Number of consecutive powers of two in the range to be used as spectrogram resolutions, starting with the minimum resolution specified";
		desc.unit = "";
		desc.minValue = 2;
		desc.maxValue = 10;
		desc.defaultValue = 3;
		desc.isQuantized = true;
		desc.quantizeStep = 1;
		list.push_back(desc);
		
		ParameterDescriptor desc2 = new ParameterDescriptor();
		desc2.identifier = "w";
		desc2.name = "Smallest resolution";
		desc2.description = "Smallest of the consecutive powers of two to use as spectrogram resolutions";
		desc2.unit = "";
		desc2.minValue = 1;
		desc2.maxValue = 14;
		desc2.defaultValue = 9;
		desc2.isQuantized = true;
		desc2.quantizeStep = 1;
		
		// I am so lazy
		desc2.valueNames.push_back("2");
		desc2.valueNames.push_back("4");
		desc2.valueNames.push_back("8");
		desc2.valueNames.push_back("16");
		desc2.valueNames.push_back("32");
		desc2.valueNames.push_back("64");
		desc2.valueNames.push_back("128");
		desc2.valueNames.push_back("256");
		desc2.valueNames.push_back("512");
		desc2.valueNames.push_back("1024");
		desc2.valueNames.push_back("2048");
		desc2.valueNames.push_back("4096");
		desc2.valueNames.push_back("8192");
		desc2.valueNames.push_back("16384");
		list.push_back(desc2);
		
		ParameterDescriptor desc3 = new ParameterDescriptor();
		desc3.identifier = "coarse";
		desc3.name = "Omit alternate resolutions";
		desc3.description = "Generate a coarser spectrogram faster by excluding every alternate resolution (first and last resolution are always retained)";
		desc3.unit = "";
		desc3.minValue = 0;
		desc3.maxValue = 1;
		desc3.defaultValue = 0;
		desc3.isQuantized = true;
		desc3.quantizeStep = 1;
		list.push_back(desc3);
		
		desc3.identifier = "threaded";
		desc3.name = "Multi-threaded processing";
		desc3.description = "Perform calculations using several threads in parallel";
		desc3.unit = "";
		desc3.minValue = 0;
		desc3.maxValue = 1;
		desc3.defaultValue = 1;
		desc3.isQuantized = true;
		desc3.quantizeStep = 1;
		list.push_back(desc3);
		
		return list;
	}
		 */

		/*
		public AdaptiveSpectrogram.OutputList getOutputDescriptors()
		{
			OutputList list = new OutputList();
			
			OutputDescriptor d = new OutputDescriptor();
			d.identifier = "output";
			d.name = "Output";
			d.description = "The output of the plugin";
			d.unit = "";
			d.hasFixedBinCount = true;
			d.binCount = getPreferredBlockSize() / 2;
			d.hasKnownExtents = false;
			d.isQuantized = false;
			d.sampleType = OutputDescriptor.FixedSampleRate;
			d.sampleRate = m_inputSampleRate / ((2 << m_w) / 2);
			d.hasDuration = false;
			string name = new string(new char[20]);
			for (int i = 0; i < d.binCount; ++i)
			{
				float freq = (m_inputSampleRate / (d.binCount * 2)) * (i + 1); // no DC bin
				name = string.Format("{0:D} Hz", (int)freq);
				d.binNames.push_back(name);
			}
			list.push_back(d);
			
			return list;
		}
		 */
		
		#region Get and Set parameter methods
		public float GetParameter(string id)
		{
			switch (id) {
				case "n":
					return m_n + 1;
				case "w":
					return m_w + 1;
				case "threaded":
					return (m_threaded ? 1 : 0);
				case "coarse":
					return (m_coarse ? 1 : 0);
				default:
					return 0.0f;
			}
		}
		
		public void SetParameter(string id, float value)
		{
			switch (id) {
				case "n":
					int n = Convert.ToInt32(value);
					if (n >= 1 && n <= 10) {
						m_n = n - 1;
					}
					break;
				case "w":
					int w = Convert.ToInt32(value);
					if (w >= 1 && w <= 14) {
						m_w = w - 1;
					}
					break;
				case "threaded":
					m_threaded = (value > 0.5);
					break;
				case "coarse":
					m_coarse = (value > 0.5);
					break;
			}
		}
		#endregion

		public double[][] Process(float[] inputBuffers)
		{
			int minwid = (2 << m_w); 			// m_w: 8 => 512, 9 => 1024
			int maxwid = ((2 << m_w) << m_n); 	// m_w: 8, m_n: 2 => 2048
			// m_w: 9, m_n: 3 => 8192
			
			#if DEBUGVERBOSE
			Console.WriteLine("Widths from {0} to {1} ({2} to {3} in real parts)", minwid, maxwid, minwid/2, maxwid/2);
			#endif
			
			var s = new Spectrograms(minwid/2, maxwid/2, 1);
			
			int w = minwid;
			int index = 0;
			
			while (w <= maxwid)
			{
				if (!IsResolutionWanted(s, w/2))
				{
					w *= 2;
					++index;
					continue;
				}
				
				if (!m_fftThreads.ContainsKey(w))
				{
					m_fftThreads.Add(w, new FFTThread(w));
				}
				if (m_threaded)
				{
					m_fftThreads[w].StartCalculation(inputBuffers, ref s, index, maxwid);
				}
				else
				{
					m_fftThreads[w].SetParameters(inputBuffers, ref s, index, maxwid);
					m_fftThreads[w].PerformTask();
				}
				w *= 2;
				++index;
			}
			
			if (m_threaded)
			{
				w = minwid;
				index = 0;
				while (w <= maxwid)
				{
					if (!IsResolutionWanted(s, w/2))
					{
						w *= 2;
						++index;
						continue;
					}
					m_fftThreads[w].Await();
					w *= 2;
					++index;
				}
			}
			
			m_threadsInUse = false;
			
			#if DEBUGVERBOSE
			Console.WriteLine("maxwid/2 = {0}, minwid/2 = {1}, n+1 = {2}, 2^(n+1) = {3}", maxwid/2, minwid/2, m_n+1, (2<<m_n));
			#endif
			
			int cutwid = maxwid/2;

			Cutting cutting = Cut(s, cutwid, 0, 0, cutwid);
			
			#if DEBUGVERBOSE
			PrintCutting(cutting, "  ");
			#endif
			
			var rmat = new double[maxwid/minwid][];
			for (int i = 0; i < maxwid/minwid; ++i)
			{
				rmat[i] = new double[maxwid/2];
			}
			
			Assemble(s, cutting, ref rmat, 0, 0, maxwid/minwid, cutwid);
			cutting.Erase();
			
			return rmat;
		}

		#region Math util methods
		public static int Log2(double val) {
			return (int) Math.Log(val, 2);
		}
		
		protected static double XLogX(double x)
		{
			if (x == 0.0) {
				return 0.0;
			} else {
				return x * Math.Log(x);
			}
		}
		
		protected static double Log(double x)
		{
			if (x == 0.0) {
				return 0.0;
			} else {
				return Math.Log(x);
			}
		}
		#endregion

		protected static double Cost(Spectrogram s, int x, int y)
		{
			return XLogX(s.data[x][y]);
		}

		protected static double Value(Spectrogram s, int x, int y)
		{
			return s.data[x][y];
		}

		protected static double Normalize(double vcost, double venergy)
		{
			return (vcost + (venergy * Log(venergy))) / venergy;
		}

		protected bool IsResolutionWanted(Spectrograms s, int res)
		{
			if (!m_coarse) {
				return true;
			}
			if (res == s.minres || res == s.maxres) {
				return true;
			}
			int n = 0;
			for (int r = res; r > s.minres; r >>= 1) {
				++n;
			}
			return ((n & 0x1) == 0);
		}

		// recursively cut the spectrogram into small pieces
		protected Cutting Cut(Spectrograms s, int res, int x, int y, int h)
		{
			#if DEBUGVERBOSE
			Console.WriteLine("res = {0}, x = {1}, y = {2}, h = {3}", res, x, y, h);
			#endif
			
			var cutting = new Cutting();
			
			if (h > 1 && res > s.minres)
			{
				if (!IsResolutionWanted(s, res))
				{
					var left = new Cutting();
					var right = new Cutting();
					//GetSubCuts(s, res, x, y, h, null, null, ref left, ref right);
					
					double hcost = left.cost + right.cost;
					double henergy = left.value + right.value;
					hcost = Normalize(hcost, henergy);
					
					cutting.cut = Cutting.Cut.Horizontal;
					cutting.first = left;
					cutting.second = right;
					cutting.cost = hcost;
					cutting.value = left.value + right.value;
				}
				else if (h == 2 && !IsResolutionWanted(s, res/2))
				{
					var top = new Cutting();
					var bottom = new Cutting();
					//GetSubCuts(s, res, x, y, h, ref top, ref bottom, null, null);
					
					double vcost = top.cost + bottom.cost;
					double venergy = top.value + bottom.value;
					vcost = Normalize(vcost, venergy);
					
					cutting.cut = Cutting.Cut.Vertical;
					cutting.first = top;
					cutting.second = bottom;
					cutting.cost = vcost;
					cutting.value = top.value + bottom.value;
				}
				else
				{
					var top = new Cutting();
					var bottom = new Cutting();
					var left = new Cutting();
					var right = new Cutting();
					GetSubCuts(s, res, x, y, h, ref top, ref bottom, ref left, ref right);
					
					double vcost = top.cost + bottom.cost;
					double venergy = top.value + bottom.value;
					vcost = Normalize(vcost, venergy);
					
					double hcost = left.cost + right.cost;
					double henergy = left.value + right.value;
					hcost = Normalize(hcost, henergy);
					
					if (vcost > hcost)
					{
						cutting.cut = Cutting.Cut.Horizontal;
						cutting.first = left;
						cutting.second = right;
						cutting.cost = hcost;
						cutting.value = left.value + right.value;
						top.Erase();
						bottom.Erase();
						return cutting;
					}
					else
					{
						cutting.cut = Cutting.Cut.Vertical;
						cutting.first = top;
						cutting.second = bottom;
						cutting.cost = vcost;
						cutting.value = top.value + bottom.value;
						left.Erase();
						right.Erase();
						return cutting;
					}
				}
			}
			else
			{
				// no cuts possible from this level
				cutting.cut = Cutting.Cut.Finished;
				cutting.first = null;
				cutting.second = null;
				
				int n = 0;
				for (int r = res; r > s.minres; r >>= 1) {
					++n;
				}
				
				Spectrogram spectrogram = s.spectrograms[n];
				cutting.cost = Cost(spectrogram, x, y);
				cutting.value = Value(spectrogram, x, y);
			}
			
			return cutting;
		}
		
		protected void GetSubCuts(Spectrograms s, int res, int x, int y, int h, ref Cutting top, ref Cutting bottom, ref Cutting left, ref Cutting right)
		{
			if (m_threaded && !m_threadsInUse)
			{
				m_threadsInUse = true;
				
				if (m_cutThreads.Count == 0)
				{
					for (int i = 0; i < 4; ++i)
					{
						var t = new CutThread(this);
						m_cutThreads.Add(t);
					}
				}
				
				// Cut threads 0 and 1 calculate the top and bottom halves;
				// threads 2 and 3 calculate left and right. See notes in
				// unthreaded code below for more information.
				
				if (top != null)
					m_cutThreads[0].Cut(s, res, x, y + h/2, h/2);
				if (bottom != null)
					m_cutThreads[1].Cut(s, res, x, y, h/2);
				if (left != null)
					m_cutThreads[2].Cut(s, res/2, 2 * x, y/2, h/2);
				if (right != null)
					m_cutThreads[3].Cut(s, res/2, 2 * x + 1, y/2, h/2);
				
				
				// you shouldn't get here before all the cut threads have finished
				
				if (top != null)
					top = m_cutThreads[0].Get();
				if (bottom != null)
					bottom = m_cutThreads[1].Get();
				if (left != null)
					left = m_cutThreads[2].Get();
				if (right != null)
					right = m_cutThreads[3].Get();
				
			}
			else
			{
				// Unthreaded version
				
				// The "vertical" division is a top/bottom split.
				// Splitting this way keeps us in the same resolution,
				// but with two vertical subregions of height h/2.
				
				if (top != null)
					top = Cut(s, res, x, y + h/2, h/2);

				if (bottom != null)
					bottom = Cut(s, res, x, y, h/2);
				
				// The "horizontal" division is a left/right split.
				// Splitting this way places us in resolution res/2, which has lower
				// vertical resolution but higher horizontal resolution.
				// We need to double x accordingly.
				
				if (left != null)
					left = Cut(s, res/2, 2 * x, y/2, h/2);

				if (right != null)
					right = Cut(s, res/2, 2 * x + 1, y/2, h/2);
			}
		}

		protected static void PrintCutting(Cutting c, string pfx)
		{
			if (c.first != null)
			{
				if (c.cut == Cutting.Cut.Horizontal)
				{
					Console.WriteLine("{0}H", pfx);
				}
				else if (c.cut == Cutting.Cut.Vertical)
				{
					Console.WriteLine("{0}V", pfx);
				}
				PrintCutting(c.first, pfx + "  ");
				PrintCutting(c.second, pfx + "  ");
			}
			else
			{
				Console.WriteLine("{0}* {1}", pfx, c.value);
			}
		}

		protected void Assemble(Spectrograms s, Cutting cutting, ref double[][] rmat, int x, int y, int w, int h)
		{
			switch (cutting.cut)
			{
				case Cutting.Cut.Finished:
					for (int i = 0; i < w; ++i)
					{
						for (int j = 0; j < h; ++j)
						{
							rmat[x+i][y+j] = cutting.value;
						}
					}
					return;
					
				case Cutting.Cut.Horizontal:
					Assemble(s, cutting.first, ref rmat, x, y, w/2, h);
					Assemble(s, cutting.second, ref rmat, x+w/2, y, w/2, h);
					break;
					
				case Cutting.Cut.Vertical:
					Assemble(s, cutting.first, ref rmat, x, y+h/2, w, h/2);
					Assemble(s, cutting.second, ref rmat, x, y, w, h/2);
					break;
			}
		}
		
		public override string ToString()
		{
			return string.Format("[SampleRate={0}, N={1}, W={2}, Threaded={3}, Coarse={4}", sampleRate,
			                     GetParameter("n"), GetWindowDescription((int) GetParameter("w")), GetParameter("threaded"), GetParameter("coarse"));
		}

		#region Helper Classes
		#region Spectrogram class
		protected class Spectrogram
		{
			public int resolution;
			public int width;
			public double[][] data;

			public Spectrogram(int r, int w)
			{
				resolution = r;
				width = w;
				data = new double[width][];
				for (int i = 0; i < width; ++i) {
					data[i] = new double[resolution];
				}
			}

			public void Dispose()
			{
				for (int i = 0; i < width; ++i) {
					data[i] = null;
				}
				data = null;
			}
			
			public override string ToString()
			{
				return string.Format("Resolution={0}, Width={1}", resolution, width);
			}
		}
		#endregion
		
		#region Spectrograms class
		protected class Spectrograms
		{
			public int minres;
			public int maxres;
			public int n;
			public Spectrogram[] spectrograms;

			public Spectrograms(int mn, int mx, int widthofmax)
			{
				minres = mn;
				maxres = mx;
				
				// The number of resolutions, n, we may obtain depends directly on our choice of L and N:
				// n = log2 (N/L) + 1
				// N = number of points in each DFT
				// L = time advance of the analysis window
				n = Log2(maxres/minres) + 1;
				
				spectrograms = new Spectrogram[n];
				int r = mn;
				for (int i = 0; i < n; ++i)
				{
					spectrograms[i] = new Spectrogram(r, widthofmax * (mx / r));
					r = r * 2;
				}
			}
			
			public void Dispose()
			{
				for (int i = 0; i < n; ++i)
				{
					if (spectrograms[i] != null) {
						spectrograms[i].Dispose();
					}
				}
				spectrograms = null;
			}
			
			public override string ToString()
			{
				return string.Format("[Spectrograms Minres={0}, Maxres={1}, N={2}, Spectrograms={3}]", minres, maxres, n, spectrograms.Length);
			}
		}
		#endregion

		#region Cutting class
		protected class Cutting
		{
			public enum Cut
			{
				Horizontal, // 0
				Vertical, 	// 1
				Finished	// 2
			}
			
			public Cut cut;
			public Cutting first;
			public Cutting second;
			public double cost;
			public double value;
			
			public Cutting allocator;
			
			public void Erase() {
				// do nothing?
			}
			
			public override string ToString()
			{
				return string.Format("[Cut={0}, Cost={1}, Value={2}, First={3}, Second={4}, Allocator={5}]", cut, cost, value, first != null, second != null, allocator != null);
			}
		}
		#endregion
		
		#region CutThread Class
		protected class CutThread
		{
			AdaptiveSpectrogram m_as;
			Spectrograms m_s;
			int m_res;
			int m_x;
			int m_y;
			int m_h;
			Cutting m_result;

			Task task; // for multi-threading
			
			public CutThread(AdaptiveSpectrogram @as)
			{
				m_as = @as;
			}
			
			public void Cut(Spectrograms s, int res, int x, int y, int h)
			{
				m_s = s;
				m_res = res;
				m_x = x;
				m_y = y;
				m_h = h;

				task = new Task(PerformTask);
				task.Start();
				//task = Task.Factory.StartNew(PerformTask);
				// startTask();
			}

			public Cutting Get()
			{
				task.Wait();
				// awaitTask();
				return m_result;
			}

			protected void PerformTask()
			{
				m_result = m_as.Cut(m_s, m_res, m_x, m_y, m_h);
			}
		}
		#endregion

		#region FFTThread Class
		protected class FFTThread
		{
			readonly FFTWindow m_window;
			float[] m_in;
			double[] m_rin;
			double[] m_rout;
			double[] m_iout;
			Spectrograms m_s;
			int m_res;
			int m_w;
			int m_maxwid;

			Task task;
			LomontFFT m_fft;
			
			public FFTThread(int w)
			{
				m_window = new FFTWindow(FFTWindowType.HANNING, w);
				m_w = w;
				m_rin = new double[m_w];
				m_rout = new double[m_w];
				m_iout = new double[m_w];
				//m_fft = new FFTReal(m_w);
				
				m_fft = new LomontFFT();
			}

			public int GetW()
			{
				return m_w;
			}

			public void StartCalculation(float[] timeDomain, ref Spectrograms s, int res, int maxwidth)
			{
				SetParameters(timeDomain, ref s, res, maxwidth);
				//startTask();
				
				task = new Task(PerformTask);
				task.Start();
			}

			public void Await()
			{
				//awaitTask();
				task.Wait();
			}

			public void SetParameters(float[] timeDomain, ref Spectrograms s, int res, int maxwidth)
			{
				m_in = timeDomain;
				m_s = s;
				m_res = res;
				m_maxwid = maxwidth;
			}

			public void PerformTask()
			{
				// perform the fft for each of the chosen window sizes
				// m_maxwid = 2048 (8192)
				// m_w = 512, 1024, 2048 (1024, 2048, 4096, 8192)
				for (int i = 0; i < m_maxwid / m_w; ++i)
				{
					int origin = m_maxwid/4 - m_w/4; // for 50% overlap
					for (int j = 0; j < m_w; ++j)
					{
						int index = origin + i * m_w/2 + j;
						if (index > m_in.Length - 1) break;
						m_rin[j] = m_in[index];
					}
					
					// perform windowing
					m_window.Apply(m_rin);
					
					var fft = new double[m_rin.Length];
					Array.Copy(m_rin, fft, m_rin.Length);
					m_fft.RealFFT(fft, true);

					// fft input will now contain the FFT values
					// r0, r(n/2), r1, i1, r2, i2 ...
					
					// Include Nyquist but not DC
					//m_s.spectrograms[m_res].data[i][0] = Math.Sqrt(fft[0] * fft[0]) / (m_w/2); // DC
					m_s.spectrograms[m_res].data[i][m_w/2-1] = Math.Sqrt(fft[1] * fft[1]) / (m_w/2); // Nyquist
					for (int j = 1; j < m_w/2; ++j)
					{
						double mag = Math.Sqrt(fft[2 * j] * fft[2 * j]
						                       + fft[2 * j + 1] * fft[2 * j + 1]);
						double scaled = mag / (m_w/2);
						m_s.spectrograms[m_res].data[i][j] = scaled;
					}
				}
			}
		}
		#endregion
		#endregion
	}
}